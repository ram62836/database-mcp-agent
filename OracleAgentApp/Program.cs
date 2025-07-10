using System.Data.Common;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OracleAgent.Core;
using OracleAgent.Core.Interfaces;
using OracleAgent.Core.Services;
using Serilog;

string logPath = Path.Combine(Directory.GetCurrentDirectory() ?? ".", "oracleagent.log");
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

ConfigurationManager config = new();
config.SetBasePath(Directory.GetCurrentDirectory());
config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
HostApplicationBuilderSettings settings = new()
{
    Configuration = config
};

HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(settings: settings);
builder.Services.AddLogging(logging => logging.AddSerilog(Log.Logger, dispose: true));
builder.Services.AddMcpServer()
  .WithStdioServerTransport()
    .WithToolsFromAssembly();
builder.Services.AddScoped<IDbConnectionFactory, OracleDbConnectionFactory>();
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
builder.Services.AddScoped<IRawSqlService, RawSqlService>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton(Log.Logger);
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger, dispose: true));
IHost app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    IStoredProcedureFunctionService storedProcedureFunctionsService = scope.ServiceProvider.GetRequiredService<IStoredProcedureFunctionService>();
    ITableDiscoveryService tableDiscoveryService = scope.ServiceProvider.GetRequiredService<ITableDiscoveryService>();
    ITriggerService triggerService = scope.ServiceProvider.GetRequiredService<ITriggerService>();
    IViewEnumerationService viewsService = scope.ServiceProvider.GetRequiredService<IViewEnumerationService>();

    _ = await storedProcedureFunctionsService.GetAllStoredProceduresAsync();
    _ = await storedProcedureFunctionsService.GetAllFunctionsAsync();
    _ = await tableDiscoveryService.GetAllUserDefinedTablesAsync();
    _ = await triggerService.GetAllTriggersAsync();
    _ = await viewsService.GetAllViewsAsync();
}

await app.RunAsync();
