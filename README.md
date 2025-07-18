# Database MCP Agent

Database MCP Agent is a powerful, ready-to-use tool for analyzing and interacting with database metadata through the Model Context Protocol (MCP). Get comprehensive database intelligence for AI agents with **zero setup** - just download and run!

**Currently supports Oracle databases** with planned support for SQL Server, PostgreSQL, and MySQL.

*Inspired by the [oracle-mcp-server](https://github.com/danielmeppiel/oracle-mcp-server) Python project, this .NET implementation provides enhanced features, cross-platform deployment capabilities, and a multi-database architecture.*

## 🚀 **Quick Start (2 Minute## 🗺️ **Roadmap**

### **Database Support Expansion**
- [x] **Oracle Database** - Fully supported (11g R2 - 19c+)
- [ ] **SQL Server** - In development
- [ ] **PostgreSQL** - Planned
- [ ] **MySQL** - Planned

### **Feature Enhancements**
- [ ] Implement more advanced dependency analysis
- [ ] Add database schema comparison tools
- [ ] Improve caching mechanisms for multi-database scenarios
- [ ] Add web-based dashboard
- [ ] Cross-database migration analysis tools**

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

Edit `appsettings.json` with your database connection:

**Oracle Database:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=your-oracle-host)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=your-service-name)));Persist Security Info=True;User Id=your-username;Password=your-password;"
  },
  "DatabaseProvider": "Oracle"
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

## 🛠️ **Database Support**

### **Currently Supported: Oracle Database**

**Supports Oracle 11g R2 through Oracle 19c+**

| Oracle Version | Support Level | Status |
|---------------|---------------|--------|
| **Oracle 19c** | ✅ **Fully Supported** | Recommended (Latest LTS) |
| **Oracle 18c** | ✅ **Fully Supported** | All features work perfectly |
| **Oracle 12c R2/R1** | ✅ **Fully Supported** | Extensively tested |
| **Oracle 11g R2** | ✅ **Fully Supported** | Minimum recommended version |

### **Planned Database Support**

| Database | Status | Timeline |
|----------|--------|----------|
| **SQL Server** | 🔄 **In Development** | Next Release |
| **PostgreSQL** | 📋 **Planned** | Future Release |
| **MySQL** | 📋 **Planned** | Future Release |

**👉 [Complete compatibility guide with feature matrix](ORACLE_COMPATIBILITY.md)**

## 🖥️ **Claude Desktop Integration**

Configure Claude Desktop to use your Database MCP Agent:

