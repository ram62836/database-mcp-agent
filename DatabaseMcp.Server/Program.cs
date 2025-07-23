using System;
using System.IO;
using DatabaseMcp.Core;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// Use the executable directory for all file operations
string executableDirectory = AppConstants.ExecutableDirectory;

// Create the configuration first to read logging settings
ConfigurationManager loggingConfig = new();
loggingConfig.AddJsonFile(Path.Combine(executableDirectory, "appsettings.json"), optional: true, reloadOnChange: true);
loggingConfig.AddEnvironmentVariables();

// Get log directory from environment variable or use executable directory as default
string logDirectory = Environment.GetEnvironmentVariable("LogFilePath") ?? executableDirectory;
string logPath = Path.Combine(logDirectory, "DatabaseMcp.Server.log");

// Ensure the log directory exists
Directory.CreateDirectory(logDirectory);

// Configure Serilog - restrict console output to Warning and above, keep detailed logs in file
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.File(logPath,
                 rollingInterval: RollingInterval.Day,
                 outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message}{NewLine}{Exception}")
    .CreateLogger();

// Use positional string formatting to avoid template parsing issues
// These are startup messages, still logged to file but not to console due to level restrictions
Log.Information("Starting DatabaseMcp.Server from directory: {0}", executableDirectory);
Log.Information("Log files will be stored in: {0}", logDirectory);

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
        Log.Warning(ex, "Failed to preload database metadata. The server will continue but may have slower initial responses: {0}", ex.Message);
    }
}

await app.RunAsync();
