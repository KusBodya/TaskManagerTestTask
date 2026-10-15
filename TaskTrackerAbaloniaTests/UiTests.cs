using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TaskTrackerAvalonia;
using TaskTrackerAvalonia.Abstractions;
using TaskTrackerAvalonia.Models;
using TaskTrackerAvalonia.ViewModels;
using TaskTrackerAvalonia.Views;

[assembly: AvaloniaTestApplication(typeof(TaskTrackerAbaloniaTests.TestAppBuilder))]

namespace TaskTrackerAbaloniaTests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UseSkia()
            .WithInterFont()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
    }
}

public class UiTests
{
    private ServiceProvider _services = null!;
    private MainWindow _window = null!;
    private MainViewModel _viewModel = null!;
    private TestTaskRepository _repository = null!;

    private async Task OpenWindowAsync()
    {
        _repository = new TestTaskRepository();
        _services = new ServiceCollection()
            .AddScoped<ITaskRepository>(_ => _repository)
            .AddScoped<IDatabaseInitializer, TestDatabaseInitializer>()
            .BuildServiceProvider(validateScopes: true);
        _viewModel = new MainViewModel(
            _services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<MainViewModel>.Instance);
        _window = new MainWindow { DataContext = _viewModel };
        _window.Show();
        await _viewModel.LoadAsync();
        Dispatcher.UIThread.RunJobs();
    }

    [TearDown]
    public void TearDown()
    {
        _window?.Close();
        _services?.Dispose();
    }

    [AvaloniaTest]
    public async Task Enter_AddsTaskFromInput()
    {
        await OpenWindowAsync();
        var input = _window.FindControl<TextBox>("NewTaskInput")!;
        input.Focus();
        _window.KeyTextInput("Задача через Enter");
        _window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
        _window.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
        Dispatcher.UIThread.RunJobs();

        Assert.That(_repository.Items.Single().Title, Is.EqualTo("Задача через Enter"));
        Assert.That(input.Text, Is.Empty);
        Assert.That(_window.FindControl<DataGrid>("TaskTable")!.ItemsSource.Cast<TaskRowViewModel>().Count(), Is.EqualTo(1));
    }

    [AvaloniaTest]
    public async Task Checkbox_ClickSavesStatusAndRestoresItOnFailure()
    {
        await OpenWindowAsync();
        _viewModel.NewTaskTitle = "Проверить чекбокс";
        await _viewModel.AddTaskAsync();
        Dispatcher.UIThread.RunJobs();
        var checkBox = _window.GetVisualDescendants().OfType<CheckBox>().Single();

        Click(checkBox);
        Assert.That(_repository.Items.Single().IsCompleted, Is.True);
        Assert.That(checkBox.IsChecked, Is.True);

        _repository.Error = new InvalidOperationException("База недоступна");
        Click(checkBox);
        Assert.That(_repository.Items.Single().IsCompleted, Is.True);
        Assert.That(checkBox.IsChecked, Is.True);
        Assert.That(_viewModel.HasError, Is.True);
    }

    [AvaloniaTest]
    public async Task DeleteButton_RemovesTask()
    {
        await OpenWindowAsync();
        _viewModel.NewTaskTitle = "Удалить через кнопку";
        await _viewModel.AddTaskAsync();
        Dispatcher.UIThread.RunJobs();
        var button = _window.GetVisualDescendants().OfType<Button>()
            .Single(control => Equals(control.Content, "Удалить"));

        Click(button);

        Assert.That(_viewModel.Tasks, Is.Empty);
        Assert.That(_repository.Items.Single().IsDeleted, Is.True);
    }

    [AvaloniaTest]
    public async Task DeleteKey_RemovesSelectedTask()
    {
        await OpenWindowAsync();
        _viewModel.NewTaskTitle = "Удалить с клавиатуры";
        await _viewModel.AddTaskAsync();
        Dispatcher.UIThread.RunJobs();
        var table = _window.FindControl<DataGrid>("TaskTable")!;
        table.SelectedItem = _viewModel.Tasks.Single();
        table.Focus();

        _window.KeyPress(Key.Delete, RawInputModifiers.None, PhysicalKey.Delete, null);
        _window.KeyRelease(Key.Delete, RawInputModifiers.None, PhysicalKey.Delete, null);
        Dispatcher.UIThread.RunJobs();

        Assert.That(_viewModel.Tasks, Is.Empty);
    }

    [AvaloniaTest]
    public async Task Window_RendersTableAtMinimumSize()
    {
        await OpenWindowAsync();
        _viewModel.NewTaskTitle = "Проверить интерфейс";
        await _viewModel.AddTaskAsync();
        Dispatcher.UIThread.RunJobs();
        var screenshotPath = Environment.GetEnvironmentVariable("TASKTRACKER_SCREENSHOT");
        if (!string.IsNullOrEmpty(screenshotPath))
        {
            using var screenshot = _window.CaptureRenderedFrame();
            screenshot!.Save(screenshotPath, PngBitmapEncoderOptions.Default);
        }

        _window.Width = _window.MinWidth;
        _window.Height = _window.MinHeight;
        Dispatcher.UIThread.RunJobs();

        using var frame = _window.CaptureRenderedFrame();
        Assert.That(frame, Is.Not.Null);
        Assert.That(_window.FindControl<DataGrid>("TaskTable")!.Bounds.Height, Is.GreaterThan(100));
        Assert.That(_window.GetVisualDescendants().OfType<CheckBox>().Single().IsVisible, Is.True);
    }

    private void Click(Control control)
    {
        var point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), _window)!.Value;
        _window.MouseDown(point, MouseButton.Left);
        _window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }
}
