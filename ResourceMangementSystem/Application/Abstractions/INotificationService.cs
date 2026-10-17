using ResourceMangementSystem.Domain.Entities;

namespace ResourceMangementSystem.Application.Abstractions;

public interface INotificationService
{
    Task NotifyAllocationAsync(Employee employee, Project project, Allocation allocation, CancellationToken cancellationToken = default);

    Task NotifyReleaseAsync(Employee employee, Project project, Allocation allocation, CancellationToken cancellationToken = default);
}
