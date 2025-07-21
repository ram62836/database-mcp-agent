namespace Hala.DatabaseAgent.Core.Models
{
    /// <summary>
    /// Represents stored procedure metadata
    /// </summary>
    public class StoredProcedureMetadata
    {
        /// <summary>
        /// The name of the stored procedure
        /// </summary>
        public string ProcedureName { get; set; } = string.Empty;
        
        /// <summary>
        /// The owner/schema of the stored procedure
        /// </summary>
        public string Owner { get; set; } = string.Empty;
        
        /// <summary>
        /// When the stored procedure was created
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        
        /// <summary>
        /// When the stored procedure was last modified
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        
        /// <summary>
        /// The source code/definition of the stored procedure
        /// </summary>
        public string? Definition { get; set; }
        
        /// <summary>
        /// Parameters for the stored procedure
        /// </summary>
        public List<ParameterMetadata> Parameters { get; set; } = new List<ParameterMetadata>();
    }

    /// <summary>
    /// Represents function metadata
    /// </summary>
    public class FunctionMetadata
    {
        /// <summary>
        /// The name of the function
        /// </summary>
        public string FunctionName { get; set; } = string.Empty;
        
        /// <summary>
        /// The owner/schema of the function
        /// </summary>
        public string Owner { get; set; } = string.Empty;
        
        /// <summary>
        /// When the function was created
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        
        /// <summary>
        /// When the function was last modified
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        
        /// <summary>
        /// The source code/definition of the function
        /// </summary>
        public string? Definition { get; set; }
        
        /// <summary>
        /// The return type of the function
        /// </summary>
        public string ReturnType { get; set; } = string.Empty;
        
        /// <summary>
        /// Parameters for the function
        /// </summary>
        public List<ParameterMetadata> Parameters { get; set; } = new List<ParameterMetadata>();
    }
    
    /// <summary>
    /// Represents parameter metadata for stored procedures and functions
    /// </summary>
    public class ParameterMetadata
    {
        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string ParameterName { get; set; } = string.Empty;
        
        /// <summary>
        /// The data type of the parameter
        /// </summary>
        public string DataType { get; set; } = string.Empty;
        
        /// <summary>
        /// The position of the parameter
        /// </summary>
        public int Position { get; set; }
        
        /// <summary>
        /// The parameter mode (IN, OUT, INOUT)
        /// </summary>
        public string ParameterMode { get; set; } = string.Empty;
        
        /// <summary>
        /// The default value for the parameter
        /// </summary>
        public string? DefaultValue { get; set; }
        
        /// <summary>
        /// The maximum length of the parameter for string types
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
    }
}
