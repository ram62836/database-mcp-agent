using System;
using System.IO;

namespace DatabaseMcp.Core
{
    public static class AppConstants
    {
        // Get the directory where the executable is located
        private static readonly string BaseDirectory = Environment.GetEnvironmentVariable("LogFilePath")
                                                      ?? AppDomain.CurrentDomain.BaseDirectory;
        
        /// <summary>
        /// Gets the directory where the executable is located. 
        /// This is where appsettings.json, cache files, and logs should be stored.
        /// </summary>
        public static string ExecutableDirectory => BaseDirectory;
        
        public static readonly string ProceduresMetadatJsonFile = Path.Combine(BaseDirectory, "ProceduresMetadatJsonFile.json");
        public static readonly string FunctionsMetadataJsonFile = Path.Combine(BaseDirectory, "FunctionsMetadataJsonFile.json");
        public static readonly string ViewsMetadatJsonFile = Path.Combine(BaseDirectory, "ViewsMetadatJsonFile.json");
        public static readonly string TablesMetadatJsonFile = Path.Combine(BaseDirectory, "TablesMetadatJsonFile.json");
        public static readonly string TriggersMetadataJsonFile = Path.Combine(BaseDirectory, "TriggersMetadataJsonFile.json");
    }
}
