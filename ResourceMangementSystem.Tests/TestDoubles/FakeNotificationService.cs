using ResourceMangementSystem.Application.Abstractions;
using ResourceMangementSystem.Domain.Entities;

namespace ResourceMangementSystem.Tests.TestDoubles;

internal sealed class FakeNotificationService : INotificationService
{
    public int AllocationNotifications { get; private set; }

    public int ReleaseNotifications { get; private set; }

    public Task NotifyAllocationAsync(Employee employee, Project project, Allocation allocation, CancellationToken cancellationToken = default)
    {
        AllocationNotifications++;
        return Task.CompletedTask;
    }

    public Task NotifyReleaseAsync(Employee employee, Project project, Allocation allocation, CancellationToken cancellationToken = default)
    {
        ReleaseNotifications++;
        return Task.CompletedTask;
    }
}
