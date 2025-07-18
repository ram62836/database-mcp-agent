using System;
using System.IO;
using DatabaseMcp.Core;

namespace DatabaseMcp.Core
{
    public static class PathDiagnostics
    {
        public static void LogPathInformation()
        {
            Console.WriteLine("=== Path Diagnostics ===");
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"AppDomain BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
            Console.WriteLine($"Assembly Location: {System.Reflection.Assembly.GetEntryAssembly()?.Location}");
            Console.WriteLine($"Assembly Directory: {Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location)}");
            Console.WriteLine($"Executable Directory (BaseDirectory): {AppConstants.ExecutableDirectory}");
            Console.WriteLine($"Expected log file path: {Path.Combine(AppConstants.ExecutableDirectory, "DatabaseMcp.Server.log")}");
            Console.WriteLine($"Expected appsettings.json path: {Path.Combine(AppConstants.ExecutableDirectory, "appsettings.json")}");
            Console.WriteLine($"appsettings.json exists: {File.Exists(Path.Combine(AppConstants.ExecutableDirectory, "appsettings.json"))}");
            Console.WriteLine("");
            Console.WriteLine("=== JSON Cache File Paths ===");
            Console.WriteLine($"TriggersMetadataJsonFile: {AppConstants.TriggersMetadataJsonFile}");
            Console.WriteLine($"Triggers file exists: {File.Exists(AppConstants.TriggersMetadataJsonFile)}");
            Console.WriteLine($"ProceduresMetadatJsonFile: {AppConstants.ProceduresMetadatJsonFile}");
            Console.WriteLine($"Procedures file exists: {File.Exists(AppConstants.ProceduresMetadatJsonFile)}");
            Console.WriteLine($"FunctionsMetadataJsonFile: {AppConstants.FunctionsMetadataJsonFile}");
            Console.WriteLine($"Functions file exists: {File.Exists(AppConstants.FunctionsMetadataJsonFile)}");
            Console.WriteLine($"ViewsMetadatJsonFile: {AppConstants.ViewsMetadatJsonFile}");
            Console.WriteLine($"Views file exists: {File.Exists(AppConstants.ViewsMetadatJsonFile)}");
            Console.WriteLine($"TablesMetadatJsonFile: {AppConstants.TablesMetadatJsonFile}");
            Console.WriteLine($"Tables file exists: {File.Exists(AppConstants.TablesMetadatJsonFile)}");
            Console.WriteLine("========================");
        }
    }
}
