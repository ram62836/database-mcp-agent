namespace OracleAgent.Core.Models
{
    public class ColumnMetadata
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public string DefaultValue { get; set; }
        public int OrdinalPosition { get; set; }
    }
}