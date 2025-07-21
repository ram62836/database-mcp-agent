namespace Hala.DatabaseAgent.Core.Models
{
    /// <summary>
    /// Represents view metadata
    /// </summary>
    public class ViewMetadata
    {
        /// <summary>
        /// The name of the view
        /// </summary>
        public string ViewName { get; set; } = string.Empty;
        
        /// <summary>
        /// The owner/schema of the view
        /// </summary>
        public string Owner { get; set; } = string.Empty;
        
        /// <summary>
        /// When the view was created
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        
        /// <summary>
        /// When the view was last modified
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        
        /// <summary>
        /// The SQL definition of the view
        /// </summary>
        public string? Definition { get; set; }
        
        /// <summary>
        /// Whether the view is updatable
        /// </summary>
        public bool IsUpdatable { get; set; }
        
        /// <summary>
        /// Columns in this view
        /// </summary>
        public List<ColumnMetadata> Columns { get; set; } = new List<ColumnMetadata>();
    }
}
