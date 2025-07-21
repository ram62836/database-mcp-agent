namespace Hala.DatabaseAgent.Core.Models
{
    /// <summary>
    /// Represents constraint metadata
    /// </summary>
    public class ConstraintMetadata
    {
        /// <summary>
        /// The name of the constraint
        /// </summary>
        public string ConstraintName { get; set; } = string.Empty;
        
        /// <summary>
        /// The type of constraint (e.g., PRIMARY KEY, FOREIGN KEY, UNIQUE, CHECK)
        /// </summary>
        public string ConstraintType { get; set; } = string.Empty;
        
        /// <summary>
        /// The table name the constraint belongs to
        /// </summary>
        public string TableName { get; set; } = string.Empty;
        
        /// <summary>
        /// The owner/schema of the table
        /// </summary>
        public string Owner { get; set; } = string.Empty;
        
        /// <summary>
        /// The definition or condition of the constraint
        /// </summary>
        public string? Definition { get; set; }
        
        /// <summary>
        /// The column(s) involved in the constraint
        /// </summary>
        public List<string> Columns { get; set; } = new List<string>();
        
        /// <summary>
        /// For foreign keys, the referenced table name
        /// </summary>
        public string? ReferencedTableName { get; set; }
        
        /// <summary>
        /// For foreign keys, the owner/schema of the referenced table
        /// </summary>
        public string? ReferencedOwner { get; set; }
        
        /// <summary>
        /// For foreign keys, the referenced column(s)
        /// </summary>
        public List<string> ReferencedColumns { get; set; } = new List<string>();
        
        /// <summary>
        /// For foreign keys, the delete rule (e.g., CASCADE, SET NULL)
        /// </summary>
        public string? DeleteRule { get; set; }
        
        /// <summary>
        /// Whether the constraint is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// When the constraint was created
        /// </summary>
        public DateTime? CreatedDate { get; set; }
    }
}
