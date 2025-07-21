using System.Text.Json.Serialization;

namespace Hala.DatabaseAgent.Core.Models
{
    /// <summary>
    /// Represents table metadata
    /// </summary>
    public class TableMetadata
    {
        /// <summary>
        /// The name of the table
        /// </summary>
        public string TableName { get; set; } = string.Empty;
        
        /// <summary>
        /// The owner/schema of the table
        /// </summary>
        public string Owner { get; set; } = string.Empty;
        
        /// <summary>
        /// The number of rows in the table (if available)
        /// </summary>
        public long? RowCount { get; set; }
        
        /// <summary>
        /// When the table was created
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        
        /// <summary>
        /// When the table was last modified
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        
        /// <summary>
        /// Columns in this table
        /// </summary>
        public List<ColumnMetadata> Columns { get; set; } = new List<ColumnMetadata>();
        
        /// <summary>
        /// Primary key constraints for this table
        /// </summary>
        public List<ConstraintMetadata> PrimaryKeyConstraints { get; set; } = new List<ConstraintMetadata>();
        
        /// <summary>
        /// Foreign key constraints for this table
        /// </summary>
        public List<ConstraintMetadata> ForeignKeyConstraints { get; set; } = new List<ConstraintMetadata>();
        
        /// <summary>
        /// Unique constraints for this table
        /// </summary>
        public List<ConstraintMetadata> UniqueConstraints { get; set; } = new List<ConstraintMetadata>();
        
        /// <summary>
        /// Check constraints for this table
        /// </summary>
        public List<ConstraintMetadata> CheckConstraints { get; set; } = new List<ConstraintMetadata>();
        
        /// <summary>
        /// Indexes for this table
        /// </summary>
        public List<IndexMetadata> Indexes { get; set; } = new List<IndexMetadata>();
    }
}
