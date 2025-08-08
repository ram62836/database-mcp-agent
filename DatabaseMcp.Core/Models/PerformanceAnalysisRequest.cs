using System;
using System.Collections.Generic;

namespace DatabaseMcp.Core.Models
{
    /// <summary>
    /// Represents performance analysis request parameters
    /// </summary>
    public class PerformanceAnalysisRequest
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TopN { get; set; } = 10;
        public string OrderBy { get; set; } = "ELAPSED_TIME";
        public List<string> IncludeModules { get; set; } = [];
        public List<string> ExcludeModules { get; set; } = [];
        public string SchemaName { get; set; } = string.Empty;
        public bool IncludeSystemSql { get; set; } = false;
    }
}
