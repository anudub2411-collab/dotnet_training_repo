using ResourceMangementSystem.Application.Abstractions;
using ResourceMangementSystem.Application.Models;
using ResourceMangementSystem.Domain.Entities;
using ResourceMangementSystem.Domain.Enums;

namespace ResourceMangementSystem.Application.Services;

public sealed class ResourceManagementService : IResourceManagementService
{
    private readonly IResourceRepository _repository;
    private readonly INotificationService _notificationService;
    private ResourceDataStore? _store;

    public ResourceManagementService(IResourceRepository repository, INotificationService notificationService)
    {
        _repository = repository;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyCollection<Employee>> GetEmployeesAsync(CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        return store.Employees.OrderBy(e => e.Name).ToList();
    }

    public async Task<Employee> AddEmployeeAsync(string name, string email, string designation, string department, EmploymentType employmentType, decimal costRate, CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        var employee = new Employee(Guid.NewGuid(), name, email, designation, department, employmentType, costRate, null);
        store.Employees.Add(employee);
        await SaveAsync(cancellationToken);
        return employee;
    }

    public async Task<Employee> UpdateEmployeeAsync(Guid employeeId, string name, string email, string designation, string department, EmploymentType employmentType, decimal costRate, CancellationToken cancellationToken = default)
    {
        var employee = await GetEmployeeOrThrowAsync(employeeId, cancellationToken);
        employee.UpdateDetails(name, email, designation, department, employmentType, costRate);
        await SaveAsync(cancellationToken);
        return employee;
    }

    public async Task DeactivateEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await GetEmployeeOrThrowAsync(employeeId, cancellationToken);
        employee.Deactivate();
        await SaveAsync(cancellationToken);
    }

    public async Task UpsertEmployeeSkillAsync(Guid employeeId, string skillName, ProficiencyLevel level, CancellationToken cancellationToken = default)
    {
        var employee = await GetEmployeeOrThrowAsync(employeeId, cancellationToken);
        employee.UpsertSkill(skillName, level);
        await SaveAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Project>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        return store.Projects.OrderBy(p => p.Name).ToList();
    }

    public async Task<Project> CreateProjectAsync(string name, string client, DateOnly startDate, DateOnly endDate, ProjectStatus status, IEnumerable<ProjectRequirement>? requirements, CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        var project = new Project(Guid.NewGuid(), name, client, startDate, endDate, status, requirements?.ToList());
        store.Projects.Add(project);
        await SaveAsync(cancellationToken);
        return project;
    }

    public async Task<Project> UpdateProjectAsync(Guid projectId, string name, string client, DateOnly startDate, DateOnly endDate, ProjectStatus status, CancellationToken cancellationToken = default)
    {
        var project = await GetProjectOrThrowAsync(projectId, cancellationToken);
        project.UpdateDetails(name, client, startDate, endDate, status);
        await SaveAsync(cancellationToken);
        return project;
    }

