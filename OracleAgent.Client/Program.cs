using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OracleAgent.App.Tools;
using OracleAgent.Core.Interfaces;
using OracleAgent.Core.Models;
using OracleAgent.Core.Services;

namespace OracleAgent.Client
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {

            // Setup dependency injection
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                    .Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IRawSqlService, RawSqlService>();
            using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true
            });
            var rawSqlService = serviceProvider.GetRequiredService<IRawSqlService>();

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