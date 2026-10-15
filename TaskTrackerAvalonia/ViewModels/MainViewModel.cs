using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskTrackerAvalonia.Abstractions;
using TaskTrackerAvalonia.Models;

namespace TaskTrackerAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MainViewModel> _logger;
    private bool _databaseInitialized;

    public ObservableCollection<TaskRowViewModel> Tasks { get; } = new();

    [ObservableProperty]
    private string newTaskTitle = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIdle))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddTaskCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleCompletedCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTaskCommand))]
    private bool isBusy;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsIdle => !IsBusy;
    public bool IsEmpty => !IsBusy && Tasks.Count == 0;
    public string TaskCountText => $"Задач: {Tasks.Count}";

    public MainViewModel(
        IServiceScopeFactory scopeFactory,
        ILogger<MainViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        
        Tasks.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(TaskCountText));
        };
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            if (!_databaseInitialized)
            {
                var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
                await initializer.InitializeAsync();
                _databaseInitialized = true;
            }

            var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var items = await repository.GetAllAsync();

            Tasks.Clear();
            foreach (var item in items)
                Tasks.Add(new TaskRowViewModel(item));
        }
        catch (Exception ex)
        {
            ReportError(ex, "Не удалось загрузить задачи. Проверь подключение к базе и нажми «Обновить».");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    public async Task AddTaskAsync()
    {
        if (IsBusy)
            return;

        var task = new TaskItem { Title = NewTaskTitle.Trim() };
        try
        {
            Validator.ValidateObject(task, new ValidationContext(task), validateAllProperties: true);
        }
        catch (ValidationException ex)
        {
            ErrorMessage = ex.Message;
            return;
        }

        IsBusy = true;
        
        ErrorMessage = null;

        
        
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            await repository.AddAsync(task);

            Tasks.Add(new TaskRowViewModel(task));
            NewTaskTitle = string.Empty;
        }
        catch (Exception ex)
        {
            ReportError(ex, "Не удалось добавить задачу. Проверь подключение к базе и попробуй ещё раз.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    public async Task ToggleCompletedAsync(TaskRowViewModel? task)
    {
        if (task is null || IsBusy)
            return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            await repository.SetCompletedAsync(task.Id, !task.IsCompleted);
            task.IsCompleted = !task.IsCompleted;
        }
        catch (Exception ex)
        {
            ReportError(ex, "Не удалось изменить статус задачи. Попробуй ещё раз.");
        }
        finally
        {
            task.RefreshCompletion();
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    public async Task DeleteTaskAsync(TaskRowViewModel? task)
    {
        if (task is null || IsBusy)
            return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            await repository.DeleteAsync(task.Id);
            Tasks.Remove(task);
        }
        catch (Exception ex)
        {
            ReportError(ex, "Не удалось удалить задачу. Попробуй ещё раз.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void ReportError(Exception exception, string message)
    {
        _logger.LogError(exception, "{Message}", message);
        ErrorMessage = message;
    }
}
