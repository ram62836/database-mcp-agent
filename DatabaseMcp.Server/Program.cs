using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DatabaseMcp.Core;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Services;
using Serilog;

// Use the executable directory for all file operations
string executableDirectory = AppConstants.ExecutableDirectory;

// Get log directory from environment variable or use executable directory as default
string logDirectory = Environment.GetEnvironmentVariable("LogFilePath") ?? executableDirectory;
string logPath = Path.Combine(logDirectory, "DatabaseMcp.Server.log");

// Ensure the log directory exists
Directory.CreateDirectory(logDirectory);

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("Starting DatabaseMcp.Server from directory: {ExecutableDirectory}", executableDirectory);
Log.Information("Log files will be stored in: {LogDirectory}", logDirectory);

ConfigurationManager config = new();
config.AddEnvironmentVariables(); 
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
builder.Services.AddScoped<DatabaseConnectionService>();
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
    try
    {
        IStoredProcedureFunctionService storedProcedureFunctionsService = scope.ServiceProvider.GetRequiredService<IStoredProcedureFunctionService>();
        ITableDiscoveryService tableDiscoveryService = scope.ServiceProvider.GetRequiredService<ITableDiscoveryService>();
        ITriggerService triggerService = scope.ServiceProvider.GetRequiredService<ITriggerService>();
        IViewEnumerationService viewsService = scope.ServiceProvider.GetRequiredService<IViewEnumerationService>();

        Log.Information("Attempting to preload database metadata...");
        _ = await storedProcedureFunctionsService.GetAllStoredProceduresAsync();
        _ = await storedProcedureFunctionsService.GetAllFunctionsAsync();
        _ = await tableDiscoveryService.GetAllUserDefinedTablesAsync();
        _ = await triggerService.GetAllTriggersAsync();
        _ = await viewsService.GetAllViewsAsync();
        Log.Information("Database metadata preloaded successfully.");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to preload database metadata. The server will continue but may have slower initial responses: {Message}", ex.Message);
    }
}

await app.RunAsync();