**Configuration File Location:**
- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
- **macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`

**Add this configuration:**

```json
{
  "mcpServers": {
    "database-mcp-agent": {
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
    "database-mcp-agent": {
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
    "database-mcp-agent": {
      "command": "/Users/yourname/Tools/database-mcp-agent/DatabaseMcp.Server",
      "args": ["--console"],
      "cwd": "/Users/yourname/Tools/database-mcp-agent"
    }
  }
}
```

Restart Claude Desktop after configuration.

**👉 See [examples/claude-desktop-config.json](examples/claude-desktop-config.json) for complete Claude Desktop configuration**

### **💻 VS Code with GitHub Copilot Integration**

**Prerequisites:**
- GitHub Copilot extension installed and configured in VS Code
- Database MCP Agent downloaded and extracted

**Setup Steps:**

1. **Install MCP Extension**: Install "MCP Client for VS Code" from the VS Code marketplace

2. **Configure MCP Server**: 
   - Open VS Code Settings (`Ctrl+,` / `Cmd+,`)
   - Search for "mcp"
   - Click "Edit in settings.json" for "MCP: Servers"

3. **Add Configuration**: Add this to your `settings.json`:

```json
{
  "mcp.servers": {
    "database-agent-server": {
      "type": "stdio",
      "command": "C:\\Tools\\database-mcp-agent\\DatabaseMcp.Server.exe",
      "args": ["--console"],
      "cwd": "C:\\Tools\\database-mcp-agent",
      "env": {
        "DOTNET_ENVIRONMENT": "Production"
      },
      "description": "Database MCP Agent for GitHub Copilot"
    }
  }
}
```

4. **Restart VS Code** to activate the MCP connection

**Usage with GitHub Copilot:**
- Use `@database-agent` in GitHub Copilot Chat
- Example prompts:
  - `@database-agent Show me the CUSTOMERS table structure`
  - `@database-agent Find all tables with EMAIL column`
  - `@database-agent What procedures depend on the ORDERS table?`
  - `@database-agent Analyze foreign key relationships`

**👉 See [examples/vscode-settings.json](examples/vscode-settings.json) for complete VS Code setup guide**

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

## 🚀 **Features & Capabilities**

The Database MCP Agent provides **25+ powerful tools** for comprehensive database analysis:

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

## 🛠️ **Complete MCP Tools Reference**

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

Restart Claude Desktop after configuration.

## 🚀 **Features & Capabilities**

The Oracle Database MCP Agent provides **25+ powerful tools** for comprehensive database analysis:

## 🛠️ **Complete MCP Tools Reference**

### 📊 **Table & Metadata Discovery**

#### **GetTablesByName**
- **Purpose**: Retrieve comprehensive metadata for specific tables by name
- **Use Cases**: Analyze table structure, understand data types, review constraints
- **Example**: *"Show me the CUSTOMERS table structure with all columns and data types"*

#### **GetColumnMetadata** 
- **Purpose**: Get detailed column information including data types, nullability, default values, and ordinal positions
- **Use Cases**: Schema analysis, data migration planning, field validation
- **Example**: *"Get all column details for the ORDERS table including constraints"*

#### **GetTablesByColumnName**
- **Purpose**: Find all tables that contain a specific column name across the entire database
- **Use Cases**: Impact analysis, finding related tables, schema exploration
- **Example**: *"Find all tables with EMAIL column"*

#### **GetColumnNames**
- **Purpose**: Quick retrieval of just the column names for a specific table
- **Use Cases**: Fast schema overview, code generation, quick reference
- **Example**: *"List all column names in the PRODUCTS table"*

#### **GetDataTypes**
- **Purpose**: Get data type information for all columns in a table
- **Use Cases**: Data type analysis, migration planning, schema documentation
- **Example**: *"Show me all data types used in the USER_ACCOUNTS table"*

#### **GetNullability**
- **Purpose**: Analyze which columns allow NULL values and which are required
- **Use Cases**: Data validation rules, required field identification, schema analysis
- **Example**: *"Which columns in EMPLOYEES table are nullable?"*

#### **GetDefaultValues**
- **Purpose**: Retrieve default values configured for table columns
- **Use Cases**: Understanding data initialization, schema documentation, migration planning
- **Example**: *"Show default values for all columns in SETTINGS table"*

### 🔗 **Relationships & Dependencies**

#### **GetForeignKeyRelationships**
- **Purpose**: Map all foreign key relationships across the entire database
- **Use Cases**: Understanding data relationships, schema visualization, impact analysis
- **Example**: *"Show me all table relationships in the database"*

#### **GetForeignKeys**
- **Purpose**: Get foreign key constraints for a specific table
- **Use Cases**: Relationship analysis, referential integrity checking, schema understanding
- **Example**: *"What foreign keys exist on the ORDERS table?"*

#### **GetPrimaryKeys**
- **Purpose**: Retrieve primary key information for tables
- **Use Cases**: Unique identifier analysis, index optimization, schema documentation
- **Example**: *"Show primary key for CUSTOMERS table"*

#### **DependentObjectsAnalysis**
- **Purpose**: Analyze what database objects depend on a specific table, view, or procedure
- **Use Cases**: Impact analysis before changes, dependency mapping, safe refactoring
- **Example**: *"What procedures and views depend on the USERS table?"*

#### **GetObjectsRelationships**
- **Purpose**: Understand relationships and dependencies for specific database objects
- **Use Cases**: Object dependency mapping, impact analysis, schema understanding
- **Example**: *"Show all objects that reference the CUSTOMER_ADDRESS table"*

### 🔒 **Constraints & Validation**

#### **GetUniqueConstraints**
- **Purpose**: Discover unique constraints defined on tables
- **Use Cases**: Data uniqueness validation, index analysis, constraint documentation
- **Example**: *"Show unique constraints on PRODUCTS table"*

#### **GetCheckConstraints**
- **Purpose**: Retrieve check constraints that enforce data validation rules
- **Use Cases**: Business rule analysis, data validation understanding, compliance checking
- **Example**: *"What check constraints are defined on EMPLOYEE_SALARY table?"*

### 📈 **Performance & Indexing**

#### **ListIndexes**
- **Purpose**: Get comprehensive index information for tables including index types and columns
- **Use Cases**: Performance optimization, query tuning, index analysis
- **Example**: *"Show all indexes on the ORDERS table with their columns"*

#### **GetIndexColumns**
- **Purpose**: Get the specific columns that make up an index
- **Use Cases**: Index composition analysis, query optimization, performance tuning
- **Example**: *"What columns are in the IDX_CUSTOMER_EMAIL index?"*

### ⚙️ **Stored Procedures & Functions**

#### **GetStoredProceduresMetadataByName**
- **Purpose**: Get complete metadata for stored procedures including parameters and source code
- **Use Cases**: Code analysis, parameter understanding, procedure documentation
- **Example**: *"Show me details for the PROCESS_ORDER procedure"*

#### **GetStoredProcedureParameters**
- **Purpose**: Get parameter information for stored procedures
- **Use Cases**: Integration planning, API development, parameter validation
- **Example**: *"What parameters does the UPDATE_CUSTOMER procedure accept?"*

#### **GetFunctionsMetadataByName**
- **Purpose**: Retrieve comprehensive function metadata including return types and parameters
- **Use Cases**: Function analysis, integration planning, code documentation
- **Example**: *"Show details for the CALCULATE_TAX function"*

#### **GetFunctionParameters**
- **Purpose**: Get parameter details for database functions
- **Use Cases**: Function integration, parameter understanding, code analysis
- **Example**: *"What parameters does GET_CUSTOMER_BALANCE function require?"*

### 👁️ **Views & Virtual Objects**

#### **GetViewDefinition**
- **Purpose**: Get view definitions including underlying SQL and structure
- **Use Cases**: View analysis, query understanding, virtual table documentation
- **Example**: *"Show me the definition of CUSTOMER_SUMMARY view"*

### 🔄 **Triggers & Automation**

#### **GetTriggersByName**
- **Purpose**: Get trigger definitions, timing, and associated tables
- **Use Cases**: Automation analysis, trigger understanding, schema documentation
- **Example**: *"Show me the AUDIT_EMPLOYEE_CHANGES trigger definition"*

### � **Synonyms & Aliases**

#### **GetSynonymMetadata**
- **Purpose**: Discover database synonyms and their target objects
- **Use Cases**: Object mapping, schema understanding, alias resolution
- **Example**: *"Show all synonyms pointing to EMPLOYEE table"*

### 💾 **Data Access & Queries**

#### **ExecuteRawSelect**
- **Purpose**: Execute SELECT statements directly against the database
- **Use Cases**: Data exploration, query testing, quick data analysis
- **Example**: *"Execute: SELECT COUNT(*) FROM ORDERS WHERE STATUS = 'PENDING'"*

### 🔄 **Cache Management**

#### **RefreshFullDBMetadata**
- **Purpose**: Refresh all cached database metadata for optimal performance
- **Use Cases**: Cache maintenance, ensuring fresh metadata, performance optimization
- **Example**: *"Refresh all database metadata cache"*

#### **RefreshTablesMetadata**
- **Purpose**: Refresh only table metadata cache
- **Use Cases**: Targeted cache refresh, table structure updates, performance tuning
- **Example**: *"Refresh table metadata after schema changes"*

#### **RefreshStoredProceduresMetadata**
- **Purpose**: Refresh stored procedure metadata cache
- **Use Cases**: Procedure cache maintenance, code deployment updates
- **Example**: *"Refresh procedure cache after deployment"*

#### **RefreshFunctionsMetadata**
- **Purpose**: Refresh function metadata cache
- **Use Cases**: Function cache maintenance, code updates
- **Example**: *"Refresh function metadata cache"*

#### **RefreshTriggersMetadata**
- **Purpose**: Refresh trigger metadata cache
- **Use Cases**: Trigger cache maintenance, automation updates
- **Example**: *"Refresh trigger metadata after changes"*

#### **RefreshViewsMetadata**
- **Purpose**: Refresh view metadata cache
- **Use Cases**: View cache maintenance, virtual object updates
- **Example**: *"Refresh view metadata cache"*

**👉 For 100+ sample prompts and detailed examples, see [MCP_TOOLS_GUIDE.md](MCP_TOOLS_GUIDE.md)**

## 📋 **Requirements**

- **Database**: Oracle 11g R2 or later (additional databases coming soon)
- **Network Access**: Connection to your database server
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

## � **Credits & Inspiration**

This .NET implementation was inspired by the excellent Python project:

**[oracle-mcp-server](https://github.com/danielmeppiel/oracle-mcp-server)** by [danielmeppiel](https://github.com/danielmeppiel)

The Python version provided the initial concept and approach for creating an Oracle database MCP server. This .NET version expands on that foundation with:
- ✅ **Cross-platform single-file executables**
- ✅ **Enhanced metadata caching and performance**
- ✅ **25+ comprehensive database analysis tools**
- ✅ **Extensive documentation and user guides**
- ✅ **Enterprise-ready deployment options**

Thank you to the original author for the inspiration and for showing the community how powerful Oracle + MCP integration can be!

## �🗺️ **Roadmap**

- [ ] Add support for SQL Server databases
- [ ] Add support for PostgreSQL databases  
- [ ] Implement more advanced dependency analysis
- [ ] Add database schema comparison tools
- [ ] Improve caching mechanisms
- [ ] Add web-based dashboard

---

**🌟 Star this repository if you find it useful!**

**Need help?** Check our comprehensive guides or open an issue for support.