using System;
using System.Collections.Generic;

namespace DatabaseMcp.Core.Models
{
    /// <summary>
    /// Represents index usage statistics
    /// </summary>
    public class IndexUsageStatistics
    {
        public string IndexName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string SchemaName { get; set; } = string.Empty;
        public string IndexType { get; set; } = string.Empty;
        public bool IsUnique { get; set; }
        public long TotalAccess { get; set; }
        public DateTime LastUsed { get; set; }
        public long BlevelDepth { get; set; }
        public long LeafBlocks { get; set; }
        public long DistinctKeys { get; set; }
        public double ClusteringFactor { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> ColumnNames { get; set; } = new();
        public bool IsUnused { get; set; }
        public DateTime MonitoringStarted { get; set; }
    }
}
