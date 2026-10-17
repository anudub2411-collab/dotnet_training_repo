using ResourceMangementSystem.Application.Models;

namespace ResourceMangementSystem.Application.Abstractions;

public interface IResourceRepository
{
    Task<ResourceDataStore> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(ResourceDataStore dataStore, CancellationToken cancellationToken = default);
}
