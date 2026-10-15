using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TaskTrackerAvalonia.Abstractions;
using TaskTrackerAvalonia.Models;
using TaskTrackerAvalonia.ViewModels;

namespace TaskTrackerAbaloniaTests;

public class MainViewModelTests
{
    private ServiceProvider _services = null!;
    private TestTaskRepository _repository = null!;
    private TestDatabaseInitializer _initializer = null!;
    private MainViewModel _viewModel = null!;

    [SetUp]
    public void Setup()
    {
        _repository = new TestTaskRepository();
        _initializer = new TestDatabaseInitializer();
        _services = new ServiceCollection()
            .AddScoped<ITaskRepository>(_ => _repository)
            .AddScoped<IDatabaseInitializer>(_ => _initializer)
            .BuildServiceProvider(validateScopes: true);
        _viewModel = new MainViewModel(
            _services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<MainViewModel>.Instance);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _services.DisposeAsync();
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("\t\n")]
    public async Task AddTaskAsync_EmptyTitle_ShowsErrorWithoutCallingRepository(string title)
    {
        _viewModel.NewTaskTitle = title;

        await _viewModel.AddTaskAsync();

        Assert.That(_repository.AddCalls, Is.Zero);
        Assert.That(_viewModel.Tasks, Is.Empty);
        Assert.That(_viewModel.HasError, Is.True);
    }

    [Test]
    public async Task AddTaskAsync_TitleTooLong_ShowsError()
    {
        _viewModel.NewTaskTitle = new string('a', 101);

        await _viewModel.AddTaskAsync();

        Assert.That(_repository.AddCalls, Is.Zero);
        Assert.That(_viewModel.HasError, Is.True);
    }

    [Test]
    public async Task AddTaskAsync_TrimsTitleAndClearsInputAfterSaving()
    {
        _viewModel.NewTaskTitle = "  Написать тесты  ";

        await _viewModel.AddTaskAsync();

        Assert.That(_repository.Items.Single().Title, Is.EqualTo("Написать тесты"));
        Assert.That(_viewModel.Tasks.Single().Id, Is.EqualTo(_repository.Items.Single().Id));
        Assert.That(_viewModel.NewTaskTitle, Is.Empty);
        Assert.That(_viewModel.HasError, Is.False);
    }

    [Test]
    public async Task AddTaskAsync_DatabaseFailure_KeepsInputAndAllowsRetry()
    {
        _viewModel.NewTaskTitle = "Не потерять текст";
        _repository.Error = new InvalidOperationException("База недоступна");

        await _viewModel.AddTaskAsync();

        Assert.That(_viewModel.NewTaskTitle, Is.EqualTo("Не потерять текст"));
        Assert.That(_viewModel.Tasks, Is.Empty);
        Assert.That(_viewModel.HasError, Is.True);
        Assert.That(_viewModel.IsBusy, Is.False);

        _repository.Error = null;
        await _viewModel.AddTaskAsync();

        Assert.That(_viewModel.Tasks, Has.Count.EqualTo(1));
        Assert.That(_viewModel.HasError, Is.False);
    }

    [Test]
    public async Task AddTaskAsync_WhileSaving_PreventsDuplicateSubmission()
    {
        _repository.AddCompletion = new TaskCompletionSource();
        _viewModel.NewTaskTitle = "Одна задача";

        var save = _viewModel.AddTaskAsync();
        Assert.That(_viewModel.IsBusy, Is.True);
        Assert.That(_viewModel.AddTaskCommand.CanExecute(null), Is.False);
        Assert.That(_viewModel.LoadCommand.CanExecute(null), Is.False);

        await _viewModel.AddTaskAsync();
        _repository.AddCompletion.SetResult();
        await save;

        Assert.That(_repository.AddCalls, Is.EqualTo(1));
        Assert.That(_viewModel.Tasks, Has.Count.EqualTo(1));
        Assert.That(_viewModel.IsBusy, Is.False);
    }

    [Test]
    public async Task LoadAsync_FailedMigration_CanRetryInitialization()
    {
        _initializer.Error = new InvalidOperationException("База недоступна");
        await _viewModel.LoadAsync();
        Assert.That(_viewModel.HasError, Is.True);

        _initializer.Error = null;
        _repository.Items.Add(new TaskItem { Title = "Загрузить" });
        await _viewModel.LoadAsync();
        await _viewModel.LoadAsync();

        Assert.That(_initializer.Calls, Is.EqualTo(2));
        Assert.That(_viewModel.Tasks, Has.Count.EqualTo(1));
        Assert.That(_viewModel.HasError, Is.False);
    }

    [Test]
    public async Task LoadAsync_DatabaseFailure_KeepsExistingRows()
    {
        _repository.Items.Add(new TaskItem { Title = "Сохранённая задача" });
        await _viewModel.LoadAsync();
        _repository.Error = new InvalidOperationException("База недоступна");

        await _viewModel.LoadAsync();

        Assert.That(_viewModel.Tasks, Has.Count.EqualTo(1));
        Assert.That(_viewModel.HasError, Is.True);
    }

    [Test]
    public async Task ToggleCompletedAsync_DatabaseFailure_KeepsPreviousStatus()
    {
        _repository.Items.Add(new TaskItem { Title = "Не менять статус" });
        await _viewModel.LoadAsync();
        var row = _viewModel.Tasks.Single();
        _repository.Error = new InvalidOperationException("База недоступна");

        await _viewModel.ToggleCompletedAsync(row);

        Assert.That(row.IsCompleted, Is.False);
        Assert.That(_viewModel.HasError, Is.True);
        Assert.That(_viewModel.IsBusy, Is.False);
    }

    [Test]
    public async Task ToggleCompletedAsync_UpdatesRowAfterSaving()
    {
        _repository.Items.Add(new TaskItem { Title = "Выполнить" });
        await _viewModel.LoadAsync();
        var row = _viewModel.Tasks.Single();

        await _viewModel.ToggleCompletedAsync(row);
        Assert.That(row.IsCompleted, Is.True);
        Assert.That(_repository.Items.Single().IsCompleted, Is.True);

        await _viewModel.ToggleCompletedAsync(row);
        Assert.That(row.IsCompleted, Is.False);
    }

    [Test]
    public async Task DeleteTaskAsync_DatabaseFailure_KeepsRow()
    {
        _repository.Items.Add(new TaskItem { Title = "Не удалять при ошибке" });
        await _viewModel.LoadAsync();
        _repository.Error = new InvalidOperationException("База недоступна");

        await _viewModel.DeleteTaskAsync(_viewModel.Tasks.Single());

        Assert.That(_viewModel.Tasks, Has.Count.EqualTo(1));
        Assert.That(_viewModel.HasError, Is.True);

        _repository.Error = null;
        await _viewModel.DeleteTaskAsync(_viewModel.Tasks.Single());
        Assert.That(_viewModel.Tasks, Is.Empty);
        Assert.That(_repository.Items.Single().IsDeleted, Is.True);
    }
}
