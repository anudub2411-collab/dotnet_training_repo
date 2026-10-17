using ResourceMangementSystem.Domain.Enums;

namespace ResourceMangementSystem.Domain.Entities;

public sealed class Employee
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Email { get; private set; }

    public string Designation { get; private set; }

    public string Department { get; private set; }

    public EmploymentType EmploymentType { get; private set; }

    public decimal CostRate { get; private set; }

    public bool IsActive { get; private set; }

    public List<EmployeeSkill> Skills { get; private set; }

    public Employee(
        Guid id,
        string name,
        string email,
        string designation,
        string department,
        EmploymentType employmentType,
        decimal costRate,
        List<EmployeeSkill>? skills,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(designation))
        {
            throw new ArgumentException("Designation is required.", nameof(designation));
        }

        if (string.IsNullOrWhiteSpace(department))
        {
            throw new ArgumentException("Department is required.", nameof(department));
        }

        if (costRate < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(costRate), "Cost rate cannot be negative.");
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = name.Trim();
        Email = email.Trim();
        Designation = designation.Trim();
        Department = department.Trim();
        EmploymentType = employmentType;
        CostRate = costRate;
        IsActive = isActive;
        Skills = skills ?? [];
    }

    public void UpdateDetails(
        string name,
        string email,
        string designation,
        string department,
        EmploymentType employmentType,
        decimal costRate)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(designation) ||
            string.IsNullOrWhiteSpace(department))
        {
            throw new ArgumentException("Employee details are incomplete.");
        }

        if (costRate < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(costRate), "Cost rate cannot be negative.");
        }

        Name = name.Trim();
        Email = email.Trim();
        Designation = designation.Trim();
        Department = department.Trim();
        EmploymentType = employmentType;
        CostRate = costRate;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpsertSkill(string skillName, ProficiencyLevel level)
    {
        var existing = Skills.FirstOrDefault(s =>
            string.Equals(s.SkillName, skillName, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            Skills.Add(new EmployeeSkill(skillName, level));
            return;
        }

        existing.UpdateLevel(level);
    }
}
