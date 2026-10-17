namespace ResourceMangementSystem.Domain.Entities;

public sealed class Allocation
{
    public Guid Id { get; private set; }

    public Guid EmployeeId { get; private set; }

    public Guid ProjectId { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public int AllocationPercentage { get; private set; }

    public bool IsActive { get; private set; }

    public Allocation(Guid id, Guid employeeId, Guid projectId, DateOnly startDate, DateOnly endDate, int allocationPercentage, bool isActive = true)
    {
        if (employeeId == Guid.Empty)
        {
            throw new ArgumentException("Employee ID is required.", nameof(employeeId));
        }

        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project ID is required.", nameof(projectId));
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        }

        if (allocationPercentage is <= 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(allocationPercentage), "Allocation must be between 1 and 100.");
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        EmployeeId = employeeId;
        ProjectId = projectId;
        StartDate = startDate;
        EndDate = endDate;
        AllocationPercentage = allocationPercentage;
        IsActive = isActive;
    }

    public void Release(DateOnly releaseDate)
    {
        if (releaseDate < StartDate)
        {
            throw new ArgumentException("Release date cannot be before allocation start date.", nameof(releaseDate));
        }

        EndDate = releaseDate < EndDate ? releaseDate : EndDate;
        IsActive = false;
    }

    public bool Overlaps(DateOnly startDate, DateOnly endDate)
    {
        return StartDate <= endDate && EndDate >= startDate;
    }
}
