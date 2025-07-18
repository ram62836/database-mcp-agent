# Oracle Database MCP Agent

Oracle Database MCP Agent is a powerful, ready-to-use tool for analyzing and interacting with Oracle database metadata through the Model Context Protocol (MCP). Get comprehensive database intelligence for AI agents with **zero setup** - just download and run!

## 🚀 **Quick Start (2 Minutes Setup)**

### 1. Download Pre-Built Release

**No compilation needed!** Download the latest release for your platform:

**[📥 Download Latest Release](https://github.com/ram62836/database-mcp-agent/releases/latest)**

- **Windows**: `database-mcp-agent-win-x64.zip`
- **macOS**: `database-mcp-agent-osx-x64.tar.gz`

### 2. Extract and Setup

**Windows:**
```cmd
# Extract the zip file
# Run the setup script
setup.bat
```

**macOS:**
```bash
# Extract the archive
tar -xzf database-mcp-agent-osx-x64.tar.gz
cd database-mcp-agent-osx-x64

# Run the setup script
./setup.sh
```

### 3. Configure Your Database

Edit `appsettings.json` with your Oracle database connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=your-oracle-host)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=your-service-name)));Persist Security Info=True;User Id=your-username;Password=your-password;"
  }
}
```

### 4. Run the Agent

**Windows:**
```cmd
DatabaseMcp.Server.exe --console
```

**macOS:**
```bash
./DatabaseMcp.Server --console
```

**✅ That's it! Your Oracle Database MCP Agent is ready to use.**

## 🖥️ **Claude Desktop Integration**

Configure Claude Desktop to use your Oracle Database Agent:

**Configuration File Location:**
- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
- **macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`

**Add this configuration:**

```json
{
  "mcpServers": {
    "oracle-database-agent": {
      "command": "/path/to/extracted/DatabaseMcp.Server.exe",
      "args": ["--console"],
      "cwd": "/path/to/extracted/folder"
    }
  }
}
```

**Windows Example:**
```json
{
  "mcpServers": {
    "oracle-database-agent": {
      "command": "C:\Tools\database-mcp-agent\DatabaseMcp.Server.exe",
      "args": ["--console"],
      "cwd": "C:\Tools\database-mcp-agent"
    }
  }
}
```

**macOS Example:**
```json
{
  "mcpServers": {
    "oracle-database-agent": {
      "command": "/Users/yourname/Tools/database-mcp-agent/DatabaseMcp.Server",
      "args": ["--console"],
      "cwd": "/Users/yourname/Tools/database-mcp-agent"
    }
  }
}
```

Restart Claude Desktop after configuration.e MCP Agent

Oracle Database MCP Agent is a .NET solution designed to analyze, manage, and interact with Oracle database metadata through the Model Context Protocol (MCP). It provides tools and services for discovering, caching, and analyzing database objects such as tables, views, stored procedures, functions, triggers, indexes, and more.

## �️ **Oracle Database Compatibility**

**Supports Oracle 11g R2 through Oracle 19c+**

| Oracle Version | Support Level | Status |
|---------------|---------------|--------|
| **Oracle 19c** | ✅ **Fully Supported** | Recommended (Latest LTS) |
| **Oracle 18c** | ✅ **Fully Supported** | All features work perfectly |
| **Oracle 12c R2/R1** | ✅ **Fully Supported** | Extensively tested |
| **Oracle 11g R2** | ✅ **Fully Supported** | Minimum recommended version |

**👉 [Complete compatibility guide with feature matrix](ORACLE_COMPATIBILITY.md)**

## Features

- **Metadata Discovery**: Enumerate tables, views, indexes, triggers, stored procedures, and functions
- **Dependency Analysis**: Analyze object dependencies (e.g., which procedures/functions/triggers reference a table)
- **Metadata Caching**: Caches metadata in JSON files for performance with tools to refresh the cache
- **Raw SQL Execution**: Execute raw SQL queries against the Oracle database
- **MCP Integration**: Expose database tools via Model Context Protocol for use with AI agents
- **Extensible Architecture**: Modular design for easy extension and customization

## Projects

