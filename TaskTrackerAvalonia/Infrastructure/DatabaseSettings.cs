using System;
using Npgsql;

namespace TaskTrackerAvalonia.Infrastructure;

public static class DatabaseSettings
{
    public static string GetConnectionString()
    {
        DotNetEnv.Env.TraversePath().NoClobber().Load();

        return new NpgsqlConnectionStringBuilder
        {
            Host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost",
            Port = int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5433"),
            Database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "tasktracker",
            Username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "tasktracker",
            Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "tasktracker",
            Timeout = 5,
            CommandTimeout = 15
        }.ConnectionString;
    }
}
