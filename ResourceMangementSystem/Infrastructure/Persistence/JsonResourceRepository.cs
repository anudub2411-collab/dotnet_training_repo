using System.Text.Json;
using ResourceMangementSystem.Application.Abstractions;
using ResourceMangementSystem.Application.Models;
using ResourceMangementSystem.Infrastructure.Configuration;

namespace ResourceMangementSystem.Infrastructure.Persistence;

public sealed class JsonResourceRepository : IResourceRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _dataFilePath;

    public JsonResourceRepository(PersistenceOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.DataFilePath))
        {
            throw new ArgumentException("Data file path is required.", nameof(options));
        }

        _dataFilePath = Path.GetFullPath(options.DataFilePath);
    }

    public async Task<ResourceDataStore> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_dataFilePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_dataFilePath)!);
            var emptyStore = new ResourceDataStore();
            await SaveAsync(emptyStore, cancellationToken);
            return emptyStore;
        }

        await using var stream = File.OpenRead(_dataFilePath);
        var data = await JsonSerializer.DeserializeAsync<ResourceDataStore>(stream, SerializerOptions, cancellationToken);
        return data ?? new ResourceDataStore();
    }

    public async Task SaveAsync(ResourceDataStore dataStore, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_dataFilePath)!);

        await using var stream = File.Create(_dataFilePath);
        await JsonSerializer.SerializeAsync(stream, dataStore, SerializerOptions, cancellationToken);
    }
}
