namespace Hala.DatabaseAgent.Core.Models
{
    /// <summary>
    /// Represents database connection settings
    /// </summary>
    public class ConnectionSettings
    {
        /// <summary>
        /// The database provider (Oracle, SqlServer, PostgreSql, MySql, etc.)
        /// </summary>
        public string Provider { get; set; } = string.Empty;
        
        /// <summary>
        /// The connection string for the database
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
        
        /// <summary>
        /// The default schema/owner to use when executing queries
        /// </summary>
        public string DefaultSchema { get; set; } = string.Empty;
        
        /// <summary>
        /// The maximum timeout for commands in seconds
        /// </summary>
        public int CommandTimeout { get; set; } = 30;
        
        /// <summary>
        /// Whether to enable metadata caching
        /// </summary>
        public bool EnableMetadataCaching { get; set; } = true;
        
        /// <summary>
        /// The cache expiration time in minutes
        /// </summary>
        public int CacheExpirationMinutes { get; set; } = 60;
    }
}
