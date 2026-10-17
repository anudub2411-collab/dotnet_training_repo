namespace ResourceMangementSystem.Infrastructure.Configuration;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public string DataFilePath { get; init; } = "App_data/EmpRecord.json";
}
