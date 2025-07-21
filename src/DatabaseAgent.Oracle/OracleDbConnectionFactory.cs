using Hala.DatabaseAgent.Core.Interfaces;
using Hala.DatabaseAgent.Core.Models;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Hala.DatabaseAgent.Oracle
{
    /// <summary>
    /// Oracle implementation of the database connection factory
    /// </summary>
    public class OracleDbConnectionFactory : IDbConnectionFactory
    {
        private readonly ConnectionSettings _connectionSettings;
        private readonly ILogger<OracleDbConnectionFactory> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDbConnectionFactory"/> class.
        /// </summary>
        /// <param name="connectionSettings">The connection settings.</param>
        /// <param name="logger">The logger.</param>
        public OracleDbConnectionFactory(ConnectionSettings connectionSettings, ILogger<OracleDbConnectionFactory> logger)
        {
            _connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Validate that this is an Oracle connection
            if (!string.Equals(_connectionSettings.Provider, "Oracle", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Connection provider must be Oracle for OracleDbConnectionFactory");
            }
        }

        /// <inheritdoc/>
        public async Task<IDbConnection> CreateConnectionAsync()
        {
            try
            {
                var connection = new OracleConnection(_connectionSettings.ConnectionString);
                await connection.OpenAsync();
                _logger.LogDebug("Oracle connection opened successfully");
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Oracle connection");
                throw;
            }
        }

        /// <inheritdoc/>
        public IDbConnection CreateConnection()
        {
            try
            {
                var connection = new OracleConnection(_connectionSettings.ConnectionString);
                connection.Open();
                _logger.LogDebug("Oracle connection opened successfully");
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Oracle connection");
                throw;
            }
        }

        /// <inheritdoc/>
        public string GetProviderName()
        {
            return "Oracle";
        }

        /// <inheritdoc/>
        public string GetDefaultSchema()
        {
            return _connectionSettings.DefaultSchema;
        }
    }
}
