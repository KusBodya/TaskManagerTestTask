using System.Threading.Tasks;

namespace TaskTrackerAvalonia.Abstractions;

public interface IDatabaseInitializer
{
    Task InitializeAsync();
}
