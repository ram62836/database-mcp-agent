namespace DatabaseMcp.Core.Models
{
    public class SynonymMetadata
    {
        public string SynonymName { get; set; }
        public string BaseObjectName { get; set; }
        public string TableOwner { get; set; }
        public string Schema { get; set; }
    }
}