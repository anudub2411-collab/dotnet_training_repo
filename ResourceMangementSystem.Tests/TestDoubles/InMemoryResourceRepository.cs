using ResourceMangementSystem.Application.Abstractions;
using ResourceMangementSystem.Application.Models;

namespace ResourceMangementSystem.Tests.TestDoubles;

internal sealed class InMemoryResourceRepository : IResourceRepository
{
    public ResourceDataStore Store { get; }

    public int SaveCount { get; private set; }

    public InMemoryResourceRepository(ResourceDataStore store)
    {
        Store = store;
    }

    public Task<ResourceDataStore> LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Store);
    }

    public Task SaveAsync(ResourceDataStore dataStore, CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
