namespace Hala.DatabaseAgent.Core.Models
{
    /// <summary>
    /// Represents column metadata
    /// </summary>
    public class ColumnMetadata
    {
        /// <summary>
        /// The name of the column
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;
        
        /// <summary>
        /// The data type of the column
        /// </summary>
        public string DataType { get; set; } = string.Empty;
        
        /// <summary>
        /// The maximum length of the column for string types
        /// </summary>
        public int? MaxLength { get; set; }
        
        /// <summary>
        /// The precision for numeric types
        /// </summary>
        public int? Precision { get; set; }
        
        /// <summary>
        /// The scale for numeric types
        /// </summary>
        public int? Scale { get; set; }
        
        /// <summary>
        /// Whether the column allows null values
        /// </summary>
        public bool IsNullable { get; set; }
        
        /// <summary>
        /// Whether the column is part of a primary key
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        
        /// <summary>
        /// The default value for the column
        /// </summary>
        public string? DefaultValue { get; set; }
        
        /// <summary>
        /// The position of the column in the table
        /// </summary>
        public int Position { get; set; }
        
        /// <summary>
        /// Whether the column is computed
        /// </summary>
        public bool IsComputed { get; set; }
        
        /// <summary>
        /// The expression used to compute the column value (if computed)
        /// </summary>
        public string? ComputedExpression { get; set; }
        
        /// <summary>
        /// Whether the column is an identity column
        /// </summary>
        public bool IsIdentity { get; set; }
        
        /// <summary>
        /// Comments or descriptions for the column
        /// </summary>
        public string? Comments { get; set; }
    }
}