- **DatabaseMcp.Core**: Contains interfaces, models, and services for database metadata operations
- **DatabaseMcp.Server**: The main MCP server application that hosts tools for metadata analysis
- **DatabaseMcp.Client**: A simple .NET console client for testing and standalone usage
- **DatabaseMcp.Core.Tests**: Unit tests for the core functionality

## Requirements

- .NET 8.0 SDK or later
- Oracle Database (tested with Oracle 11g and later)
- Oracle.ManagedDataAccess.Core NuGet package

## Available MCP Tools

The Oracle Database MCP Agent provides **25+ powerful tools** for comprehensive database analysis:

### 📊 **Database Discovery & Analysis**
- **Table Metadata**: Get detailed table information, structure, and properties
- **Column Analysis**: Analyze data types, nullability, defaults, and find tables by column name
- **Schema Relationships**: Map foreign keys and table relationships across your database

### 🔍 **Advanced Database Intelligence** 
- **Dependency Analysis**: Understand object dependencies and impact analysis before changes
- **Constraint Analysis**: Discover primary keys, foreign keys, unique constraints, and check constraints
- **Index Optimization**: Analyze index structures and column compositions for performance tuning

### ⚙️ **Stored Code Analysis**
- **Procedures & Functions**: Get complete definitions, parameters, and source code
- **Triggers**: Analyze trigger definitions and their relationships
- **Views**: Understand view structures and underlying SQL

### 🚀 **Operational Tools**
- **Raw SQL Execution**: Execute SELECT statements directly
- **Metadata Caching**: Refresh database metadata cache for optimal performance
- **Synonym Management**: Discover and analyze database synonyms

**👉 For complete tool documentation with sample prompts, see [MCP_TOOLS_GUIDE.md](MCP_TOOLS_GUIDE.md)**

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/oracle-ai-agent.git
cd oracle-ai-agent
```

### 2. Configure Database Connection

Copy the example configuration file and update it with your database details:

```bash
# For the main project
cp appsettings.example.json appsettings.json

# For the server project
cp DatabaseMcp.Server/appsettings.example.json DatabaseMcp.Server/appsettings.json

# For the client project  
cp DatabaseMcp.Client/appsettings.example.json DatabaseMcp.Client/appsettings.json
```

Edit the `appsettings.json` files and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=your-oracle-host)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=your-service-name)));Persist Security Info=True;User Id=your-username;Password=your-password;"
  }
}
```

### 3. Build and Run

```bash
# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run the MCP server
dotnet run --project DatabaseMcp.Server
```

## Claude Desktop Integration

To use this MCP server with Claude Desktop, you'll need to configure Claude to connect to the server.

### Option 1: Development Mode (using dotnet run)

Add this to your Claude Desktop configuration file:

**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "oracle-database-agent": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\path\\to\\your\\oracle-ai-agent\\DatabaseMcp.Server"
      ]
    }
  }
}
```

### Option 2: Using Built Executable

First, publish the application:

```bash
dotnet publish DatabaseMcp.Server -c Release -o ./publish
```

Then configure Claude Desktop:

```json
## 🛠️ **Oracle Database Compatibility**

**Supports Oracle 11g R2 through Oracle 19c+**

| Oracle Version | Support Level | Status |
|---------------|---------------|--------|
| **Oracle 19c** | ✅ **Fully Supported** | Recommended (Latest LTS) |
| **Oracle 18c** | ✅ **Fully Supported** | All features work perfectly |
| **Oracle 12c R2/R1** | ✅ **Fully Supported** | Extensively tested |
| **Oracle 11g R2** | ✅ **Fully Supported** | Minimum recommended version |

**👉 [Complete compatibility guide with feature matrix](ORACLE_COMPATIBILITY.md)**

## 🚀 **Features & Capabilities**

The Oracle Database MCP Agent provides **25+ powerful tools** for comprehensive database analysis:

### 📊 **Database Discovery & Analysis**
- **Table Metadata**: Get detailed table information, structure, and properties
- **Column Analysis**: Analyze data types, nullability, defaults, and find tables by column name
- **Schema Relationships**: Map foreign keys and table relationships across your database

