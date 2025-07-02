using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OracleAgent.Core.Interfaces;
using OracleAgent.Core.Services;
using Microsoft.Extensions.Configuration;
using System.IO;

var config = new ConfigurationManager();
config.SetBasePath(Directory.GetCurrentDirectory());
config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
var settings = new HostApplicationBuilderSettings
{
    Configuration = config
};

var builder = Host.CreateEmptyApplicationBuilder(settings: settings);
builder.Services.AddMcpServer()
  .WithStdioServerTransport()
    .WithToolsFromAssembly();
builder.Services.AddScoped<IColumnMetadataService, ColumnMetadataService>();
builder.Services.AddScoped<IConstraintGatheringService, ConstraintGatheringService>();
builder.Services.AddScoped<IIndexListingService, IndexListingService>();
builder.Services.AddScoped<IKeyIdentificationService, KeyIdentificationService>();
builder.Services.AddScoped<IStoredProcedureFunctionService, StoredProcedureFunctionService>();
builder.Services.AddScoped<ISynonymListingService, SynonymListingService>();
builder.Services.AddScoped<ITableDiscoveryService, TableDiscoveryService>();
builder.Services.AddScoped<IViewEnumerationService, ViewEnumerationService>();
builder.Services.AddScoped<IObjectRelationshipService, ObjectRelationshipService>();
builder.Services.AddScoped<ITriggerService, TriggerService>();
builder.Services.AddMemoryCache();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var storedProcedureFunctionsService = scope.ServiceProvider.GetRequiredService<IStoredProcedureFunctionService>();
    var tableDiscoveryService = scope.ServiceProvider.GetRequiredService<ITableDiscoveryService>();
    var triggerService = scope.ServiceProvider.GetRequiredService<ITriggerService>();
    var viewsService = scope.ServiceProvider.GetRequiredService<IViewEnumerationService>();
    
    await storedProcedureFunctionsService.GetAllStoredProceduresAsync();
    await storedProcedureFunctionsService.GetAllFunctionsAsync();
    await tableDiscoveryService.GetAllUserDefinedTablesAsync();
    await triggerService.GetAllTriggersAsync();
    await viewsService.GetAllViewsAsync();
}

await app.RunAsync();
