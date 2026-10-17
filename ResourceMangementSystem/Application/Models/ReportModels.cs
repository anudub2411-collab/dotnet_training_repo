namespace ResourceMangementSystem.Application.Models;

public sealed class BenchReportItem
{
    public Guid EmployeeId { get; init; }

    public string EmployeeName { get; init; } = string.Empty;

    public string Department { get; init; } = string.Empty;

    public int AllocationPercentage { get; init; }
}

public sealed class ProjectStaffingSummaryItem
{
    public Guid ProjectId { get; init; }

    public string ProjectName { get; init; } = string.Empty;

    public string Client { get; init; } = string.Empty;

    public int ActiveAllocationCount { get; init; }

    public int TotalAllocatedPercentage { get; init; }

    public int UniqueEmployees { get; init; }
}

public sealed class DepartmentUtilizationItem
{
    public string Department { get; init; } = string.Empty;

    public int EmployeeCount { get; init; }

    public decimal AverageUtilizationPercentage { get; init; }
}
