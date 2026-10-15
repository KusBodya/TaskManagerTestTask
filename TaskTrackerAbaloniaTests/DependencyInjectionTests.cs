using Microsoft.Extensions.DependencyInjection;
using TaskTrackerAvalonia.Abstractions;
using TaskTrackerAvalonia.Extensions;
using TaskTrackerAvalonia.Infrastructure;
using TaskTrackerAvalonia.ViewModels;

namespace TaskTrackerAbaloniaTests;

public class DependencyInjectionTests
{
    [Test]
    public void AddTaskServices_UsesSeparateDatabaseContextsForEachScope()
    {
        using var services = new ServiceCollection()
            .AddTaskServices("Host=localhost;Database=tests;Username=tests;Password=tests")
            .BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
        using var first = services.CreateScope();
        using var second = services.CreateScope();

        var firstContext = first.ServiceProvider.GetRequiredService<TaskDbContext>();
        Assert.That(first.ServiceProvider.GetRequiredService<TaskDbContext>(), Is.SameAs(firstContext));
        Assert.That(second.ServiceProvider.GetRequiredService<TaskDbContext>(), Is.Not.SameAs(firstContext));
        Assert.That(first.ServiceProvider.GetRequiredService<ITaskRepository>(), Is.TypeOf<EfTaskRepository>());
        var viewModel = services.GetRequiredService<MainViewModel>();
        Assert.That(services.GetRequiredService<MainViewModel>(), Is.SameAs(viewModel));
        Assert.Throws<InvalidOperationException>(() => services.GetRequiredService<TaskDbContext>());
    }
}
