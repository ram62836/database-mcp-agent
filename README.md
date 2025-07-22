# Hala Database Agent

A modular database access age    "Hala.DatabaseAgent.OracleMcpServer": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "Hala.DatabaseAgent.OracleMcpServer@1.0.10-preview",
        "--yes"
      ],AI assistants using the Model Context Protocol (MCP).

[![NuGet Version](https://img.shields.io/nuget/v/Hala.DatabaseAgent.OracleMcpServer.svg)](https://www.nuget.org/packages/Hala.DatabaseAgent.OracleMcpServer)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Hala Database Agent** enables AI assistants to:
- Analyze database schema, tables, columns, and relationships
- Map dependencies between database objects
- Execute SQL queries and analyze results
- Discover and document stored procedures, views, and triggers

**Currently supports Oracle databases** with planned support for SQL Server, PostgreSQL, and MySQL in a modular architecture.

## Prerequisites

### .NET 10 SDK Requirement

> **Note:** The `.NET 10 SDK` is required as a prerequisite to use this package.  
> For setup and usage details, refer to the official blog:  
> [Download .NET 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
> The initial startup of the MCP server may take some time, as it caches important metadata about the database objects.

The `dnx` command used for MCP server integration requires the .NET 10 SDK. Follow the installation instructions for your platform:

## Quick Start

### VS Code Integration

1. Create a file at `.vscode/mcp.json` in your workspace with:

```json
{
  "inputs": [
    {
      "type": "promptString",
      "id": "oracle-connection-string",
      "description": "Oracle database connection string",
      "password": true
    },
    {
      "type": "promptString",
      "id": "metadata-json-path",
      "description": "Path to metadata JSON file cache",
      "password": false
    },
    {
      "type": "promptString",
      "id": "log-file-path",
      "description": "Path to store log files",
      "password": false
    }
  ],
  "servers": {
    "Hala.DatabaseMcpAgent": {
      "type": "stdio",
      "command": "dnx",
      "args": [
        "Hala.DatabaseMcpAgent@1.0.10-preview",
        "--yes"
      ],
      "env": {
        "OracleConnectionString": "${input:oracle-connection-string}",
        "MetadataCacheJsonPath": "${input:metadata-json-path}",
        "LogFilePath": "${input:log-file-path}"
      }
    }
  }
}
```

## Environment Configuration

The MCP agent uses environment variables for configuration:

### Required Configuration
```bash
# Oracle connection string
export OracleConnectionString="User Id=system;Password=oracle;Data Source=localhost:1521/XEPDB1;"
```

### Optional Configuration
```bash
# Cache and log file paths
export MetadataCacheJsonPath="/path/to/metadata/cache"
export LogFilePath="/path/to/logs"
```

### Logging
The MCP agent implements a clean logging approach:
- Console output limited to warnings and errors
- Detailed logs in log files for troubleshooting

## 💡 **Useful Prompts**

Unleash your creativity with over 25 powerful tools designed to help you craft more dynamic and effective prompts—fully leveraging the capabilities of the MCP server.

### **Performance Optimization**
- *"Analyze all indexes on the ORDERS table and suggest optimization opportunities"*
- *"The following SQL query has performance issues. Please analyze and suggest optimizations."*

### **Database Relationship Mapping**
- *"What procedures, views, and triggers depend on the CUSTOMERS table before I modify it?"*
- *"Show me the complete structure of the CUSTOMER_ORDERS view and all tables it references"*

### **Data Quality Assessment**
- *"Find all tables with missing primary keys or unique constraints for data integrity review"*

### **Security & Compliance Auditing**
- *"List all tables and views that contain personally identifiable information like EMAIL or SSN columns"*

### **Database Architecture Understanding**
- *"Find all tables that contain customer information based on column names for migration planning"*

### **Stored Procedure Documentation**
- *"Generate a bulk insert script for CUSTOMER_DATA with validation against all table constraints"*

These prompts leverage the agent's capabilities for metadata discovery, dependency analysis, constraint validation, performance optimization, and comprehensive database intelligence gathering.



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

## Claude Desktop Integration

Configure Claude Desktop to use your Database MCP Agent:

**Configuration File Location:**
- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
- **macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`

**Add this configuration:**

```json
{
  "mcpServers": {
    "Hala.DatabaseAgent.OracleMcpServer": {
      "command": "dnx",
      "args": [
        "Hala.DatabaseAgent.OracleMcpServer@1.0.2",
        "--yes"
      ],
      "env": {
        "OracleConnectionString": "YOUR_ORACLE_CONNECTION_STRING_HERE",
        "MetadataCacheJsonPath": "YOUR_METADATA_JSON_PATH_HERE", 
        "LogFilePath": "YOUR_LOG_FILE_PATH_HERE"
      }
    }
  }
}
```

Restart Claude Desktop after configuration.

For complete examples, see [examples/claude-desktop-config.json](examples/claude-desktop-config.json)

## VS Code with GitHub Copilot Integration

1. **Install GitHub Copilot Chat**: Install the GitHub Copilot Chat extension from the VS Code marketplace

2. **Create MCP Configuration File**: Create a file at `.vscode/mcp.json` in your workspace with the following content:

```json
{
  "inputs": [
    {
      "type": "promptString",
      "id": "oracle-connection-string",
      "description": "Oracle database connection string",
      "password": true
    },
    {
      "type": "promptString",
      "id": "metadata-json-path",
      "description": "Path to metadata JSON file cache",
      "password": false
    },
    {
      "type": "promptString",
      "id": "log-file-path",
      "description": "Path to store log files",
      "password": false
    }
  ],
  "servers": {
    "Hala.DatabaseMcpAgent": {
      "type": "stdio",
      "command": "dnx",
      "args": [
        "Hala.DatabaseMcpAgent@1.0.10-preview",
        "--yes"
      ],
      "env": {
        "OracleConnectionString": "${input:oracle-connection-string}",
        "MetadataCacheJsonPath": "${input:metadata-json-path}",
        "LogFilePath": "${input:log-file-path}"
      }
    }
  }
}
```

3. **Restart VS Code** to activate the MCP connection

**Usage with GitHub Copilot:**
- Use `@Hala.DatabaseAgent.OracleMcpServer` in GitHub Copilot Chat
- Example: `@Hala.DatabaseAgent.OracleMcpServer Show me the CUSTOMERS table structure`

For complete setup instructions, see [examples/vscode-mcp-config.json](examples/vscode-mcp-config.json)

## 🚀 **Features & Capabilities**

Oracle Database MCP Agent is a .NET solution designed to analyze, manage, and interact with Oracle database metadata through the Model Context Protocol (MCP). It provides tools and services for discovering, caching, and analyzing database objects such as tables, views, stored procedures, functions, triggers, indexes, and more.

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

## 🔨 **For Developers: Building from Source**

If you prefer to build from source or contribute to the project:

### Prerequisites
- .NET 10.0 SDK (required for both building and the `dnx` command)
- .NET 8.0 SDK (minimum for running the application)
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

### Alternative: Development Mode Setup

```bash
# Clone the Repository
git clone https://github.com/your-username/oracle-ai-agent.git
cd oracle-ai-agent

# Configure Database Connection
cp appsettings.example.json appsettings.json
cp DatabaseMcp.Server/appsettings.example.json DatabaseMcp.Server/appsettings.json
cp DatabaseMcp.Client/appsettings.example.json DatabaseMcp.Client/appsettings.json

# Edit appsettings.json files with your database connection details

# Build and Run
dotnet restore
dotnet build
dotnet run --project DatabaseMcp.Server
```

### Claude Desktop Integration (Development Mode)

For development mode using `dotnet run`:

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

## 📋 **Requirements**

- **.NET SDK**: .NET 10.0 SDK (required for the `dnx` command)
- **Database**: Oracle 11g R2 or later (additional databases coming soon)
- **Network Access**: Connection to your database server
- **Database Permissions**: CONNECT, RESOURCE, and SELECT access on system views
- **Operating System**: Windows 10+ or macOS 10.15+

For NuGet package usage with `dnx`, the .NET 10.0 SDK is required. For direct executable usage, no additional .NET installation is required as the executables are self-contained.

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

## 🏗️ **Architecture**

The solution consists of several projects:

- **DatabaseMcp.Core**: Contains interfaces, models, and services for database metadata operations
- **DatabaseMcp.Server**: The main MCP server application that hosts tools for metadata analysis
- **DatabaseMcp.Client**: A simple .NET console client for testing and standalone usage
- **DatabaseMcp.Core.Tests**: Unit tests for the core functionality

## 📚 **Features**

- **Metadata Discovery**: Enumerate tables, views, indexes, triggers, stored procedures, and functions
- **Dependency Analysis**: Analyze object dependencies (e.g., which procedures/functions/triggers reference a table)
- **Metadata Caching**: Caches metadata in JSON files for performance with tools to refresh the cache
- **Raw SQL Execution**: Execute raw SQL queries against the Oracle database
- **MCP Integration**: Expose database tools via Model Context Protocol for use with AI agents
- **Extensible Architecture**: Modular design for easy extension and customization

## 📚 **Documentation**

- **[MCP_TOOLS_GUIDE.md](MCP_TOOLS_GUIDE.md)** - Complete tools documentation with 100+ sample prompts
- **[ORACLE_COMPATIBILITY.md](ORACLE_COMPATIBILITY.md)** - Database version compatibility guide
- **[SECURITY.md](SECURITY.md)** - Security best practices and recommendations
- **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - Quick command reference

## 🗺️ **Roadmap**

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
- [ ] Cross-database migration analysis tools

## 🤝 **Contributing**

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ⚠️ **Disclaimer**

This software is provided "as-is" without any warranties or guarantees. The authors and contributors are **not responsible for any data loss, corruption, or other impacts** resulting from the use of this package. Use it at your own risk.

## 🆘 **Support**

If you encounter any issues or have questions:

1. **Check the documentation** in this repository
2. **Review common issues** in [SECURITY.md](SECURITY.md)
3. **Open an issue** on the GitHub repository with:
   - Your operating system
   - Oracle database version
   - Error messages (sanitized)
   - Steps to reproduce

## 🙏 **Credits & Inspiration**

This .NET implementation was inspired by the excellent Python project:

**[oracle-mcp-server](https://github.com/danielmeppiel/oracle-mcp-server)** by [danielmeppiel](https://github.com/danielmeppiel)

The Python version provided the initial concept and approach for creating an Oracle database MCP server. This .NET version expands on that foundation with:
- ✅ **Cross-platform single-file executables**
- ✅ **Enhanced metadata caching and performance**
- ✅ **25+ comprehensive database analysis tools**
- ✅ **Extensive documentation and user guides**
- ✅ **Enterprise-ready deployment options**

Thank you to the original author for the inspiration and for showing the community how powerful Oracle + MCP integration can be!

---

**🌟 Star this repository if you find it useful!**

**Need help?** Check our comprehensive guides or open an issue for support.