# Oracle Database MCP Agent

Oracle Database MCP Agent with 25+ tools for AI-powered database analysis.

## � VS Code Integration

Add to your VS Code MCP settings to automatically install and configure:

```json
{
  "mcp.servers": {
    "oracle-database": {
      "command": "dnx",
      "args": ["Hala.DatabaseMcpAgent", "--version", "1.0.7-preview", "--yes"],
      "env": {
        "OracleConnectionString": "your-oracle-connection-string"
      }
    }
  }
}
```

The `dnx` command will automatically download and install the package when VS Code starts the MCP server.

## ⚡ Configuration

Set your Oracle connection string in the environment variables:

**Required:**
- `OracleConnectionString` - Your Oracle database connection string

**Optional:**
- `DatabaseMcp__CacheExpirationMinutes=30` - Cache expiration time
- `DatabaseMcp__MaxConnectionRetries=3` - Connection retry attempts

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
dotnet tool install -g Hala.DatabaseMcpAgent --version 1.0.7-preview
```

Then run directly: `database-mcp-agent`

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
