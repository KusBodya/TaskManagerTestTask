using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskTrackerAvalonia.Infrastructure;
using TaskTrackerAvalonia.Models;

namespace TaskTrackerAbaloniaTests;

public class RepositoryTests
{
    private SqliteConnection _connection = null!;
    private DbContextOptions<TaskDbContext> _options = null!;

    [SetUp]
    public async Task Setup()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();
        _options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseSqlite(_connection)
            .Options;

        await using var db = new TaskDbContext(_options);
        await db.Database.EnsureCreatedAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _connection.DisposeAsync();
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public async Task AddAsync_EmptyTitle_DoesNotSave(string? title)
    {
        await using var db = new TaskDbContext(_options);
        var repository = new EfTaskRepository(db);

        Assert.ThrowsAsync<ValidationException>(() =>
            repository.AddAsync(new TaskItem { Title = title! }));

        Assert.That(await db.Tasks.CountAsync(), Is.Zero);
        Assert.That(db.ChangeTracker.Entries<TaskItem>(), Is.Empty);
    }

    [Test]
    public async Task AddAsync_TitleTooLong_DoesNotSave()
    {
        await using var db = new TaskDbContext(_options);
        var repository = new EfTaskRepository(db);

        Assert.ThrowsAsync<ValidationException>(() =>
            repository.AddAsync(new TaskItem { Title = new string('a', 101) }));

        Assert.That(await db.Tasks.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task AddAsync_ValidTask_PersistsBetweenContexts()
    {
        var task = new TaskItem { Title = new string('a', 100) };
        await using (var db = new TaskDbContext(_options))
            await new EfTaskRepository(db).AddAsync(task);

        await using var readDb = new TaskDbContext(_options);
        var tasks = await new EfTaskRepository(readDb).GetAllAsync();

        Assert.That(tasks, Has.Count.EqualTo(1));
        Assert.That(tasks[0].Id, Is.EqualTo(task.Id));
        Assert.That(tasks[0].Title, Is.EqualTo(task.Title));
        Assert.That(tasks[0].IsCompleted, Is.False);
        Assert.That(tasks[0].CreatedAt, Is.EqualTo(task.CreatedAt));
    }

    [Test]
    public async Task SetCompletedAsync_CanCompleteAndReopenTask()
    {
        var task = new TaskItem { Title = "Проверить статус" };
        await using (var db = new TaskDbContext(_options))
            await new EfTaskRepository(db).AddAsync(task);

        foreach (var completed in new[] { true, false })
        {
            await using (var db = new TaskDbContext(_options))
                await new EfTaskRepository(db).SetCompletedAsync(task.Id, completed);

            await using var readDb = new TaskDbContext(_options);
            Assert.That((await readDb.Tasks.SingleAsync()).IsCompleted, Is.EqualTo(completed));
        }
    }

    [Test]
    public async Task DeleteAsync_HidesTaskButKeepsDatabaseRow()
    {
        var task = new TaskItem { Title = "Удалить задачу" };
        await using (var db = new TaskDbContext(_options))
        {
            var repository = new EfTaskRepository(db);
            await repository.AddAsync(task);
            await repository.DeleteAsync(task.Id);
        }

        await using var readDb = new TaskDbContext(_options);
        Assert.That(await new EfTaskRepository(readDb).GetAllAsync(), Is.Empty);
        var deleted = await readDb.Tasks.IgnoreQueryFilters().SingleAsync();
        Assert.That(deleted.Id, Is.EqualTo(task.Id));
        Assert.That(deleted.IsDeleted, Is.True);

        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new EfTaskRepository(readDb).SetCompletedAsync(task.Id, true));
    }

    [Test]
    public async Task GetAllAsync_ReturnsTasksInCreationOrder()
    {
        await using var db = new TaskDbContext(_options);
        var repository = new EfTaskRepository(db);
        await repository.AddAsync(new TaskItem { Title = "Позже", CreatedAt = DateTime.UtcNow });
        await repository.AddAsync(new TaskItem { Title = "Раньше", CreatedAt = DateTime.UtcNow.AddDays(-1) });

        var tasks = await repository.GetAllAsync();

        Assert.That(tasks.Select(task => task.Title), Is.EqualTo(new[] { "Раньше", "Позже" }));
    }
}
