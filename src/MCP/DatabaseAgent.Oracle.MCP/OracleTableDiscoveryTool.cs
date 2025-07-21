using Hala.DatabaseAgent.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.ModelContextProtocol;
using Microsoft.ModelContextProtocol.Converters;
using Microsoft.ModelContextProtocol.Server;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Hala.DatabaseAgent.Oracle.MCP
{
    [McpTool("table-discovery", "Oracle Database Table Discovery")]
    [Description("Discover tables and views in an Oracle database")]
    public class OracleTableDiscoveryTool : McpTool
    {
        private readonly IDatabaseMetadataService _metadataService;
        private readonly ILogger<OracleTableDiscoveryTool> _logger;

        public OracleTableDiscoveryTool(IDatabaseMetadataService metadataService, ILogger<OracleTableDiscoveryTool> logger)
        {
            _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [McpToolFunction("list-tables", "List all tables in the database")]
        [Description("Returns a list of all user-defined tables in the database")]
        [McpFunctionOutput("A list of tables with their metadata")]
        public async Task<McpToolResponse> ListTablesAsync()
        {
            try
            {
                var tables = await _metadataService.GetAllUserDefinedTablesAsync();
                return McpToolResponse.FromObject(tables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing tables");
                return McpToolResponse.Error($"Error listing tables: {ex.Message}");
            }
        }

        [McpToolFunction("get-table", "Get detailed metadata for a specific table")]
        [Description("Returns detailed metadata for a specific table including columns, constraints, and indexes")]
        [McpFunctionOutput("Detailed table metadata")]
        public async Task<McpToolResponse> GetTableAsync(
            [Description("The schema/owner of the table")] string schema,
            [Description("The name of the table")] string tableName)
        {
            try
            {
                var table = await _metadataService.GetTableMetadataAsync(schema, tableName);
                if (table == null)
                {
                    return McpToolResponse.Error($"Table {schema}.{tableName} not found");
                }
                return McpToolResponse.FromObject(table);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table metadata for {Schema}.{TableName}", schema, tableName);
                return McpToolResponse.Error($"Error getting table metadata: {ex.Message}");
            }
        }

        [McpToolFunction("list-views", "List all views in the database")]
        [Description("Returns a list of all views in the database")]
        [McpFunctionOutput("A list of views with their metadata")]
        public async Task<McpToolResponse> ListViewsAsync()
        {
            try
            {
                var views = await _metadataService.GetAllViewsAsync();
                return McpToolResponse.FromObject(views);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing views");
                return McpToolResponse.Error($"Error listing views: {ex.Message}");
            }
        }

        [McpToolFunction("get-view", "Get detailed metadata for a specific view")]
        [Description("Returns detailed metadata for a specific view including columns and definition")]
        [McpFunctionOutput("Detailed view metadata")]
        public async Task<McpToolResponse> GetViewAsync(
            [Description("The schema/owner of the view")] string schema,
            [Description("The name of the view")] string viewName)
        {
            try
            {
                var view = await _metadataService.GetViewMetadataAsync(schema, viewName);
                if (view == null)
                {
                    return McpToolResponse.Error($"View {schema}.{viewName} not found");
                }
                return McpToolResponse.FromObject(view);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting view metadata for {Schema}.{ViewName}", schema, viewName);
                return McpToolResponse.Error($"Error getting view metadata: {ex.Message}");
            }
        }

        [McpToolFunction("clear-cache", "Clear metadata cache")]
        [Description("Clears the database metadata cache to ensure fresh data on subsequent requests")]
        [McpFunctionOutput("Cache clear confirmation")]
        public McpToolResponse ClearCache()
        {
            try
            {
                _metadataService.ClearCache();
                return McpToolResponse.FromObject(new { Success = true, Message = "Metadata cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing metadata cache");
                return McpToolResponse.Error($"Error clearing metadata cache: {ex.Message}");
            }
        }
    }
}
