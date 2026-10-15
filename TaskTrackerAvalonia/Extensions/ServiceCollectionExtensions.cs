using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskTrackerAvalonia.Abstractions;
using TaskTrackerAvalonia.Infrastructure;
using TaskTrackerAvalonia.ViewModels;

namespace TaskTrackerAvalonia.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaskServices(
        this IServiceCollection services,
        string? connectionString = null)
    {
        services.AddDbContext<TaskDbContext>(
            options => options.UseNpgsql(connectionString ?? DatabaseSettings.GetConnectionString()));

        services.AddScoped<ITaskRepository, EfTaskRepository>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        services.AddSingleton<MainViewModel>();
        services.AddLogging(logging => logging.AddConsole());

        return services;
    }
}
