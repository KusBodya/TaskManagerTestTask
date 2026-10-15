using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskTrackerAvalonia.Extensions;
using TaskTrackerAvalonia.ViewModels;
using TaskTrackerAvalonia.Views;

namespace TaskTrackerAvalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection()
                .AddTaskServices()
                .BuildServiceProvider(validateScopes: true);

            var viewModel = services.GetRequiredService<MainViewModel>();
            var logger = services.GetRequiredService<ILogger<App>>();

            void OnUiException(object sender, DispatcherUnhandledExceptionEventArgs args)
            {
                viewModel.ReportError(args.Exception, "Произошла ошибка. Попробуй повторить действие.");
                args.Handled = true;
            }

            void OnUnobservedException(object? sender, UnobservedTaskExceptionEventArgs args)
            {
                logger.LogError(args.Exception, "Необработанная ошибка фоновой задачи");
                args.SetObserved();
                Dispatcher.UIThread.Post(() =>
                    viewModel.ErrorMessage = "Произошла ошибка в фоне. Попробуй повторить действие.");
            }

            void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
            {
                logger.LogCritical(args.ExceptionObject as Exception, "Критическая ошибка приложения");
            }

            Dispatcher.UIThread.UnhandledException += OnUiException;
            TaskScheduler.UnobservedTaskException += OnUnobservedException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            var window = new MainWindow
            {
                DataContext = viewModel
            };

            window.Opened += async (_, _) => await viewModel.LoadAsync();
            desktop.MainWindow = window;

            desktop.Exit += (_, _) =>
            {
                Dispatcher.UIThread.UnhandledException -= OnUiException;
                TaskScheduler.UnobservedTaskException -= OnUnobservedException;
                AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
                services.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
