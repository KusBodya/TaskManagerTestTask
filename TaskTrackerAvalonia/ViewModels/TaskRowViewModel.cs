using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TaskTrackerAvalonia.Models;

namespace TaskTrackerAvalonia.ViewModels;

public partial class TaskRowViewModel : ViewModelBase
{
    public Guid Id { get; }
    public string Title { get; }
    public DateTime CreatedAt { get; }
    public string CreatedAtText => CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

    [ObservableProperty]
    private bool isCompleted;

    public TaskRowViewModel(TaskItem task)
    {
        Id = task.Id;
        Title = task.Title;
        CreatedAt = task.CreatedAt;
        isCompleted = task.IsCompleted;
    }

    public void RefreshCompletion()
    {
        OnPropertyChanged(nameof(IsCompleted));
    }
}
