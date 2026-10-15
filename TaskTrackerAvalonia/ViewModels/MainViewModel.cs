using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskTrackerAvalonia.Abstractions;
using TaskTrackerAvalonia.Models;

namespace TaskTrackerAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MainViewModel> _logger;

    public ObservableCollection<TaskItem> Tasks { get; } = new();

    [ObservableProperty]
    private string? errorMessage;

    public MainViewModel(
        IServiceScopeFactory scopeFactory,
        ILogger<MainViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task LoadAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var items = await repository.GetAllAsync();

            Tasks.Clear();
            foreach (var item in items)
                Tasks.Add(item);

            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось загрузить задачи");
            ErrorMessage = "Не удалось загрузить задачи. Проверь подключение к базе.";
        }
    }
}