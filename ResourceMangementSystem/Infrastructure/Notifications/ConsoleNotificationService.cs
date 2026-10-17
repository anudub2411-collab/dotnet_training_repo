using ResourceMangementSystem.Application.Abstractions;
using ResourceMangementSystem.Domain.Entities;

namespace ResourceMangementSystem.Infrastructure.Notifications;

public sealed class ConsoleNotificationService : INotificationService
{
    public Task NotifyAllocationAsync(Employee employee, Project project, Allocation allocation, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[NOTIFY] Allocated {employee.Name} to {project.Name} ({allocation.AllocationPercentage}%) from {allocation.StartDate:yyyy-MM-dd} to {allocation.EndDate:yyyy-MM-dd}");
        return Task.CompletedTask;
    }

    public Task NotifyReleaseAsync(Employee employee, Project project, Allocation allocation, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[NOTIFY] Released {employee.Name} from {project.Name} on {allocation.EndDate:yyyy-MM-dd}");
        return Task.CompletedTask;
    }
}
