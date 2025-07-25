using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Core.Interfaces
{
    /// <summary>
    /// Interface for database performance analytics operations
    /// </summary>
    public interface IPerformanceAnalyticsService
    {
        /// <summary>
        /// Gets top SQL statements by various performance metrics
        /// </summary>
        /// <param name="request">Analysis request parameters</param>
        /// <returns>List of SQL performance metrics</returns>
        Task<List<SqlPerformanceMetrics>> GetTopSqlByPerformanceAsync(PerformanceAnalysisRequest request);

        /// <summary>
        /// Gets top SQL statements by execution count
        /// </summary>
        /// <param name="topN">Number of top SQL statements to return</param>
        /// <param name="startTime">Optional start time filter</param>
        /// <param name="endTime">Optional end time filter</param>
        /// <returns>List of SQL performance metrics</returns>
        Task<List<SqlPerformanceMetrics>> GetTopSqlByExecutionsAsync(int topN = 10, DateTime? startTime = null, DateTime? endTime = null);

        /// <summary>
        /// Gets top SQL statements by CPU time
        /// </summary>
        /// <param name="topN">Number of top SQL statements to return</param>
        /// <param name="startTime">Optional start time filter</param>
        /// <param name="endTime">Optional end time filter</param>
        /// <returns>List of SQL performance metrics</returns>
        Task<List<SqlPerformanceMetrics>> GetTopSqlByCpuTimeAsync(int topN = 10, DateTime? startTime = null, DateTime? endTime = null);

        /// <summary>
        /// Gets top SQL statements by elapsed time
        /// </summary>
        /// <param name="topN">Number of top SQL statements to return</param>
        /// <param name="startTime">Optional start time filter</param>
        /// <param name="endTime">Optional end time filter</param>
        /// <returns>List of SQL performance metrics</returns>
        Task<List<SqlPerformanceMetrics>> GetTopSqlByElapsedTimeAsync(int topN = 10, DateTime? startTime = null, DateTime? endTime = null);

        /// <summary>
        /// Gets wait event analysis for database bottlenecks
        /// </summary>
        /// <param name="objectName">Optional object name filter</param>
        /// <param name="startTime">Optional start time filter</param>
        /// <param name="endTime">Optional end time filter</param>
        /// <returns>List of wait event metrics</returns>
        Task<List<WaitEventMetrics>> GetWaitEventAnalysisAsync(string objectName = null, DateTime? startTime = null, DateTime? endTime = null);

        /// <summary>
        /// Gets table usage statistics
        /// </summary>
        /// <param name="tableNames">List of table names to analyze</param>
        /// <returns>List of table usage statistics</returns>
        Task<List<TableUsageStatistics>> GetTableUsageStatisticsAsync(List<string> tableNames);

        /// <summary>
        /// Gets index usage statistics for a specific table
        /// </summary>
        /// <param name="tableName">Table name to analyze indexes for</param>
        /// <returns>List of index usage statistics</returns>
        Task<List<IndexUsageStatistics>> GetIndexUsageStatisticsAsync(string tableName);

        /// <summary>
        /// Gets unused indexes for specific tables or all tables
        /// </summary>
        /// <param name="tableNames">Optional list of table names to filter by. If null or empty, returns unused indexes for all tables</param>
        /// <returns>List of unused index statistics</returns>
        Task<List<IndexUsageStatistics>> GetUnusedIndexesAsync(List<string> tableNames = null);
    }
}
