# Executable Directory Configuration Guide

## Overview

The DatabaseMcp.Server has been configured to use the executable's directory as the base location for all file operations when published as a standalone executable. This ensures consistent behavior regardless of the current working directory when the MCP server is launched.

## File Locations

When the application is published and deployed, all files will be located relative to the executable directory:

### Configuration Files
- **appsettings.json**: Located in the same directory as `DatabaseMcp.Server.exe`
- **tool-metadata.json**: Located in the same directory as `DatabaseMcp.Server.exe`

### Cache Files (JSON Metadata)
All database metadata cache files are stored in the executable directory:
- `ProceduresMetadatJsonFile.json`
- `FunctionsMetadataJsonFile.json`
- `ViewsMetadatJsonFile.json`
- `TablesMetadatJsonFile.json`
- `TriggersMetadataJsonFile.json`

### Log Files
- **DatabaseMcp.Server{date}.log**: Application logs with daily rolling

## Technical Implementation

### AppConstants.cs
```csharp
// Get the directory where the executable is located
private static readonly string BaseDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) 
                                              ?? AppDomain.CurrentDomain.BaseDirectory;

public static string ExecutableDirectory => BaseDirectory;
```

### Program.cs
```csharp
// Use the executable directory for all file operations
string executableDirectory = AppConstants.ExecutableDirectory;
string logPath = Path.Combine(executableDirectory, "DatabaseMcp.Server.log");

// Configuration setup
config.SetBasePath(executableDirectory);
config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
```

## VS Code/Claude Integration

When integrating with VS Code or Claude Desktop, the MCP server will:

1. **Always use the executable directory** for file operations, regardless of where it's called from
2. **Find appsettings.json** in the same directory as the executable
3. **Create and read cache files** in the executable directory
4. **Generate logs** in the executable directory

### Example Claude Desktop Configuration
```json
{
  "mcpServers": {
    "database-mcp-agent": {
      "command": "C:\\Path\\To\\Your\\Build\\DatabaseMcp.Server.exe",
      "args": ["--console"],
      "cwd": "C:\\Path\\To\\Your\\Build"
    }
  }
}
```

## Benefits

1. **Portable Deployment**: The entire application and its data stay in one directory
2. **Consistent Behavior**: Works the same regardless of calling context
3. **Easy Backup**: All application data is in one location
4. **No Permission Issues**: Doesn't rely on user directories or system paths
5. **MCP Integration Ready**: Works reliably when called from VS Code or Claude Desktop

## Path Diagnostics

The application includes diagnostic logging at startup that shows:
- Current working directory
- Executable directory
- Expected file paths
- File existence status

This helps troubleshoot any path-related issues during deployment or integration.

## Migration Notes

- Previous versions used `Directory.GetCurrentDirectory()` which could vary
- New version consistently uses `Assembly.GetEntryAssembly()?.Location` directory
- Log file naming changed from `oracleagent.log` to `DatabaseMcp.Server.log`
- All JSON cache files remain in the same naming convention but now use executable directory
