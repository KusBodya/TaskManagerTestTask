using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskTrackerAvalonia.Infrastructure;

public class TaskDbContextFactory : IDesignTimeDbContextFactory<TaskDbContext>
{
    public TaskDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseNpgsql(DatabaseSettings.GetConnectionString())
            .Options;

        return new TaskDbContext(options);
    }
}
