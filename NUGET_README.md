# Oracle Database MCP Agent

Oracle Database MCP Agent with 25+ tools for AI-powered database analysis.

##  VS Code Integration

Add to your VS Code MCP settings to automatically install and configure:

```json
{
  "servers": {
    "oracle-agent-server": {
      "command": "dnx",
      "args": ["Hala.DatabaseAgent.OracleMcpServer", "--version", "1.0.2", "--yes"],
      "env": {
        "OracleConnectionString": "your-oracle-connection-string",
        "MetadataCacheJsonPath": "directory-path-to-store-metadata-cache",
        "LogFilePath": "log-file-directory"
      }
    }
  }
}
```

The `dnx` command will automatically download and install the package when VS Code starts the MCP server.

## ⚡ Configuration

Set your Oracle connection string in the environment variables:

> **Note:** The `.NET 10 SDK` is required as a prerequisite to use this package, to make use of dnx command.  
> For setup and usage details, refer to the official blog:  
> [Download .NET 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
> The initial startup of the MCP server may take some time, as it caches important metadata about the database objects.

**Required:**
- `OracleConnectionString` - Your Oracle database connection string

## 🔍 Example Connection Strings

```bash
# Standard Oracle connection
Host=localhost;Port=1521;Database=ORCL;User Id=hr;Password=password;

# Oracle with TNS
Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=server)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=service)));User Id=user;Password=pass;
```

## � Manual Installation (Optional)

If you prefer to install manually:

```bash
dotnet tool install -g Hala.DatabaseAgent.OracleMcpServer --version 1.0.2
```

Then run directly: `oracle-mcp-server`

## �📚 Documentation

- **[GitHub Repository](https://github.com/ram62836/database-mcp-agent)** - Full documentation and source code
- **[MCP Tools Guide](https://github.com/ram62836/database-mcp-agent/blob/main/MCP_TOOLS_GUIDE.md)** - Complete tools reference
- **[Quick Reference](https://github.com/ram62836/database-mcp-agent/blob/main/QUICK_REFERENCE.md)** - Command reference

## 🎯 Features

- **25+ MCP Tools** for database analysis
- **Metadata Discovery** - Tables, views, procedures, functions
- **Dependency Analysis** - Impact analysis before schema changes
- **Performance Tools** - Index analysis and optimization
- **AI Integration** - Seamless integration with AI assistants

## 📋 Requirements

- .NET 8.0+
- Oracle Database 11g R2+
- Network access to Oracle database

## ⚠️ Disclaimer

This software is provided "as-is" without any warranties or guarantees. The authors and contributors are **not responsible for any data loss, corruption, or other impacts** resulting from the use of this package. Use it at your own risk.
