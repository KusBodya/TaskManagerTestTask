using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTrackerAvalonia.Abstractions;
using TaskTrackerAvalonia.Models;

namespace TaskTrackerAvalonia.Infrastructure;

public class EfTaskRepository : ITaskRepository
{
    public Task<List<TaskItem>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(TaskItem task)
    {
        throw new NotImplementedException();
    }

    public Task SetCompletedAsync(Guid id, bool isCompleted)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}