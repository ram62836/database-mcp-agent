namespace DatabaseMcp.Core.Models
{
    public class ParameterMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public string DefaultValue { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public int Position { get; set; }
    }
}
