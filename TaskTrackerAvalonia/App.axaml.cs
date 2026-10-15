using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
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
            DotNetEnv.Env.TraversePath().Load();

            static string Required(string name)
            {
                return Environment.GetEnvironmentVariable(name)
                       ?? throw new InvalidOperationException(
                           $"Не задано значение {name} в .env");
            }

            var connectionString = new NpgsqlConnectionStringBuilder
            {
                Host = "localhost",
                Port = 5433,
                Database = Required("POSTGRES_DB"),
                Username = Required("POSTGRES_USER"),
                Password = Required("POSTGRES_PASSWORD")
            }.ConnectionString;

            var services = new ServiceCollection()
                .AddTaskServices(connectionString)
                .BuildServiceProvider(validateScopes: true);

            var viewModel = services.GetRequiredService<MainViewModel>();

            var window = new MainWindow
            {
                DataContext = viewModel
            };

            window.Opened += async (_, _) => await viewModel.LoadAsync();
            desktop.MainWindow = window;

            desktop.Exit += (_, _) => services.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}