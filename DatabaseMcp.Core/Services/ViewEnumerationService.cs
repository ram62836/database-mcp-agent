using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using Microsoft.Extensions.Logging;

namespace DatabaseMcp.Core.Services
{
    public class ViewEnumerationService : IViewEnumerationService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<ViewEnumerationService> _logger;
        private readonly string _owner;

        public ViewEnumerationService(IDbConnectionFactory connectionFactory, ILogger<ViewEnumerationService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
            _owner = Environment.GetEnvironmentVariable("SchemaOwner");
        }

        public async Task<List<ViewMetadata>> GetViewsDefinitionByNamesAsync(List<string> viewNames)
        {
            _logger.LogInformation("Getting views by name.");
            List<ViewMetadata> viewsMetadata = [];
            foreach (string viewName in viewNames)
            {
                ViewMetadata view = new()
                {
                    Definition = await GetViewDefinitionAsync(viewName)
                };
                viewsMetadata.Add(view);
            }

            return viewsMetadata;
        }

        private async Task<string> GetViewDefinitionAsync(string viewName)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException("The value cannot be an empty string", nameof(viewName));
            }

            _logger.LogInformation("Getting view definition for: {ViewName}", viewName);
            try
            {
                using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
                IDbCommand command = connection.CreateCommand();
                command.CommandText = string.IsNullOrEmpty(_owner) 
                    ? @"SELECT DBMS_METADATA.GET_DDL('VIEW', :ViewName) AS DDL FROM DUAL"
                    : @"SELECT DBMS_METADATA.GET_DDL('VIEW', :ViewName, :Owner) AS DDL FROM DUAL";
                IDbDataParameter param = command.CreateParameter();
                param.ParameterName = "ViewName";
                param.Value = viewName;
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
                _logger.LogError(ex, "Error getting function definition for: {ViewName}", viewName);
                throw;
            }
        }
    }
}