using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace TaskTrackerAvalonia.Infrastructure;

public class TaskDbContextFactory : IDesignTimeDbContextFactory<TaskDbContext>
{
    public TaskDbContext CreateDbContext(string[] args)
    {
        DotNetEnv.Env.TraversePath().Load();

        static string Required(string name) =>
            Environment.GetEnvironmentVariable(name)
            ?? throw new InvalidOperationException($"Не задано {name} в .env");

        var connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = "localhost",
            Port = 5433,
            Database = Required("POSTGRES_DB"),
            Username = Required("POSTGRES_USER"),
            Password = Required("POSTGRES_PASSWORD")
        }.ConnectionString;

        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TaskDbContext(options);
    }
}