### 🔍 **Advanced Database Intelligence** 
- **Dependency Analysis**: Understand object dependencies and impact analysis before changes
- **Constraint Analysis**: Discover primary keys, foreign keys, unique constraints, and check constraints
- **Index Optimization**: Analyze index structures and column compositions for performance tuning

### ⚙️ **Stored Code Analysis**
- **Procedures & Functions**: Get complete definitions, parameters, and source code
- **Triggers**: Analyze trigger definitions and their relationships
- **Views**: Understand view structures and underlying SQL

### 🚀 **Operational Tools**
- **Raw SQL Execution**: Execute SELECT statements directly
- **Metadata Caching**: Refresh database metadata cache for optimal performance
- **Synonym Management**: Discover and analyze database synonyms

**👉 For complete tool documentation with 100+ sample prompts, see [MCP_TOOLS_GUIDE.md](MCP_TOOLS_GUIDE.md)**

## 📋 **Requirements**

- **Oracle Database**: 11g R2 or later
- **Network Access**: Connection to your Oracle database
- **Database Permissions**: CONNECT, RESOURCE, and SELECT access on system views
- **Operating System**: Windows 10+ or macOS 10.15+

**No .NET installation required** - Single-file executables are self-contained!

## 🔧 **Configuration Reference**

The agent uses a simple JSON configuration file (`appsettings.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)));Persist Security Info=True;User Id=hr;Password=password;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

**Common Connection String Examples:**

**Standard Connection:**
```
Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=hostname)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=servicename)));User Id=username;Password=password;
```

**Connection with SID:**
```
Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=hostname)(PORT=1521))(CONNECT_DATA=(SID=sid)));User Id=username;Password=password;
```

**Oracle Cloud (OCI):**
```
Data Source=(description=(retry_count=20)(retry_delay=3)(address=(protocol=tcps)(port=1522)(host=hostname))(connect_data=(service_name=servicename))(security=(ssl_server_cert_dn=certificatedn)));User Id=username;Password=password;
```

## 🧪 **Testing Your Setup**

After configuration, test your connection:

1. **Run the agent**: `DatabaseMcp.Server.exe --console`
2. **Look for**: `Connected to Oracle database successfully`
3. **Test with AI**: Ask Claude "Show me all tables in the database"

If you see connection errors, verify:
- ✅ Database hostname and port are correct
- ✅ Service name or SID is accurate  
- ✅ Username and password are valid
- ✅ Network connectivity to the database server
- ✅ Database permissions are sufficient

## 🔨 **For Developers: Building from Source**

If you prefer to build from source or contribute to the project:

### Prerequisites
- .NET 8.0 SDK
- Git

### Build Steps
```bash
git clone https://github.com/ram62836/database-mcp-agent.git
cd database-mcp-agent
dotnet restore
dotnet build
dotnet run --project DatabaseMcp.Server
```

### Running Tests
```bash
dotnet test
```

## 📚 **Documentation**

- **[MCP_TOOLS_GUIDE.md](MCP_TOOLS_GUIDE.md)** - Complete tools documentation with 100+ sample prompts
- **[ORACLE_COMPATIBILITY.md](ORACLE_COMPATIBILITY.md)** - Database version compatibility guide
- **[SECURITY.md](SECURITY.md)** - Security best practices and recommendations
- **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - Quick command reference

## 🤝 **Contributing**

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 **Support**

If you encounter any issues or have questions:

1. **Check the documentation** in this repository
2. **Review common issues** in [SECURITY.md](SECURITY.md)
3. **Open an issue** on the GitHub repository with:
   - Your operating system
   - Oracle database version
   - Error messages (sanitized)
   - Steps to reproduce

## 🗺️ **Roadmap**

- [ ] Add support for SQL Server databases
- [ ] Add support for PostgreSQL databases  
- [ ] Implement more advanced dependency analysis
- [ ] Add database schema comparison tools
- [ ] Improve caching mechanisms
- [ ] Add web-based dashboard

---

**🌟 Star this repository if you find it useful!**

**Need help?** Check our comprehensive guides or open an issue for support.