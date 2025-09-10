using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using Microsoft.Extensions.Logging;

namespace DatabaseMcp.Core.Services
{
    public class TableDiscoveryService : ITableDiscoveryService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<TableDiscoveryService> _logger;
        private readonly string _owner;

        public TableDiscoveryService(IDbConnectionFactory connectionFactory, ILogger<TableDiscoveryService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
            _owner = Environment.GetEnvironmentVariable("SchemaOwner");
        }

        public async Task<List<TableMetadata>> GetTablesMetadataByNamesAsync(List<string> tableNames)
        {
            _logger.LogInformation("Getting tables by name.");

            // Handle null or empty table names list
            if (tableNames == null || tableNames.Count == 0)
            {
                _logger.LogInformation("No table names provided, returning empty list.");
                return [];
            }

            // Get all tables
            List<TableMetadata> tablesMetadata = [];
            foreach (string tableName in tableNames)
            {
                TableMetadata table = new()
                {
                    Definition = await GetTableMetadataAsync(tableName)
                };
                tablesMetadata.Add(table);
            }

            _logger.LogInformation("Found {0} tables by name.", tablesMetadata.Count);
            return tablesMetadata;
        }

        private async Task<string> GetTableMetadataAsync(string tableName)
        {
            try
            {
                using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
                IDbCommand command = connection.CreateCommand();
                command.CommandText = string.IsNullOrEmpty(_owner) 
                    ? @"SELECT DBMS_METADATA.GET_DDL('TABLE', :TableName) AS DDL FROM DUAL"
                    : @"SELECT DBMS_METADATA.GET_DDL('TABLE', :TableName, :Owner) AS DDL FROM DUAL";
                IDbDataParameter param = command.CreateParameter();
                param.ParameterName = "TableName";
                param.Value = tableName;
                _ = command.Parameters.Add(param);
                
                if (!string.IsNullOrEmpty(_owner))
                {
                    IDbDataParameter ownerParam = command.CreateParameter();
                    ownerParam.ParameterName = "Owner";
                    ownerParam.Value = _owner;
                    _ = command.Parameters.Add(ownerParam);
                }
                return command.ExecuteScalar()?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user-defined table metadata.");
                throw;
            }
        }
    }
}