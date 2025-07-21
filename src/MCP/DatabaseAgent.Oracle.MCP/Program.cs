using Hala.DatabaseAgent.Core.Models;
using Hala.DatabaseAgent.Oracle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ModelContextProtocol.Server;
using System.Reflection;

namespace Hala.DatabaseAgent.Oracle.MCP
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            // Set up logging
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddConfiguration(configuration.GetSection("Logging"));
            });
            
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("Starting Oracle Database Agent MCP Server...");
            
            try
            {
                // Set up services
                var services = new ServiceCollection();
                
                // Add logging
                services.AddLogging(builder =>
                {
                    builder.AddConfiguration(configuration.GetSection("Logging"));
                    builder.AddConsole();
                });
                
                // Configure connection settings
                var connectionSettings = configuration.GetSection("ConnectionSettings").Get<ConnectionSettings>();
                if (connectionSettings == null)
                {
                    throw new InvalidOperationException("Connection settings are missing or invalid in configuration.");
                }
                
                // Add Oracle database services
                services.AddOracleDatabaseServices(connectionSettings);
                
                // Add MCP tools
                services.AddSingleton<OracleTableDiscoveryTool>();
                services.AddSingleton<OracleQueryExecutionTool>();
                services.AddSingleton<OracleStoredProcedureTool>();
                
                // Build service provider
                var serviceProvider = services.BuildServiceProvider();
                
                // Configure MCP server
                var mcpServerConfiguration = configuration.GetSection("ModelContextProtocol:Server").Get<McpServerConfiguration>();
                if (mcpServerConfiguration == null)
                {
                    throw new InvalidOperationException("MCP Server configuration is missing or invalid.");
                }
                
                // Create MCP server builder
                var mcpServerBuilder = new McpServerBuilder()
                    .WithConfiguration(mcpServerConfiguration)
                    .WithServiceProvider(serviceProvider)
                    .WithVersion(Assembly.GetExecutingAssembly().GetName().Version)
                    .AddTool<OracleTableDiscoveryTool>()
                    .AddTool<OracleQueryExecutionTool>()
                    .AddTool<OracleStoredProcedureTool>();
                
                // Add Swagger support if enabled
                if (mcpServerConfiguration.EnableSwagger)
                {
                    mcpServerBuilder = mcpServerBuilder.WithSwagger();
                }
                
                // Start the server
                using var mcpServer = mcpServerBuilder.Build();
                await mcpServer.StartAsync();
                
                logger.LogInformation("Oracle Database Agent MCP Server started on port {Port}.", mcpServerConfiguration.Port);
                
                // Wait for the server to be stopped
                var tcs = new TaskCompletionSource<bool>();
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    tcs.SetResult(true);
                };
                
                await tcs.Task;
                
                // Stop the server
                await mcpServer.StopAsync();
                logger.LogInformation("Oracle Database Agent MCP Server stopped.");
                
                return 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error starting Oracle Database Agent MCP Server.");
                return 1;
            }
        }
    }
}
