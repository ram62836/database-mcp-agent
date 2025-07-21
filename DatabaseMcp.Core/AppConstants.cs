using System;
using System.IO;

namespace DatabaseMcp.Core
{
    public static class AppConstants
    {
        // Get the directory for storing metadata files
        private static readonly string BaseDirectory = GetBaseDirectory();
        
        /// <summary>
        /// Gets the directory where metadata files are stored.
        /// </summary>
        public static string ExecutableDirectory => BaseDirectory;
        
        /// <summary>
        /// Gets a directory that's guaranteed to exist and be writable for storing metadata.
        /// </summary>
        private static string GetBaseDirectory()
        {
            // First try environment variable
            string path = Environment.GetEnvironmentVariable("MetadataCacheJsonPath");
            
            if (string.IsNullOrWhiteSpace(path))
            {
                // Fall back to a subdirectory of the executable directory
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "metadata");
            }
            
            try
            {
                // Ensure directory exists
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                
                // Test write permissions
                string testFile = Path.Combine(path, "test_write.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                
                return path;
            }
            catch (Exception)
            {
                // If there are permission issues, try a temp directory
                var tempPath = Path.Combine(Path.GetTempPath(), "DatabaseMcpAgent", "metadata");
                Directory.CreateDirectory(tempPath);
                return tempPath;
            }
        }
        
        public static readonly string ProceduresMetadatJsonFile = Path.Combine(BaseDirectory, "ProceduresMetadatJsonFile.json");
        public static readonly string FunctionsMetadataJsonFile = Path.Combine(BaseDirectory, "FunctionsMetadataJsonFile.json");
        public static readonly string ViewsMetadatJsonFile = Path.Combine(BaseDirectory, "ViewsMetadatJsonFile.json");
        public static readonly string TablesMetadatJsonFile = Path.Combine(BaseDirectory, "TablesMetadatJsonFile.json");
        public static readonly string TriggersMetadataJsonFile = Path.Combine(BaseDirectory, "TriggersMetadataJsonFile.json");
    }
}
