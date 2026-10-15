**Разработать кроссплатформенное десктопное приложение на AvaloniaUI, позволяющее управлять списком задач с сохранением в локальную базу данных через Entity Framework Core (PostgreSQL).
Приложение должно следовать архитектуре MVVM, использовать Dependency Injection, асинхронность и демонстрировать зрелый подход к проектированию и тестируемости.

Функциональные требования
1. Модель задачи (TaskItem)
   public class TaskItem
   {
   public Guid Id { get; set; }
   public string Title { get; set; }      // обязательно, до 100 символов
   public bool IsCompleted { get; set; }
   public DateTime CreatedAt { get; set; }
   }
2. Основной функционал
   Отображение списка задач в виде таблицы
   Возможность:
   Добавить новую задачу (через добавление строки в таблице по нажатию на кнопку Enter)
   Отметить задачу как выполненную/невыполненную (чекбокс)
   Удалить задачу (soft delete — не показывать в списке)
3. UI/UX требования
   Чистый, понятный интерфейс без излишеств
   Адаптивность (окно должно корректно масштабироваться)
   Обратная связь при ошибках (например, если название задачи пустое — показать сообщение)
   Поддержка клавиатурной навигации (по желанию, но плюсом)

Технические требования
Entity Framework Core
Использовать EF Core 8 или 9 с провайдером PostgreSQL
База данных должна храниться в локальном файле (например, tasks.db)
Настроить миграции (Add-Migration InitialCreate, Update-Database)
DbContext должен быть зарегистрирован через scoped lifetime в DI-контейнере
Репозиторий (ITaskRepository) должен инкапсулировать работу с DbSet<TaskItem>
Архитектура
Чёткое разделение на слои:
Domain — сущности и бизнес-логика
Infrastructure — EF Core, репозитории, миграции
Presentation — Avalonia Views + ViewModels
Использовать паттерн MVVM:
View — только XAML + минимальный code-behind
ViewModel — логика, команды (ICommand), привязки
Использовать ReactiveUI или CommunityToolkit.MVVM для упрощения INotifyPropertyChanged
Все операции с БД — асинхронные (async/await)
Прочее
Валидация:
Нельзя сохранить задачу без Title
Отображать ошибки в UI (например, подсветка поля или сообщение)
Обработка ошибок:
Глобальный обработчик исключений (не должно падать при ошибке БД)
Логирование через ILogger<T>
Dependency Injection:
Настроить через Microsoft.Extensions.DependencyInjection
Зарегистрировать MainViewModel, репозиторий, DbContext, логгер
Unit-тесты:
Минимум 2 unit-теста:
Проверка, что задача с пустым Title не сохраняется
Проверка добавления задачи в репозиторий (можно с in-memory PostgreSQL)

Рекомендуемая структура проекта
TaskManager.Desktop/
├── Models/
│   └── TaskItem.cs
├── Services/
│   ├── ITaskRepository.cs
│   └── LiteDbTaskRepository.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   └── AddEditTaskViewModel.cs
├── Views/
│   ├── MainWindow.axaml
│   └── AddEditTaskWindow.axaml
├── App.axaml
├── Program.cs
└── DI / Extensions (настройка сервисов)

TaskManager.Tests/
├── UnitTests/
│   ├── TaskValidationTests.cs
│   ├── RepositoryTests.cs
│   └── MainViewModelTests.cs

Что сдавать?
Ссылка на публичный Git-репозиторий
В README.md:
Как собрать и запустить (dotnet run)
Как применить миграции (или автоматически при старте)
Краткое описание архитектуры
Скриншоты интерфейса (опционально, но приятно)
Указание, какие требования выполнены/не выполнены**
