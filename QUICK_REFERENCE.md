# 🚀 Oracle Database MCP Agent - Quick Reference

## Most Popular Tools

| Tool | Purpose | Sample Prompt |
|------|---------|---------------|
| `GetTablesByName` | Get table metadata | *"Show me the CUSTOMERS table structure"* |
| `GetColumnMetadata` | Analyze columns | *"Get all column details for ORDERS table"* |
| `GetTablesByColumnName` | Find tables by column | *"Find all tables with EMAIL column"* |
| `GetForeignKeyRelationships` | Map relationships | *"Show me all table relationships"* |
| `DependentObjectsAnalysis` | Impact analysis | *"What depends on USERS table?"* |
| `ExecuteRawSelect` | Run SQL queries | *"Execute: SELECT COUNT(*) FROM ORDERS"* |

## Quick Commands

### 🔍 Discovery
- *"Find all tables containing 'USER' in the name"*
- *"Show me all foreign key relationships"*
- *"What columns are in the PRODUCTS table?"*

### 📊 Analysis  
- *"Analyze the ORDERS table completely"*
- *"What procedures reference the CUSTOMERS table?"*
- *"Show me all constraints on USER_ACCOUNTS"*

### ⚙️ Maintenance
- *"Refresh all metadata cache"*
- *"Update table metadata cache"*

## 📖 Full Documentation
- **Complete Guide**: [MCP_TOOLS_GUIDE.md](MCP_TOOLS_GUIDE.md)
- **Setup Instructions**: [README.md](README.md)
- **Sample Prompts**: 100+ examples in the tools guide
