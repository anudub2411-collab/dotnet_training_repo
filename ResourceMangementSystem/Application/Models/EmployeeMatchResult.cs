namespace ResourceMangementSystem.Application.Models;

public sealed class EmployeeMatchResult
{
    public Guid EmployeeId { get; init; }

    public string EmployeeName { get; init; } = string.Empty;

    public string Department { get; init; } = string.Empty;

    public int CurrentAllocationPercentage { get; init; }

    public IReadOnlyCollection<string> MatchingSkills { get; init; } = [];
}
