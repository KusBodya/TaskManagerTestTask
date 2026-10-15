using Microsoft.EntityFrameworkCore;
using TaskTrackerAvalonia.Models;

namespace TaskTrackerAvalonia.Infrastructure;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>()
            .HasQueryFilter(task => !task.IsDeleted);
    }
}
