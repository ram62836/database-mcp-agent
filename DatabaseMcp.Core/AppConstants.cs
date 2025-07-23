using System;
using System.IO;

namespace DatabaseMcp.Core
{
    public static class AppConstants
    {
        /// <summary>
        /// Gets the directory where metadata files are stored.
        /// </summary>
        public static string ExecutableDirectory { get; } = GetBaseDirectory();

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
                    _ = Directory.CreateDirectory(path);
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
                string tempPath = Path.Combine(Path.GetTempPath(), "DatabaseMcpAgent", "metadata");
                _ = Directory.CreateDirectory(tempPath);
                return tempPath;
            }
        }

        public static readonly string ProceduresMetadatJsonFile = Path.Combine(ExecutableDirectory, "ProceduresMetadatJsonFile.json");
        public static readonly string FunctionsMetadataJsonFile = Path.Combine(ExecutableDirectory, "FunctionsMetadataJsonFile.json");
        public static readonly string ViewsMetadatJsonFile = Path.Combine(ExecutableDirectory, "ViewsMetadatJsonFile.json");
        public static readonly string TablesMetadatJsonFile = Path.Combine(ExecutableDirectory, "TablesMetadatJsonFile.json");
        public static readonly string TriggersMetadataJsonFile = Path.Combine(ExecutableDirectory, "TriggersMetadataJsonFile.json");
    }
}
