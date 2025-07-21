namespace Hala.DatabaseAgent.Core.Models
{
    /// <summary>
    /// Represents index metadata
    /// </summary>
    public class IndexMetadata
    {
        /// <summary>
        /// The name of the index
        /// </summary>
        public string IndexName { get; set; } = string.Empty;
        
        /// <summary>
        /// The table name the index belongs to
        /// </summary>
        public string TableName { get; set; } = string.Empty;
        
        /// <summary>
        /// The owner/schema of the table
        /// </summary>
        public string Owner { get; set; } = string.Empty;
        
        /// <summary>
        /// The type of index (e.g., BTREE, HASH, FULLTEXT, etc.)
        /// </summary>
        public string IndexType { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the index is unique
        /// </summary>
        public bool IsUnique { get; set; }
        
        /// <summary>
        /// Whether the index is a primary key index
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        
        /// <summary>
        /// The column(s) included in the index
        /// </summary>
        public List<IndexColumnMetadata> Columns { get; set; } = new List<IndexColumnMetadata>();
        
        /// <summary>
        /// When the index was created
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        
        /// <summary>
        /// When the index was last modified
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        
        /// <summary>
        /// Whether the index is enabled/valid
        /// </summary>
        public bool IsValid { get; set; } = true;
    }
    
    /// <summary>
    /// Represents a column in an index
    /// </summary>
    public class IndexColumnMetadata
    {
        /// <summary>
        /// The name of the column
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;
        
        /// <summary>
        /// The position of the column in the index
        /// </summary>
        public int Position { get; set; }
        
        /// <summary>
        /// Whether the column is sorted in descending order
        /// </summary>
        public bool IsDescending { get; set; }
    }
}
