using System;

namespace DatabaseMcp.Core.Models
{
    /// <summary>
    /// Represents SQL performance statistics
    /// </summary>
    public class SqlPerformanceMetrics
    {
        public string SqlId { get; set; } = string.Empty;
        public string SqlText { get; set; } = string.Empty;
        public long Executions { get; set; }
        public double ElapsedTimeSeconds { get; set; }
        public double CpuTimeSeconds { get; set; }
        public long DiskReads { get; set; }
        public long BufferGets { get; set; }
        public double AvgElapsedTimeSeconds { get; set; }
        public double AvgCpuTimeSeconds { get; set; }
        public long RowsProcessed { get; set; }
        public DateTime FirstLoadTime { get; set; }
        public DateTime LastLoadTime { get; set; }
        public string PlanHash { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string ParsingSchemaName { get; set; } = string.Empty;
    }
}
