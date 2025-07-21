# Oracle Database MCP Agent - Available Tools & Sample Prompts

This document provides a comprehensive list of all available MCP (Model Context Protocol) tools in the Oracle Database MCP Agent, organized by category with practical sample prompts that you can use with AI agents like Claude Desktop.

## 🗂️ Table of Contents

- [📊 Table Metadata Tools](#-table-metadata-tools)
- [🔍 Column Analysis Tools](#-column-analysis-tools)
- [🔑 Key & Constraint Tools](#-key--constraint-tools)
- [📇 Index Management Tools](#-index-management-tools)
- [👁️ View Analysis Tools](#-view-analysis-tools)
- [⚙️ Stored Procedures & Functions](#️-stored-procedures--functions)
- [🔔 Trigger Analysis Tools](#-trigger-analysis-tools)
- [🔗 Relationship & Dependency Tools](#-relationship--dependency-tools)
- [💾 Metadata Caching Tools](#-metadata-caching-tools)
- [📝 Raw SQL Execution](#-raw-sql-execution)
- [🏷️ Synonym Management](#️-synonym-management)

---

## 📊 Table Metadata Tools

### GetTablesByName
**Description**: Retrieves detailed metadata for specified tables including structure, properties, and characteristics.

**Sample Prompts**:
```
"Get metadata for the EMPLOYEES and DEPARTMENTS tables"

"Show me the table structure and properties for USER_ACCOUNTS, ORDER_HISTORY, and PRODUCT_CATALOG"

"I need detailed information about the CUSTOMER_ORDERS table"

"Analyze the metadata for all tables starting with 'INV_' - first get a list using column search, then get their metadata"
```

---

## 🔍 Column Analysis Tools

### GetColumnMetadata
**Description**: Fetches comprehensive column information including data types, nullability, default values, and ordinal positions.

**Sample Prompts**:
```
"Show me all column details for the USERS table"

"Get the complete column metadata for PRODUCT_INVENTORY including data types and constraints"

"Analyze the column structure of the FINANCIAL_TRANSACTIONS table"

"I need to understand the schema of the CUSTOMER_PROFILES table - show me all column information"
```

### GetColumnNames
**Description**: Retrieves just the column names for a specified table.

**Sample Prompts**:
```
"List all column names in the ORDERS table"

"What columns are available in the EMPLOYEE_DETAILS table?"

"Show me the column names for SALES_REPORTS"

"I need a quick list of all fields in the AUDIT_LOG table"
```

### GetDataTypes
**Description**: Fetches data type information for all columns in a table.

**Sample Prompts**:
```
"Show me the data types for all columns in the TRANSACTIONS table"

"What are the data types used in the USER_PREFERENCES table?"

"Get data type information for the INVENTORY_ITEMS table"

"Analyze the data types in the PAYMENT_METHODS table to understand storage requirements"
```

### GetNullability
**Description**: Identifies which columns allow NULL values and which are required.

**Sample Prompts**:
```
"Which columns in the CUSTOMERS table can be null?"

"Show me the nullability constraints for the ORDERS table"

"I need to understand which fields are required in the USER_REGISTRATION table"

"Analyze nullable columns in the PRODUCT_REVIEWS table for data validation"
```

### GetDefaultValues
**Description**: Retrieves default values assigned to columns.

**Sample Prompts**:
```
"Show me all default values for columns in the SETTINGS table"

"What default values are set for the USER_ACCOUNTS table?"

"Get default column values for the CONFIGURATION table"

"I need to see the default values in the SYSTEM_PARAMETERS table"
```

### GetTablesByColumnName
**Description**: Finds all tables that contain a specific column name.

**Sample Prompts**:
```
"Find all tables that have a column named 'EMAIL'"

"Which tables contain a 'CREATED_DATE' column?"

"Show me all tables with a 'USER_ID' column"

"I need to find every table that has a 'STATUS' field"

"Find all tables containing 'AMOUNT' or 'TOTAL' columns"
```

---

## 🔑 Key & Constraint Tools

### GetPrimaryKeys
**Description**: Retrieves primary key information including column names and constraint details.

**Sample Prompts**:
```
"Show me the primary key for the CUSTOMERS table"

"What are the primary key columns in the ORDER_ITEMS table?"

"Get primary key information for the EMPLOYEES table"

"I need to understand the primary key structure of the PRODUCTS table"
```

### GetForeignKeys
**Description**: Fetches foreign key relationships and constraint information for a table.

**Sample Prompts**:
```
"Show me all foreign keys in the ORDERS table"

"What foreign key relationships does the CUSTOMER_ADDRESSES table have?"

"Get foreign key constraints for the INVOICE_ITEMS table"

"I need to understand how the USER_ROLES table relates to other tables through foreign keys"
```

### GetForeignKeyRelationships
**Description**: Maps all foreign key relationships across the entire database.

**Sample Prompts**:
```
"Show me all foreign key relationships in the database"

"Map out the table relationships using foreign keys"

"I need a complete view of how tables are connected through foreign keys"

"Generate a relationship map of the database using foreign key constraints"
```

### GetUniqueConstraints
**Description**: Retrieves unique constraint information for a table.

**Sample Prompts**:
```
"Show me unique constraints on the USERS table"

"What unique constraints are defined for the PRODUCT_CODES table?"

"Get unique constraint information for the EMAIL_ADDRESSES table"

"I need to see uniqueness rules for the ACCOUNT_NUMBERS table"
```

### GetCheckConstraints
**Description**: Fetches check constraint definitions and rules.

**Sample Prompts**:
```
"Show me check constraints on the ORDERS table"

"What validation rules are defined for the SALARY table?"

"Get check constraint details for the PRODUCT_RATINGS table"

"I need to understand business rules implemented as check constraints on TRANSACTIONS"
```

---

## 📇 Index Management Tools

### ListIndexes
**Description**: Retrieves all index information for a specific table.

**Sample Prompts**:
```
"Show me all indexes on the CUSTOMERS table"

"What indexes are defined for the ORDER_HISTORY table?"

"Get index information for the SEARCH_LOGS table to understand query performance"

"I need to see all indexes on the TRANSACTIONS table for optimization"
```

### GetIndexColumns
**Description**: Shows which columns are included in a specific index.

**Sample Prompts**:
```
"Show me which columns are in the IDX_CUSTOMER_EMAIL index"

"What columns make up the PK_ORDERS index?"

"Get column details for the IDX_PRODUCT_SEARCH index"

"I need to understand the structure of the IDX_USER_ACTIVITY index"
```

---

## 👁️ View Analysis Tools

### GetViewDefinition
**Description**: Retrieves view definitions including the underlying SQL and metadata.

**Sample Prompts**:
```
"Show me the definition of the CUSTOMER_SUMMARY view"

"Get the SQL definition for the SALES_REPORT and INVENTORY_STATUS views"

"I need to understand how the USER_PERMISSIONS view is constructed"

"Show me the view definition for MONTHLY_SALES_SUMMARY"
```

---

## ⚙️ Stored Procedures & Functions

### GetStoredProceduresMetadataByName
**Description**: Retrieves detailed metadata for stored procedures including parameters and source code.

**Sample Prompts**:
```
"Show me details for the PROCESS_ORDER procedure"

"Get metadata for the UPDATE_CUSTOMER_INFO and CALCULATE_DISCOUNT procedures"

"I need to understand the GENERATE_REPORT stored procedure"

"Show me the source code and parameters for VALIDATE_USER_ACCESS"
```

### GetFunctionsMetadataByName
**Description**: Fetches function metadata including return types and parameters.

**Sample Prompts**:
```
"Show me details for the CALCULATE_TAX function"

"Get metadata for the GET_USER_PERMISSIONS and FORMAT_ADDRESS functions"

"I need to understand the VALIDATE_EMAIL function"

"Show me the source code for the CURRENCY_CONVERTER function"
```

### GetStoredProcedureParameters
**Description**: Lists all parameters for a specific stored procedure.

**Sample Prompts**:
```
"Show me all parameters for the CREATE_USER procedure"

"What parameters does the PROCESS_PAYMENT procedure accept?"

"Get parameter details for the GENERATE_INVOICE procedure"

"I need to understand the input parameters for UPDATE_INVENTORY"
```

### GetFunctionParameters
**Description**: Retrieves parameter information for a specific function.

**Sample Prompts**:
```
"Show me parameters for the CALCULATE_INTEREST function"

"What parameters does the VALIDATE_PHONE_NUMBER function require?"

"Get parameter details for the FORMAT_CURRENCY function"

"I need to understand inputs for the GET_USER_ROLE function"
```

---

## 🔔 Trigger Analysis Tools

### GetTriggersByName
**Description**: Retrieves trigger definitions and metadata.

**Sample Prompts**:
```
"Show me the AUDIT_TRIGGER definition"

"Get details for the UPDATE_TIMESTAMP and LOG_CHANGES triggers"

"I need to understand the USER_ACTIVITY_TRIGGER"

"Show me all triggers that start with 'BEFORE_' in their names"
```

---

## 🔗 Relationship & Dependency Tools

### GetObjectsRelationships
**Description**: Analyzes object dependencies and relationships in the database.

**Sample Prompts**:
```
"Show me what objects depend on the CUSTOMERS table"

"What procedures and functions reference the USER_ACCOUNTS table?"

"Get relationship information for the ORDERS table"

"I need to understand dependencies for the CALCULATE_TOTAL function"
```

### DependentObjectsAnalysis
**Description**: Performs comprehensive dependency analysis returning full definitions of dependent objects.

**Sample Prompts**:
```
"Analyze all dependencies for the PRODUCTS table and show me the dependent object definitions"

"What would be affected if I modify the USER_PROFILES table? Show me the complete analysis"

"Perform impact analysis for the ORDER_STATUS table"

"I need a complete dependency analysis for the PAYMENT_METHODS table before making changes"
```

---

## 💾 Metadata Caching Tools

### RefreshFullDBMetadata
**Description**: Refreshes the complete metadata cache for all database objects.

**Sample Prompts**:
```
"Refresh all metadata cache"

"Update the complete database metadata cache"

"I made schema changes - please refresh all cached metadata"

"Rebuild the entire metadata cache"
```

### RefreshTablesMetadata
**Description**: Refreshes only the table metadata cache.

**Sample Prompts**:
```
"Refresh the tables metadata cache"

"Update cached table information"

"I added new tables - refresh the table cache"

"Reload table metadata from the database"
```

### RefreshStoredProceduresMetadata / RefreshFunctionsMetadata / RefreshTriggersMetadata / RefreshViewsMetadata
**Description**: Refreshes specific object type caches.

**Sample Prompts**:
```
"Refresh stored procedures cache"

"Update function metadata cache"

"Refresh trigger definitions cache"

"Update view metadata cache"

"I modified some procedures - refresh the stored procedure cache"
```

---

## 📝 Raw SQL Execution

### ExecuteRawSelect
**Description**: Executes SELECT statements directly against the database.

**Sample Prompts**:
```
"Execute: SELECT COUNT(*) FROM CUSTOMERS WHERE STATUS = 'ACTIVE'"

"Run this query: SELECT TOP 10 * FROM ORDERS ORDER BY ORDER_DATE DESC"

"Execute: SELECT DISTINCT CATEGORY FROM PRODUCTS"

"Run: SELECT AVG(SALARY) FROM EMPLOYEES GROUP BY DEPARTMENT"

"Execute this SQL: SELECT TABLE_NAME FROM USER_TABLES WHERE TABLE_NAME LIKE 'SALES%'"
```

---

## 🏷️ Synonym Management

### ListSynonyms
**Description**: Retrieves all synonym definitions from the database.

**Sample Prompts**:
```
"Show me all synonyms in the database"

"List all synonym definitions"

"I need to see what synonyms are available"

"Get all synonym metadata"
```

---

## 🚀 Advanced Use Case Examples

### Database Schema Analysis
```
"I need to analyze the complete schema for tables related to user management:
1. Find all tables with 'USER' in the name
2. Get metadata for those tables
3. Show me their relationships through foreign keys
4. Analyze any dependent procedures or triggers"
```

### Pre-Migration Impact Analysis
```
"Before I modify the ORDERS table:
1. Show me all foreign keys pointing to this table
2. Find all dependent objects (procedures, functions, triggers)
3. Get the complete definitions of dependent objects
4. Show me which views use this table"
```

### Performance Optimization Analysis
```
"Help me optimize the SALES_TRANSACTIONS table:
1. Show me all indexes on this table
2. Get column metadata to understand data types
3. Find all procedures that use this table
4. Show me any check constraints that might affect performance"
```

### Data Integration Planning
```
"I'm planning to integrate with the CUSTOMER_DATA table:
1. Show me the complete column structure
2. Get all constraints (primary, foreign, unique, check)
3. Find all related tables through foreign keys
4. Show me any triggers that might affect data insertion"
```

---

## 💡 Tips for Effective Usage

1. **Combine Tools**: Use multiple tools in sequence for comprehensive analysis
2. **Start Broad**: Begin with table/column discovery, then drill down to specifics
3. **Check Dependencies**: Always analyze relationships before making schema changes
4. **Use Caching**: Refresh metadata cache after schema changes for accurate results
5. **Validate with Raw SQL**: Use ExecuteRawSelect to validate your understanding

---

## 📞 Need Help?

If you need assistance with any of these tools or have questions about specific use cases, please refer to the [README.md](README.md) or open an issue on the GitHub repository.
