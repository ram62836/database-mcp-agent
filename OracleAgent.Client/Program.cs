using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OracleAgent.App.Tools;
using OracleAgent.Core.Interfaces;
using OracleAgent.Core.Services;

namespace OracleAgent.Client
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {

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
                string selectStatement = "SELECT * FROM EMS_RS_AWARD WHERE ROWNUM <= 10";
                string results = await RawSqlTool.ExecuteRawSelectAsync(rawSqlService, selectStatement);
                Console.WriteLine("SQL Query Results:");
                Console.WriteLine(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing SQL query: {ex.Message}");
            }
        }
    }
}