using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ResourceMangementSystem.Application.Abstractions;
using ResourceMangementSystem.Application.Services;
using ResourceMangementSystem.Infrastructure.Configuration;
using ResourceMangementSystem.Infrastructure.Notifications;
using ResourceMangementSystem.Infrastructure.Persistence;
using ResourceMangementSystem.Presentation;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var persistenceOptions = configuration
    .GetSection(PersistenceOptions.SectionName)
    .Get<PersistenceOptions>() ?? new PersistenceOptions();

var services = new ServiceCollection();

services.AddSingleton(persistenceOptions);
services.AddSingleton<IResourceRepository, JsonResourceRepository>();
services.AddSingleton<INotificationService, ConsoleNotificationService>();
services.AddSingleton<IResourceManagementService, ResourceManagementService>();
services.AddSingleton<ConsoleApp>();

using var serviceProvider = services.BuildServiceProvider();
var app = serviceProvider.GetRequiredService<ConsoleApp>();
await app.RunAsync();
