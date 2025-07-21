using Hala.DatabaseAgent.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.ModelContextProtocol;
using Microsoft.ModelContextProtocol.Converters;
using Microsoft.ModelContextProtocol.Server;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Hala.DatabaseAgent.Oracle.MCP
{
    [McpTool("query-execution", "Oracle SQL Query Execution")]
    [Description("Execute SQL queries against an Oracle database")]
    public class OracleQueryExecutionTool : McpTool
    {
        private readonly ISqlExecutionService _sqlExecutionService;
        private readonly ILogger<OracleQueryExecutionTool> _logger;

        public OracleQueryExecutionTool(ISqlExecutionService sqlExecutionService, ILogger<OracleQueryExecutionTool> logger)
        {
            _sqlExecutionService = sqlExecutionService ?? throw new ArgumentNullException(nameof(sqlExecutionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [McpToolFunction("execute-sql", "Execute a SQL query")]
        [Description("Executes a SQL query against the Oracle database and returns the results")]
        [McpFunctionOutput("Query results including column names, data types, rows, and execution time")]
        public async Task<McpToolResponse> ExecuteSqlAsync(
            [Description("The SQL query to execute")] 
            [Required] string sql,
            
            [Description("Optional timeout in seconds")]
            [Range(1, 3600)] int? timeoutSeconds = null)
        {
            try
            {
                _logger.LogDebug("Executing SQL query: {Sql}", sql);
                var result = await _sqlExecutionService.ExecuteQueryAsync(sql, timeoutSeconds);
                return McpToolResponse.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SQL query: {Sql}", sql);
                return McpToolResponse.Error($"Error executing SQL query: {ex.Message}");
            }
        }
    }
}
