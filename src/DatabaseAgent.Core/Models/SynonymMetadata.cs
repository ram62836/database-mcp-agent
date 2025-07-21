namespace Hala.DatabaseAgent.Core.Models
{
    /// <summary>
    /// Represents synonym metadata
    /// </summary>
    public class SynonymMetadata
    {
        /// <summary>
        /// The name of the synonym
        /// </summary>
        public string SynonymName { get; set; } = string.Empty;
        
        /// <summary>
        /// The owner/schema of the synonym
        /// </summary>
        public string Owner { get; set; } = string.Empty;
        
        /// <summary>
        /// The name of the target object
        /// </summary>
        public string TargetObjectName { get; set; } = string.Empty;
        
        /// <summary>
        /// The owner/schema of the target object
        /// </summary>
        public string TargetObjectOwner { get; set; } = string.Empty;
        
        /// <summary>
        /// The type of the target object (TABLE, VIEW, SEQUENCE, etc.)
        /// </summary>
        public string TargetObjectType { get; set; } = string.Empty;
        
        /// <summary>
        /// For remote synonyms, the database link name
        /// </summary>
        public string? DatabaseLink { get; set; }
        
        /// <summary>
        /// Whether this is a public synonym
        /// </summary>
        public bool IsPublic { get; set; }
        
        /// <summary>
        /// When the synonym was created
        /// </summary>
        public DateTime? CreatedDate { get; set; }
    }
}
