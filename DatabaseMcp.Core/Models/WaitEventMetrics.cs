using System;

namespace DatabaseMcp.Core.Models
{
    /// <summary>
    /// Represents database wait event statistics
    /// </summary>
    public class WaitEventMetrics
    {
        public string EventName { get; set; } = string.Empty;
        public string WaitClass { get; set; } = string.Empty;
        public long TotalWaits { get; set; }
        public double TotalWaitTimeSeconds { get; set; }
        public double AvgWaitTimeMs { get; set; }
        public double TimeWaitedPercent { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime SampleTime { get; set; }
        public string ObjectName { get; set; } = string.Empty;
        public string ObjectOwner { get; set; } = string.Empty;
    }
}
