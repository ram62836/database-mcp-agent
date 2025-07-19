using System;
using System.Data.Common;
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
string logPath = Path.Combine(executableDirectory, "DatabaseMcp.Server.log");

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("Starting DatabaseMcp.Server from directory: {ExecutableDirectory}", executableDirectory);

// Log path diagnostics to file instead of stdout to avoid MCP protocol interference
Log.Information("=== Path Diagnostics ===");
Log.Information("Current Directory: {CurrentDirectory}", Directory.GetCurrentDirectory());
Log.Information("AppDomain BaseDirectory: {BaseDirectory}", AppDomain.CurrentDomain.BaseDirectory);
Log.Information("Assembly Location: {AssemblyLocation}", System.Reflection.Assembly.GetEntryAssembly()?.Location);
Log.Information("Assembly Directory: {AssemblyDirectory}", Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location));
Log.Information("Executable Directory: {ExecutableDirectory}", executableDirectory);
Log.Information("Expected appsettings.json path: {AppSettingsPath}", Path.Combine(executableDirectory, "appsettings.json"));
Log.Information("appsettings.json exists: {AppSettingsExists}", File.Exists(Path.Combine(executableDirectory, "appsettings.json")));

ConfigurationManager config = new();
config.SetBasePath(executableDirectory);
config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
config.AddEnvironmentVariables(); // Add environment variables support
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
builder.Services.AddScoped<DatabaseMcp.Core.Services.DatabaseConnectionService>();
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
