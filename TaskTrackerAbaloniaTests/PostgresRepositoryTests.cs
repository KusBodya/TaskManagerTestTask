using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskTrackerAvalonia.Infrastructure;
using TaskTrackerAvalonia.Models;

namespace TaskTrackerAbaloniaTests;

public class PostgresRepositoryTests
{
    [Test]
    [Category("PostgreSQL")]
    public async Task MigrationsAndRepository_WorkWithPostgres()
    {
        if (Environment.GetEnvironmentVariable("TASKTRACKER_POSTGRES_TESTS") != "1")
            Assert.Ignore("Для проверки PostgreSQL задай TASKTRACKER_POSTGRES_TESTS=1 и запусти локальную БД.");

        var settings = new NpgsqlConnectionStringBuilder(DatabaseSettings.GetConnectionString());
        var databaseName = "tasktracker_tests_" + Guid.NewGuid().ToString("N");
        var adminSettings = new NpgsqlConnectionStringBuilder(settings.ConnectionString)
        {
            Database = "postgres",
            Pooling = false
        };

        await using var admin = new NpgsqlConnection(adminSettings.ConnectionString);
        await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", admin))
            await create.ExecuteNonQueryAsync();

        try
        {
            settings.Database = databaseName;
            settings.Pooling = false;
            var options = new DbContextOptionsBuilder<TaskDbContext>()
                .UseNpgsql(settings.ConnectionString)
                .Options;

            await using var db = new TaskDbContext(options);
            var initializer = new DatabaseInitializer(db);
            await initializer.InitializeAsync();
            await initializer.InitializeAsync();

            Assert.That(await db.Database.GetAppliedMigrationsAsync(), Is.Not.Empty);
            Assert.That(await db.Database.GetPendingMigrationsAsync(), Is.Empty);
            Assert.That(db.Database.HasPendingModelChanges(), Is.False);

            var repository = new EfTaskRepository(db);
            var task = new TaskItem { Title = "Проверить PostgreSQL" };
            await repository.AddAsync(task);
            db.ChangeTracker.Clear();

            var saved = (await repository.GetAllAsync()).Single();
            Assert.That(saved.Id, Is.EqualTo(task.Id));
            Assert.That(saved.Title, Is.EqualTo(task.Title));
            Assert.That(saved.CreatedAt.Kind, Is.EqualTo(DateTimeKind.Utc));

            await repository.SetCompletedAsync(task.Id, true);
            db.ChangeTracker.Clear();
            Assert.That((await repository.GetAllAsync()).Single().IsCompleted, Is.True);

            await repository.DeleteAsync(task.Id);
            db.ChangeTracker.Clear();
            Assert.That(await repository.GetAllAsync(), Is.Empty);
            Assert.That((await db.Tasks.IgnoreQueryFilters().SingleAsync()).IsDeleted, Is.True);
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{databaseName}\" WITH (FORCE)", admin);
            await drop.ExecuteNonQueryAsync();
        }
    }
}
