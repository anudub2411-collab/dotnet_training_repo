using ResourceMangementSystem.Application.Models;
using ResourceMangementSystem.Domain.Entities;
using ResourceMangementSystem.Domain.Enums;

namespace ResourceMangementSystem.Application.Services;

public interface IResourceManagementService
{
    Task<IReadOnlyCollection<Employee>> GetEmployeesAsync(CancellationToken cancellationToken = default);

    Task<Employee> AddEmployeeAsync(string name, string email, string designation, string department, EmploymentType employmentType, decimal costRate, CancellationToken cancellationToken = default);

    Task<Employee> UpdateEmployeeAsync(Guid employeeId, string name, string email, string designation, string department, EmploymentType employmentType, decimal costRate, CancellationToken cancellationToken = default);

    Task DeactivateEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task UpsertEmployeeSkillAsync(Guid employeeId, string skillName, ProficiencyLevel level, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Project>> GetProjectsAsync(CancellationToken cancellationToken = default);

    Task<Project> CreateProjectAsync(string name, string client, DateOnly startDate, DateOnly endDate, ProjectStatus status, IEnumerable<ProjectRequirement>? requirements, CancellationToken cancellationToken = default);

    Task<Project> UpdateProjectAsync(Guid projectId, string name, string client, DateOnly startDate, DateOnly endDate, ProjectStatus status, CancellationToken cancellationToken = default);

    Task SetProjectRequirementsAsync(Guid projectId, IEnumerable<ProjectRequirement> requirements, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Allocation>> GetAllocationsAsync(CancellationToken cancellationToken = default);

    Task<Allocation> AllocateAsync(Guid employeeId, Guid projectId, DateOnly startDate, DateOnly endDate, int allocationPercentage, CancellationToken cancellationToken = default);

    Task ReleaseAllocationAsync(Guid allocationId, DateOnly releaseDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EmployeeMatchResult>> SuggestEmployeesForProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<BenchReportItem>> GetBenchReportAsync(int thresholdPercentage, DateOnly asOfDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ProjectStaffingSummaryItem>> GetProjectStaffingSummaryAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DepartmentUtilizationItem>> GetDepartmentUtilizationAsync(DateOnly asOfDate, CancellationToken cancellationToken = default);
}
