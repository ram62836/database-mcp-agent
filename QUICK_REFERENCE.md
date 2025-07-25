# 🚀 Oracle Database MCP Agent - Quick Reference

## ⚡ **Instant Setup**

### 1. Download & Extract
**[� Get Latest Release](https://github.com/ram62836/database-mcp-agent/releases/latest)**
- Windows: `database-mcp-agent-win-x64.zip`
- macOS: `database-mcp-agent-osx-x64.tar.gz`

### 2. Setup & Run
```bash
# Windows
setup.bat
DatabaseMcp.Server.exe --console

# macOS  
./setup.sh
./DatabaseMcp.Server --console
```

### 3. Configure Claude Desktop
Add to your Claude config file:
```json
{
  "mcpServers": {
    "oracle-database-agent": {
      "command": "/path/to/DatabaseMcp.Server.exe",
      "args": ["--console"]
    }
  }
}
```

## �🗄️ **Oracle Compatibility**
**Supports Oracle 11g R2 through Oracle 19c+** | **✅ No .NET installation required**

## 🛠️ **Most Popular Tools**

| Tool | Purpose | Sample Prompt |
|------|---------|---------------|
| `GetTablesByName` | Get table metadata | *"Show me the CUSTOMERS table structure"* |
| `GetColumnMetadata` | Analyze columns | *"Get all column details for ORDERS table"* |
| `GetTablesByColumnName` | Find tables by column | *"Find all tables with EMAIL column"* |
| `GetForeignKeyRelationships` | Map relationships | *"Show me all table relationships"* |
| `DependentObjectsAnalysis` | Impact analysis | *"What depends on USERS table?"* |
| `ExecuteRawSelect` | Run SQL queries | *"Execute: SELECT COUNT(*) FROM ORDERS"* |

## 🎯 **Quick Commands**

### 🔍 **Discovery**
- *"Find all tables containing 'USER' in the name"*
- *"Show me all foreign key relationships"*
- *"What columns are in the PRODUCTS table?"*
- *"List all indexes on the ORDERS table"*

### 📊 **Analysis**  
- *"Analyze the ORDERS table completely"*
- *"What procedures reference the CUSTOMERS table?"*
- *"Show me all constraints on USER_ACCOUNTS"*
- *"Find all tables that reference PRODUCT_ID"*

### ⚙️ **Maintenance**
- *"Refresh all metadata cache"*
- *"Update table metadata cache"*
- *"Show me all stored procedures"*

## � **Configuration Files**

### Database Connection (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=hostname)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=servicename)));User Id=username;Password=password;"
  }
}
```

### Claude Desktop Config Locations
- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
- **macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`

## 🚨 **Common Issues & Solutions**

| Issue | Solution |
|-------|----------|
| Connection failed | ✅ Check hostname, port, service name |
| Permission denied | ✅ Verify database user has CONNECT + SELECT rights |
| File not found | ✅ Use full absolute paths in Claude config |
| Process won't start | ✅ Run `setup.bat` or `setup.sh` first |

## 📖 **Complete Documentation**
- **🔧 Setup Guide**: [README.md](README.md) - Complete installation and configuration
- **🛠️ All Tools**: [MCP_TOOLS_GUIDE.md](MCP_TOOLS_GUIDE.md) - 30+ tools with 100+ sample prompts
- **🔒 Security**: [SECURITY.md](SECURITY.md) - Best practices and recommendations
- **🗃️ Compatibility**: [ORACLE_COMPATIBILITY.md](ORACLE_COMPATIBILITY.md) - Database version matrix

---
**⭐ Single-file executables • No dependencies • Ready to use**
