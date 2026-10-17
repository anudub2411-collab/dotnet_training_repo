using ResourceMangementSystem.Domain.Entities;

namespace ResourceMangementSystem.Application.Models;

public sealed class ResourceDataStore
{
    public List<Employee> Employees { get; init; } = [];

    public List<Project> Projects { get; init; } = [];

    public List<Allocation> Allocations { get; init; } = [];
}
