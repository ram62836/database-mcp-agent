namespace OracleAgent.Core.Models
{
    public class TriggerMetadata
    {
        public string TriggerName { get; set; }
        public string TriggerType { get; set; }
        public string TriggeringEvent { get; set; }
        public string TableName { get; set; }
        public string Description { get; set; }
    }
}