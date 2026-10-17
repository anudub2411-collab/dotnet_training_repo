using ResourceMangementSystem.Application.Services;
using ResourceMangementSystem.Domain.Entities;
using ResourceMangementSystem.Domain.Enums;

namespace ResourceMangementSystem.Presentation;

public sealed class ConsoleApp
{
    private readonly IResourceManagementService _service;

    public ConsoleApp(IResourceManagementService service)
    {
        _service = service;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ShowMenu();
            var input = Console.ReadLine();

            if (!int.TryParse(input, out var option))
            {
                Console.WriteLine("Invalid option.");
                continue;
            }

            try
            {
                switch (option)
                {
                    case 0:
                        return;
                    case 1:
                        await ListEmployeesAsync(cancellationToken);
                        break;
                    case 2:
                        await AddEmployeeAsync(cancellationToken);
                        break;
                    case 3:
                        await UpdateEmployeeAsync(cancellationToken);
                        break;
                    case 4:
                        await DeactivateEmployeeAsync(cancellationToken);
                        break;
                    case 5:
                        await UpsertSkillAsync(cancellationToken);
                        break;
                    case 6:
                        await ListProjectsAsync(cancellationToken);
                        break;
                    case 7:
                        await CreateProjectAsync(cancellationToken);
                        break;
                    case 8:
                        await AllocateEmployeeAsync(cancellationToken);
                        break;
                    case 9:
                        await ReleaseAllocationAsync(cancellationToken);
                        break;
                    case 10:
                        await SuggestForProjectAsync(cancellationToken);
                        break;
                    case 11:
                        await RunBenchReportAsync(cancellationToken);
                        break;
                    case 12:
                        await RunProjectStaffingSummaryAsync(cancellationToken);
                        break;
                    case 13:
                        await RunDepartmentUtilizationAsync(cancellationToken);
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    private static void ShowMenu()
    {
        Console.WriteLine("Resource Management System");
        Console.WriteLine("1. List employees");
        Console.WriteLine("2. Add employee");
        Console.WriteLine("3. Update employee");
        Console.WriteLine("4. Deactivate employee");
        Console.WriteLine("5. Add or update employee skill");
        Console.WriteLine("6. List projects");
        Console.WriteLine("7. Create project");
        Console.WriteLine("8. Allocate employee to project");
        Console.WriteLine("9. Release allocation");
        Console.WriteLine("10. Suggest employees for project");
        Console.WriteLine("11. Bench report");
        Console.WriteLine("12. Project staffing summary");
        Console.WriteLine("13. Utilization per department");
        Console.WriteLine("0. Exit");
        Console.Write("Choose option: ");
    }

    private async Task ListEmployeesAsync(CancellationToken cancellationToken)
    {
        var employees = await _service.GetEmployeesAsync(cancellationToken);
        foreach (var employee in employees)
        {
            Console.WriteLine($"{employee.Id} | {employee.Name} | {employee.Department} | Active: {employee.IsActive}");
        }
    }

    private async Task AddEmployeeAsync(CancellationToken cancellationToken)
    {
        Console.Write("Name: ");
        var name = ReadRequired();
        Console.Write("Email: ");
        var email = ReadRequired();
        Console.Write("Designation: ");
        var designation = ReadRequired();
        Console.Write("Department: ");
        var department = ReadRequired();
        var employmentType = ReadEnum<EmploymentType>("Employment type");
        Console.Write("Cost rate: ");
        var costRate = decimal.Parse(ReadRequired());

        var employee = await _service.AddEmployeeAsync(name, email, designation, department, employmentType, costRate, cancellationToken);
        Console.WriteLine($"Employee created: {employee.Id}");
    }

    private async Task UpdateEmployeeAsync(CancellationToken cancellationToken)
    {
        var employeeId = ReadGuid("Employee ID: ");
        Console.Write("Name: ");
        var name = ReadRequired();
        Console.Write("Email: ");
        var email = ReadRequired();
        Console.Write("Designation: ");
        var designation = ReadRequired();
        Console.Write("Department: ");
        var department = ReadRequired();
        var employmentType = ReadEnum<EmploymentType>("Employment type");
        Console.Write("Cost rate: ");
        var costRate = decimal.Parse(ReadRequired());

        await _service.UpdateEmployeeAsync(employeeId, name, email, designation, department, employmentType, costRate, cancellationToken);
        Console.WriteLine("Employee updated.");
    }

    private async Task DeactivateEmployeeAsync(CancellationToken cancellationToken)
    {
        var employeeId = ReadGuid("Employee ID: ");
        await _service.DeactivateEmployeeAsync(employeeId, cancellationToken);
        Console.WriteLine("Employee deactivated.");
    }

    private async Task UpsertSkillAsync(CancellationToken cancellationToken)
    {
        var employeeId = ReadGuid("Employee ID: ");
        Console.Write("Skill name: ");
        var skillName = ReadRequired();
        var level = ReadEnum<ProficiencyLevel>("Proficiency");

        await _service.UpsertEmployeeSkillAsync(employeeId, skillName, level, cancellationToken);
        Console.WriteLine("Skill saved.");
    }

    private async Task ListProjectsAsync(CancellationToken cancellationToken)
    {
        var projects = await _service.GetProjectsAsync(cancellationToken);
        foreach (var project in projects)
        {
            Console.WriteLine($"{project.Id} | {project.Name} | {project.Client} | {project.StartDate:yyyy-MM-dd} to {project.EndDate:yyyy-MM-dd}");
        }
    }

    private async Task CreateProjectAsync(CancellationToken cancellationToken)
    {
        Console.Write("Name: ");
        var name = ReadRequired();
        Console.Write("Client: ");
        var client = ReadRequired();
        var startDate = ReadDate("Start date (yyyy-MM-dd): ");
        var endDate = ReadDate("End date (yyyy-MM-dd): ");
        var status = ReadEnum<ProjectStatus>("Project status");

        var requirements = new List<ProjectRequirement>();
        Console.Write("How many required skills? ");
        var count = int.Parse(ReadRequired());
        for (var i = 0; i < count; i++)
        {
            Console.Write($"Skill {i + 1} name: ");
            var skill = ReadRequired();
            var level = ReadEnum<ProficiencyLevel>($"Skill {i + 1} minimum proficiency");
            requirements.Add(new ProjectRequirement(skill, level));
        }

        var project = await _service.CreateProjectAsync(name, client, startDate, endDate, status, requirements, cancellationToken);
        Console.WriteLine($"Project created: {project.Id}");
    }

    private async Task AllocateEmployeeAsync(CancellationToken cancellationToken)
    {
        var employeeId = ReadGuid("Employee ID: ");
        var projectId = ReadGuid("Project ID: ");
        var startDate = ReadDate("Allocation start date (yyyy-MM-dd): ");
        var endDate = ReadDate("Allocation end date (yyyy-MM-dd): ");
        Console.Write("Allocation percentage: ");
        var percentage = int.Parse(ReadRequired());

        var allocation = await _service.AllocateAsync(employeeId, projectId, startDate, endDate, percentage, cancellationToken);
        Console.WriteLine($"Allocation created: {allocation.Id}");
    }

    private async Task ReleaseAllocationAsync(CancellationToken cancellationToken)
    {
        var allocationId = ReadGuid("Allocation ID: ");
        var releaseDate = ReadDate("Release date (yyyy-MM-dd): ");

        await _service.ReleaseAllocationAsync(allocationId, releaseDate, cancellationToken);
        Console.WriteLine("Allocation released.");
    }

    private async Task SuggestForProjectAsync(CancellationToken cancellationToken)
    {
        var projectId = ReadGuid("Project ID: ");
        var matches = await _service.SuggestEmployeesForProjectAsync(projectId, cancellationToken);

        foreach (var item in matches)
        {
            Console.WriteLine($"{item.EmployeeName} | {item.Department} | Current allocation: {item.CurrentAllocationPercentage}% | Skills: {string.Join(", ", item.MatchingSkills)}");
        }

        if (matches.Count == 0)
        {
            Console.WriteLine("No matching employees found.");
        }
    }

    private async Task RunBenchReportAsync(CancellationToken cancellationToken)
    {
        Console.Write("Bench threshold percentage: ");
        var threshold = int.Parse(ReadRequired());
        var report = await _service.GetBenchReportAsync(threshold, DateOnly.FromDateTime(DateTime.Today), cancellationToken);

        foreach (var item in report)
        {
            Console.WriteLine($"{item.EmployeeName} | {item.Department} | {item.AllocationPercentage}%");
        }
    }

    private async Task RunProjectStaffingSummaryAsync(CancellationToken cancellationToken)
    {
        var report = await _service.GetProjectStaffingSummaryAsync(cancellationToken);
        foreach (var item in report)
        {
            Console.WriteLine($"{item.ProjectName} | Client: {item.Client} | Active allocations: {item.ActiveAllocationCount} | Total %: {item.TotalAllocatedPercentage} | Employees: {item.UniqueEmployees}");
        }
    }

    private async Task RunDepartmentUtilizationAsync(CancellationToken cancellationToken)
    {
        var report = await _service.GetDepartmentUtilizationAsync(DateOnly.FromDateTime(DateTime.Today), cancellationToken);
        foreach (var item in report)
        {
            Console.WriteLine($"{item.Department} | Employees: {item.EmployeeCount} | Avg utilization: {item.AverageUtilizationPercentage}%");
        }
    }

    private static string ReadRequired()
    {
        var value = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(value))
        {
            Console.Write("Please enter a value: ");
            value = Console.ReadLine();
        }

        return value.Trim();
    }

    private static Guid ReadGuid(string prompt)
    {
        Console.Write(prompt);
        Guid id;
        while (!Guid.TryParse(ReadRequired(), out id))
        {
            Console.Write("Invalid GUID. Try again: ");
        }

        return id;
    }

    private static DateOnly ReadDate(string prompt)
    {
        Console.Write(prompt);
        DateOnly date;
        while (!DateOnly.TryParse(ReadRequired(), out date))
        {
            Console.Write("Invalid date. Try again (yyyy-MM-dd): ");
        }

        return date;
    }

    private static TEnum ReadEnum<TEnum>(string title) where TEnum : struct, Enum
    {
        Console.WriteLine($"{title}:");

        foreach (var value in Enum.GetValues<TEnum>())
        {
            Console.WriteLine($"{Convert.ToInt32(value)}. {value}");
        }

        Console.Write("Choose: ");
        int enumValue;
        while (!int.TryParse(ReadRequired(), out enumValue) || !Enum.IsDefined(typeof(TEnum), enumValue))
        {
            Console.Write("Invalid choice. Try again: ");
        }

        return (TEnum)Enum.ToObject(typeof(TEnum), enumValue);
    }
}