    public async Task SetProjectRequirementsAsync(Guid projectId, IEnumerable<ProjectRequirement> requirements, CancellationToken cancellationToken = default)
    {
        var project = await GetProjectOrThrowAsync(projectId, cancellationToken);
        project.SetRequiredSkills(requirements);
        await SaveAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Allocation>> GetAllocationsAsync(CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        return store.Allocations.OrderByDescending(a => a.StartDate).ToList();
    }

    public async Task<Allocation> AllocateAsync(Guid employeeId, Guid projectId, DateOnly startDate, DateOnly endDate, int allocationPercentage, CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        var employee = await GetEmployeeOrThrowAsync(employeeId, cancellationToken);
        var project = await GetProjectOrThrowAsync(projectId, cancellationToken);

        if (!employee.IsActive)
        {
            throw new InvalidOperationException("Inactive employees cannot be allocated.");
        }

        if (startDate < project.StartDate || endDate > project.EndDate)
        {
            throw new InvalidOperationException("Allocation dates must fall within the project dates.");
        }

        var overlappingAllocation = store.Allocations
            .Where(a => a.EmployeeId == employeeId)
            .Where(a => a.Overlaps(startDate, endDate))
            .Sum(a => a.AllocationPercentage);

        if (overlappingAllocation + allocationPercentage > 100)
        {
            throw new InvalidOperationException("Employee total allocation cannot exceed 100% in an overlapping period.");
        }

        var allocation = new Allocation(Guid.NewGuid(), employeeId, projectId, startDate, endDate, allocationPercentage);
        store.Allocations.Add(allocation);

        await SaveAsync(cancellationToken);
        await _notificationService.NotifyAllocationAsync(employee, project, allocation, cancellationToken);

        return allocation;
    }

    public async Task ReleaseAllocationAsync(Guid allocationId, DateOnly releaseDate, CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        var allocation = store.Allocations.FirstOrDefault(a => a.Id == allocationId)
                         ?? throw new InvalidOperationException("Allocation not found.");

        allocation.Release(releaseDate);

        var employee = await GetEmployeeOrThrowAsync(allocation.EmployeeId, cancellationToken);
        var project = await GetProjectOrThrowAsync(allocation.ProjectId, cancellationToken);

        await SaveAsync(cancellationToken);
        await _notificationService.NotifyReleaseAsync(employee, project, allocation, cancellationToken);
    }

    public async Task<IReadOnlyCollection<EmployeeMatchResult>> SuggestEmployeesForProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        var project = await GetProjectOrThrowAsync(projectId, cancellationToken);

        if (project.RequiredSkills.Count == 0)
        {
            return [];
        }

        var matches = store.Employees
            .Where(e => e.IsActive)
            .Select(employee =>
            {
                var skillMap = employee.Skills.ToDictionary(s => s.SkillName, s => s.Level, StringComparer.OrdinalIgnoreCase);

                var matchedSkills = project.RequiredSkills
                    .Where(requirement =>
                        skillMap.TryGetValue(requirement.SkillName, out var level) && level >= requirement.MinimumLevel)
                    .Select(r => r.SkillName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var currentAllocation = store.Allocations
                    .Where(a => a.EmployeeId == employee.Id)
                    .Where(a => a.Overlaps(project.StartDate, project.EndDate))
                    .Sum(a => a.AllocationPercentage);

                return new
                {
                    Employee = employee,
                    MatchedSkills = matchedSkills,
                    CurrentAllocation = currentAllocation
                };
            })
            .Where(x => x.MatchedSkills.Count == project.RequiredSkills.Count)
            .Where(x => x.CurrentAllocation < 100)
            .OrderBy(x => x.CurrentAllocation)
            .ThenBy(x => x.Employee.Name)
            .Select(x => new EmployeeMatchResult
            {
                EmployeeId = x.Employee.Id,
                EmployeeName = x.Employee.Name,
                Department = x.Employee.Department,
                CurrentAllocationPercentage = x.CurrentAllocation,
                MatchingSkills = x.MatchedSkills
            })
            .ToList();

        return matches;
    }

    public async Task<IReadOnlyCollection<BenchReportItem>> GetBenchReportAsync(int thresholdPercentage, DateOnly asOfDate, CancellationToken cancellationToken = default)
    {
        if (thresholdPercentage is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(thresholdPercentage), "Threshold must be between 0 and 100.");
        }

        var store = await GetStoreAsync(cancellationToken);
        return store.Employees
            .Where(e => e.IsActive)
            .Select(e => new
            {
                Employee = e,
                Allocation = GetCurrentAllocation(store.Allocations, e.Id, asOfDate)
            })
            .Where(x => x.Allocation < thresholdPercentage)
            .OrderBy(x => x.Allocation)
            .ThenBy(x => x.Employee.Name)
            .Select(x => new BenchReportItem
            {
                EmployeeId = x.Employee.Id,
                EmployeeName = x.Employee.Name,
                Department = x.Employee.Department,
                AllocationPercentage = x.Allocation
            })
            .ToList();
    }

    public async Task<IReadOnlyCollection<ProjectStaffingSummaryItem>> GetProjectStaffingSummaryAsync(CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);

        var summaries = store.Projects
            .OrderBy(p => p.Name)
            .Select(project =>
            {
                var allocations = store.Allocations
                    .Where(a => a.ProjectId == project.Id && a.IsActive)
                    .ToList();

                return new ProjectStaffingSummaryItem
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    Client = project.Client,
                    ActiveAllocationCount = allocations.Count,
                    TotalAllocatedPercentage = allocations.Sum(a => a.AllocationPercentage),
                    UniqueEmployees = allocations.Select(a => a.EmployeeId).Distinct().Count()
                };
            })
            .ToList();

        return summaries;
    }

    public async Task<IReadOnlyCollection<DepartmentUtilizationItem>> GetDepartmentUtilizationAsync(DateOnly asOfDate, CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);

        var report = store.Employees
            .Where(e => e.IsActive)
            .GroupBy(e => e.Department)
            .Select(group =>
            {
                var utilizations = group
                    .Select(employee => GetCurrentAllocation(store.Allocations, employee.Id, asOfDate))
                    .ToList();

                var average = utilizations.Count == 0 ? 0 : utilizations.Average();

                return new DepartmentUtilizationItem
                {
                    Department = group.Key,
                    EmployeeCount = group.Count(),
                    AverageUtilizationPercentage = Math.Round((decimal)average, 2)
                };
            })
            .OrderBy(x => x.Department)
            .ToList();

        return report;
    }

    private async Task<Employee> GetEmployeeOrThrowAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var store = await GetStoreAsync(cancellationToken);
        return store.Employees.FirstOrDefault(e => e.Id == employeeId)
               ?? throw new InvalidOperationException("Employee not found.");
    }

    private async Task<Project> GetProjectOrThrowAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var store = await GetStoreAsync(cancellationToken);
        return store.Projects.FirstOrDefault(p => p.Id == projectId)
               ?? throw new InvalidOperationException("Project not found.");
    }

    private async Task<ResourceDataStore> GetStoreAsync(CancellationToken cancellationToken)
    {
        if (_store is not null)
        {
            return _store;
        }

        _store = await _repository.LoadAsync(cancellationToken);
        return _store;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (_store is null)
        {
            return;
        }

        await _repository.SaveAsync(_store, cancellationToken);
    }

    private static int GetCurrentAllocation(IEnumerable<Allocation> allocations, Guid employeeId, DateOnly asOfDate)
    {
        return allocations
            .Where(a => a.EmployeeId == employeeId)
            .Where(a => a.StartDate <= asOfDate && a.EndDate >= asOfDate)
            .Sum(a => a.AllocationPercentage);
    }
}
