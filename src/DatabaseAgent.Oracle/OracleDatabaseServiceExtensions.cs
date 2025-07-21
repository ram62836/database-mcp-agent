using Hala.DatabaseAgent.Core.Interfaces;
using Hala.DatabaseAgent.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Hala.DatabaseAgent.Oracle
{
    /// <summary>
    /// Extension methods for configuring Oracle database services
    /// </summary>
    public static class OracleDatabaseServiceExtensions
    {
        /// <summary>
        /// Adds Oracle database services to the specified IServiceCollection
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="connectionSettings">The connection settings</param>
        /// <returns>The IServiceCollection so that additional calls can be chained</returns>
        public static IServiceCollection AddOracleDatabaseServices(this IServiceCollection services, ConnectionSettings connectionSettings)
        {
            // Add connection settings as singleton
            services.AddSingleton(connectionSettings);
            
            // Register Oracle specific implementations
            services.AddSingleton<IDbConnectionFactory, OracleDbConnectionFactory>();
            services.AddSingleton<IDatabaseMetadataService, OracleDatabaseMetadataService>();
            services.AddSingleton<ISqlExecutionService, OracleSqlExecutionService>();
            services.AddSingleton<IStoredProcedureFunctionService, OracleStoredProcedureFunctionService>();
            
            return services;
        }
    }
}
