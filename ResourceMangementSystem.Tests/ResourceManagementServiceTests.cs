using ResourceMangementSystem.Application.Models;
using ResourceMangementSystem.Application.Services;
using ResourceMangementSystem.Domain.Entities;
using ResourceMangementSystem.Domain.Enums;
using ResourceMangementSystem.Tests.TestDoubles;

namespace ResourceMangementSystem.Tests;

public class ResourceManagementServiceTests
{
    [Fact]
    public async Task AllocateAsync_ShouldThrow_WhenEmployeeIsInactive()
    {
        var employee = new Employee(Guid.NewGuid(), "Test User", "test@company.com", "Engineer", "Engineering", EmploymentType.FullTime, 100, null, isActive: false);
        var project = new Project(Guid.NewGuid(), "Project A", "Client", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), ProjectStatus.Active, null);

        var service = CreateService(new ResourceDataStore
        {
            Employees = [employee],
            Projects = [project]
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AllocateAsync(employee.Id, project.Id, new DateOnly(2026, 2, 1), new DateOnly(2026, 3, 1), 50));
    }

    [Fact]
    public async Task AllocateAsync_ShouldThrow_WhenDatesOutsideProject()
    {
        var employee = CreateActiveEmployee();
        var project = new Project(Guid.NewGuid(), "Project B", "Client", new DateOnly(2026, 3, 1), new DateOnly(2026, 6, 30), ProjectStatus.Active, null);

        var service = CreateService(new ResourceDataStore
        {
            Employees = [employee],
            Projects = [project]
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AllocateAsync(employee.Id, project.Id, new DateOnly(2026, 2, 15), new DateOnly(2026, 4, 1), 40));
    }

    [Fact]
    public async Task AllocateAsync_ShouldThrow_WhenOverlappingAllocationExceedsHundred()
    {
        var employee = CreateActiveEmployee();
        var project = new Project(Guid.NewGuid(), "Project C", "Client", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), ProjectStatus.Active, null);
        var existing = new Allocation(Guid.NewGuid(), employee.Id, project.Id, new DateOnly(2026, 2, 1), new DateOnly(2026, 4, 30), 70);

        var service = CreateService(new ResourceDataStore
        {
            Employees = [employee],
            Projects = [project],
            Allocations = [existing]
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AllocateAsync(employee.Id, project.Id, new DateOnly(2026, 3, 1), new DateOnly(2026, 5, 1), 40));
    }

    [Fact]
    public async Task SuggestEmployeesForProjectAsync_ShouldReturnOnlySkillMatchesUnderCapacity()
    {
        var matchedEmployee = CreateActiveEmployee("A", "a@company.com");
        matchedEmployee.UpsertSkill("C#", ProficiencyLevel.Expert);

        var lowSkillEmployee = CreateActiveEmployee("B", "b@company.com");
        lowSkillEmployee.UpsertSkill("C#", ProficiencyLevel.Basic);

        var fullCapacityEmployee = CreateActiveEmployee("C", "c@company.com");
        fullCapacityEmployee.UpsertSkill("C#", ProficiencyLevel.Expert);

        var project = new Project(
            Guid.NewGuid(),
            "Project D",
            "Client",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            ProjectStatus.Active,
            [new ProjectRequirement("C#", ProficiencyLevel.Advanced)]);

        var fullAllocation = new Allocation(Guid.NewGuid(), fullCapacityEmployee.Id, project.Id, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 100);

        var service = CreateService(new ResourceDataStore
        {
            Employees = [matchedEmployee, lowSkillEmployee, fullCapacityEmployee],
            Projects = [project],
            Allocations = [fullAllocation]
        });

        var result = await service.SuggestEmployeesForProjectAsync(project.Id);

        var match = Assert.Single(result);
        Assert.Equal(matchedEmployee.Id, match.EmployeeId);
    }

    [Fact]
    public async Task GetBenchReportAsync_ShouldReturnEmployeesBelowThreshold()
    {
        var benchEmployee = CreateActiveEmployee("Bench", "bench@company.com");
        var allocatedEmployee = CreateActiveEmployee("Busy", "busy@company.com");
        var project = new Project(Guid.NewGuid(), "Project E", "Client", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), ProjectStatus.Active, null);
        var allocation = new Allocation(Guid.NewGuid(), allocatedEmployee.Id, project.Id, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 80);

        var service = CreateService(new ResourceDataStore
        {
            Employees = [benchEmployee, allocatedEmployee],
            Projects = [project],
            Allocations = [allocation]
        });

        var result = await service.GetBenchReportAsync(50, new DateOnly(2026, 5, 1));

        var bench = Assert.Single(result);
        Assert.Equal(benchEmployee.Id, bench.EmployeeId);
    }

    [Fact]
    public async Task GetDepartmentUtilizationAsync_ShouldComputeAveragesByDepartment()
    {
        var empA = new Employee(Guid.NewGuid(), "EmpA", "a1@company.com", "Engineer", "Engineering", EmploymentType.FullTime, 100, null);
        var empB = new Employee(Guid.NewGuid(), "EmpB", "a2@company.com", "Engineer", "Engineering", EmploymentType.FullTime, 100, null);
        var project = new Project(Guid.NewGuid(), "Project F", "Client", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), ProjectStatus.Active, null);

        var allocations = new List<Allocation>
        {
            new(Guid.NewGuid(), empA.Id, project.Id, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 40),
            new(Guid.NewGuid(), empB.Id, project.Id, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 60)
        };

        var service = CreateService(new ResourceDataStore
        {
            Employees = [empA, empB],
            Projects = [project],
            Allocations = allocations
        });

        var result = await service.GetDepartmentUtilizationAsync(new DateOnly(2026, 2, 1));
        var engineering = Assert.Single(result);

        Assert.Equal("Engineering", engineering.Department);
        Assert.Equal(50m, engineering.AverageUtilizationPercentage);
    }

    private static ResourceManagementService CreateService(ResourceDataStore store)
    {
        var repository = new InMemoryResourceRepository(store);
        var notifier = new FakeNotificationService();
        return new ResourceManagementService(repository, notifier);
    }

    private static Employee CreateActiveEmployee(string name = "Active User", string email = "active@company.com")
    {
        return new Employee(Guid.NewGuid(), name, email, "Engineer", "Engineering", EmploymentType.FullTime, 100, null);
    }
}
