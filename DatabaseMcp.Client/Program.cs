using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Services;

namespace OracleAgent.Client
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 Oracle Database MCP Client");
            Console.WriteLine("============================");
            Console.WriteLine();
            Console.WriteLine("This is a simple client for testing the Oracle Database MCP Core services.");
            Console.WriteLine("For full MCP functionality, use DatabaseMcp.Server instead.");
            Console.WriteLine();

            // Setup dependency injection
            ServiceCollection services = new();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                    .Build();
            _ = services.AddSingleton<IConfiguration>(configuration);
            _ = services.AddSingleton<IRawSqlService, RawSqlService>();
            
            using ServiceProvider serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true
            });
            
            IRawSqlService rawSqlService = serviceProvider.GetRequiredService<IRawSqlService>();

            try
            {
                string selectStatement = "SELECT 'Database connection test successful!' AS message FROM DUAL";
                string results = await rawSqlService.ExecuteRawSelectAsync(selectStatement);
                Console.WriteLine("SQL Query Results:");
                Console.WriteLine(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing SQL query: {ex.Message}");
                Console.WriteLine("Please check your database connection configuration in appsettings.json");
            }
            
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}