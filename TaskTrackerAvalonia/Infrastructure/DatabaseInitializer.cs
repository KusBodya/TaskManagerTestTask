using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskTrackerAvalonia.Abstractions;

namespace TaskTrackerAvalonia.Infrastructure;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly TaskDbContext _db;

    public DatabaseInitializer(TaskDbContext db)
    {
        _db = db;
    }

    public async Task InitializeAsync()
    {
        await _db.Database.MigrateAsync();
    }
}
