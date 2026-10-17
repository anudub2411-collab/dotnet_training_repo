using ResourceMangementSystem.Domain.Enums;

namespace ResourceMangementSystem.Domain.Entities;

public sealed class Project
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Client { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public ProjectStatus Status { get; private set; }

    public List<ProjectRequirement> RequiredSkills { get; private set; }

    public Project(
        Guid id,
        string name,
        string client,
        DateOnly startDate,
        DateOnly endDate,
        ProjectStatus status,
        List<ProjectRequirement>? requiredSkills)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(client))
        {
            throw new ArgumentException("Client is required.", nameof(client));
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("Project end date cannot be before start date.", nameof(endDate));
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = name.Trim();
        Client = client.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
        RequiredSkills = requiredSkills ?? [];
    }

    public void UpdateDetails(
        string name,
        string client,
        DateOnly startDate,
        DateOnly endDate,
        ProjectStatus status)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(client))
        {
            throw new ArgumentException("Project details are incomplete.");
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("Project end date cannot be before start date.", nameof(endDate));
        }

        Name = name.Trim();
        Client = client.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
    }

    public void SetRequiredSkills(IEnumerable<ProjectRequirement> requirements)
    {
        RequiredSkills.Clear();
        RequiredSkills.AddRange(requirements);
    }
}
