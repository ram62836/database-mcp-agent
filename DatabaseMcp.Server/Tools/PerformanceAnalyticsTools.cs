using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using ModelContextProtocol.Server;

namespace DatabaseMcp.Server.Tools
{
    [McpServerToolType()]
    public static class PerformanceAnalyticsTools
    {
        [McpServerTool, Description("Gets top SQL statements by performance metrics (executions, CPU time, elapsed time, etc.)")]
        public static async Task<List<SqlPerformanceMetrics>> GetTopSqlByPerformanceAsync(
            IPerformanceAnalyticsService service,
            [Description("Analysis request with filters and ordering preferences")] PerformanceAnalysisRequest request)
        {
            return await service.GetTopSqlByPerformanceAsync(request);
        }

        [McpServerTool, Description("Gets top SQL statements by execution count")]
        public static async Task<List<SqlPerformanceMetrics>> GetTopSqlByExecutionsAsync(
            IPerformanceAnalyticsService service,
            [Description("Number of top SQL statements to return (default: 10)")] int topN = 10,
            [Description("Optional start time filter")] DateTime? startTime = null,
            [Description("Optional end time filter")] DateTime? endTime = null)
        {
            return await service.GetTopSqlByExecutionsAsync(topN, startTime, endTime);
        }

        [McpServerTool, Description("Gets top SQL statements by CPU time consumption")]
        public static async Task<List<SqlPerformanceMetrics>> GetTopSqlByCpuTimeAsync(
            IPerformanceAnalyticsService service,
            [Description("Number of top SQL statements to return (default: 10)")] int topN = 10,
            [Description("Optional start time filter")] DateTime? startTime = null,
            [Description("Optional end time filter")] DateTime? endTime = null)
        {
            return await service.GetTopSqlByCpuTimeAsync(topN, startTime, endTime);
        }

        [McpServerTool, Description("Gets top SQL statements by elapsed time")]
        public static async Task<List<SqlPerformanceMetrics>> GetTopSqlByElapsedTimeAsync(
            IPerformanceAnalyticsService service,
            [Description("Number of top SQL statements to return (default: 10)")] int topN = 10,
            [Description("Optional start time filter")] DateTime? startTime = null,
            [Description("Optional end time filter")] DateTime? endTime = null)
        {
            return await service.GetTopSqlByElapsedTimeAsync(topN, startTime, endTime);
        }

        [McpServerTool, Description("Analyzes database wait events to identify performance bottlenecks")]
        public static async Task<List<WaitEventMetrics>> GetWaitEventAnalysisAsync(
            IPerformanceAnalyticsService service,
            [Description("Optional object name to filter wait events")] string objectName = null,
            [Description("Optional start time filter")] DateTime? startTime = null,
            [Description("Optional end time filter")] DateTime? endTime = null)
        {
            return await service.GetWaitEventAnalysisAsync(objectName, startTime, endTime);
        }

        [McpServerTool, Description("Gets table usage statistics including scans, lookups, and DML operations")]
        public static async Task<List<TableUsageStatistics>> GetTableUsageStatisticsAsync(
            IPerformanceAnalyticsService service,
            [Description("List of table names to analyze")] List<string> tableNames)
        {
            return await service.GetTableUsageStatisticsAsync(tableNames);
        }

        [McpServerTool, Description("Gets index usage statistics for a specific table")]
        public static async Task<List<IndexUsageStatistics>> GetIndexUsageStatisticsAsync(
            IPerformanceAnalyticsService service,
            [Description("Table name to analyze indexes for")] string tableName)
        {
            return await service.GetIndexUsageStatisticsAsync(tableName);
        }

        [McpServerTool, Description("Identifies unused indexes that could be dropped to improve DML performance")]
        public static async Task<List<IndexUsageStatistics>> GetUnusedIndexesAsync(
            IPerformanceAnalyticsService service,
            [Description("Optional list of table names to filter by. If not provided, returns unused indexes for all tables")] List<string> tableNames = null)
        {
            return await service.GetUnusedIndexesAsync(tableNames);
        }
    }
}
