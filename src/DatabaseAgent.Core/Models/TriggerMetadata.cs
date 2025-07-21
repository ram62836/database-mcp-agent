namespace Hala.DatabaseAgent.Core.Models
{
    /// <summary>
    /// Represents trigger metadata
    /// </summary>
    public class TriggerMetadata
    {
        /// <summary>
        /// The name of the trigger
        /// </summary>
        public string TriggerName { get; set; } = string.Empty;
        
        /// <summary>
        /// The owner/schema of the trigger
        /// </summary>
        public string Owner { get; set; } = string.Empty;
        
        /// <summary>
        /// The table name the trigger belongs to
        /// </summary>
        public string TableName { get; set; } = string.Empty;
        
        /// <summary>
        /// The owner/schema of the table
        /// </summary>
        public string TableOwner { get; set; } = string.Empty;
        
        /// <summary>
        /// When the trigger was created
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        
        /// <summary>
        /// When the trigger was last modified
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        
        /// <summary>
        /// The trigger timing (BEFORE, AFTER, INSTEAD OF)
        /// </summary>
        public string TriggerTiming { get; set; } = string.Empty;
        
        /// <summary>
        /// The events that fire the trigger (INSERT, UPDATE, DELETE)
        /// </summary>
        public List<string> TriggerEvents { get; set; } = new List<string>();
        
        /// <summary>
        /// Whether the trigger is row level or statement level
        /// </summary>
        public string TriggerLevel { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the trigger is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// The source code/definition of the trigger
        /// </summary>
        public string? Definition { get; set; }
    }
}
