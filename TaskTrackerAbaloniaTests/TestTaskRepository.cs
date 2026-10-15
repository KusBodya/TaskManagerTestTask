using TaskTrackerAvalonia.Abstractions;
using TaskTrackerAvalonia.Models;

namespace TaskTrackerAbaloniaTests;

public class TestTaskRepository : ITaskRepository
{
    public List<TaskItem> Items { get; } = new();
    public Exception? Error { get; set; }
    public TaskCompletionSource? AddCompletion { get; set; }
    public int AddCalls { get; private set; }

    public Task<List<TaskItem>> GetAllAsync()
    {
        ThrowIfFailed();
        return Task.FromResult(Items.Where(task => !task.IsDeleted).ToList());
    }

    public async Task AddAsync(TaskItem task)
    {
        AddCalls++;
        ThrowIfFailed();
        if (AddCompletion is not null)
            await AddCompletion.Task;

        Items.Add(task);
    }

    public Task SetCompletedAsync(Guid id, bool isCompleted)
    {
        ThrowIfFailed();
        Items.Single(task => task.Id == id).IsCompleted = isCompleted;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        ThrowIfFailed();
        Items.Single(task => task.Id == id).IsDeleted = true;
        return Task.CompletedTask;
    }

    private void ThrowIfFailed()
    {
        if (Error is not null)
            throw Error;
    }
}

public class TestDatabaseInitializer : IDatabaseInitializer
{
    public int Calls { get; private set; }
    public Exception? Error { get; set; }

    public Task InitializeAsync()
    {
        Calls++;
        if (Error is not null)
            throw Error;

        return Task.CompletedTask;
    }
}
