using System;

namespace DatabaseMcp.Core.Models
{
    /// <summary>
    /// Represents table usage statistics
    /// </summary>
    public class TableUsageStatistics
    {
        public string TableName { get; set; } = string.Empty;
        public string SchemaName { get; set; } = string.Empty;
        public long TableScans { get; set; }
        public long RowLookups { get; set; }
        public long RowsInserted { get; set; }
        public long RowsUpdated { get; set; }
        public long RowsDeleted { get; set; }
        public DateTime LastUsed { get; set; }
        public double AvgRowLength { get; set; }
        public long TableSize { get; set; }
        public long NumRows { get; set; }
        public string Tablespace { get; set; } = string.Empty;
        public bool IsPartitioned { get; set; }
    }
}
