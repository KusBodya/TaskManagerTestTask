using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTrackerAvalonia.Models;

namespace TaskTrackerAvalonia.Abstractions;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync();
    Task AddAsync(TaskItem task);
    Task SetCompletedAsync(Guid id, bool isCompleted);
    Task DeleteAsync(Guid id);
}

