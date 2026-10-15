using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskTrackerAvalonia.Abstractions;
using TaskTrackerAvalonia.Models;

namespace TaskTrackerAvalonia.Infrastructure;

public class EfTaskRepository : ITaskRepository
{
    private readonly TaskDbContext _db;

    public EfTaskRepository(TaskDbContext db)
    {
        _db = db;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _db.Tasks
            .AsNoTracking()
            .OrderBy(task => task.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(TaskItem task)
    {
        Validator.ValidateObject(
            task,
            new ValidationContext(task),
            validateAllProperties: true);

        await _db.Tasks.AddAsync(task);
        await _db.SaveChangesAsync();
    }

    public async Task SetCompletedAsync(Guid id, bool isCompleted)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(task => task.Id == id)
                   ?? throw new KeyNotFoundException("Тасочка не найдена.");

        task.IsCompleted = isCompleted;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(task => task.Id == id)
                   ?? throw new KeyNotFoundException("Тасочка не найдена.");

        task.IsDeleted = true;
        await _db.SaveChangesAsync();
    }
}