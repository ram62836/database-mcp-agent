# 📚 Oracle Database MCP Agent - Tools Reference

This document details all tools available in the Oracle Database MCP Agent. Each tool is documented with its purpose, parameters, example usage, and sample prompts.

## Table of Contents

- [Table & Metadata Discovery](#table--metadata-discovery)
- [Column Analysis Tools](#column-analysis-tools)
- [Key & Constraint Analysis](#key--constraint-analysis)
- [Index Management](#index-management)
- [Relationship Analysis](#relationship-analysis)
- [Stored Procedures & Functions](#stored-procedures--functions)
- [Views & Triggers](#views--triggers)
- [Synonyms & Packages](#synonyms--packages)
- [Performance Analytics & Optimization](#performance-analytics--optimization)
- [Raw SQL Execution](#raw-sql-execution)

## Table & Metadata Discovery

### GetTablesByName

Retrieves comprehensive metadata for specific tables by name.

**Example Usage:**
```
Show me the CUSTOMERS table structure with all columns and data types
```

**Sample Prompts:**
- "Describe the EMPLOYEES table in detail"
- "Get full metadata for the ORDER_ITEMS and ORDERS tables"
- "What is the structure of the PRODUCTS table?"
- "Show schema details for the USER_ACCOUNTS table"

### GetTablesByColumnName

Find all tables that contain a specific column name across the entire database.

**Example Usage:**
```
Find all tables with EMAIL column
```

**Sample Prompts:**
- "Which tables have a CUSTOMER_ID column?"
- "Show me all tables containing PRODUCT_NAME field"
- "Find tables with CREATION_DATE column"
- "List all tables with USER_ID field"

## Column Analysis Tools

### GetColumnMetadata

Get detailed column information including data types, nullability, default values, and ordinal positions.

**Example Usage:**
```
Get all column details for the ORDERS table including constraints
```

**Sample Prompts:**
- "Show me complete column information for CUSTOMERS table"
- "What are all the column properties in the INVENTORY table?"
- "Detail the columns in PRODUCTS table with their data types and nullability"
- "Get the full metadata for columns in USER_PROFILES table"

### GetColumnNames

Quick retrieval of just the column names for a specific table.

**Example Usage:**
```
List all column names in the PRODUCTS table
```

**Sample Prompts:**
- "What columns are in the EMPLOYEES table?"
- "Show me just the column names from ORDERS table"
- "List all fields in the CUSTOMERS table"
- "What are the column names in INVENTORY?"

### GetDataTypes

Get data type information for all columns in a table.

**Example Usage:**
```
Show me all data types used in the USER_ACCOUNTS table
```

**Sample Prompts:**
- "What data types are used in the PRODUCTS table?"
- "Show me the column data types for ORDERS table"
- "List the data types for all columns in CUSTOMERS"
- "What types of data are stored in the INVENTORY table?"

### GetNullability

Analyze which columns allow NULL values and which are required.

**Example Usage:**
```
Which columns in EMPLOYEES table are nullable?
```

**Sample Prompts:**
- "Show me which fields in CUSTOMERS are required vs optional"
- "List nullable columns in the ORDERS table"
- "Which columns in PRODUCTS can have NULL values?"
- "Get required fields from USER_ACCOUNTS table"

### GetDefaultValues

Retrieve default values for all columns in a specified table.

**Example Usage:**
```
What are the default values for columns in the ORDERS table?
```

**Sample Prompts:**
- "Show default values for CUSTOMERS table columns"
- "What default values are set in the PRODUCTS table?"
- "List all columns with default values in USER_ACCOUNTS"
- "Get default values for INVENTORY table fields"

## Key & Constraint Analysis

### GetPrimaryKeys

Retrieves the primary key metadata for the specified table, including column names and constraints.

**Example Usage:**
```
What is the primary key for the CUSTOMERS table?
```

**Sample Prompts:**
- "Show me the primary key columns in ORDERS table"
- "Get primary key information for PRODUCTS table"
- "What columns make up the primary key in USER_ACCOUNTS?"
- "Identify the primary key in INVENTORY table"

### GetForeignKeys

Retrieves the foreign key metadata for the specified table, including column names and constraints.

**Example Usage:**
```
Show foreign key relationships for the ORDERS table
```

**Sample Prompts:**
- "What foreign keys are defined in the ORDER_ITEMS table?"
- "List all foreign keys in EMPLOYEES table"
- "Show me the foreign key constraints in CUSTOMER_ADDRESSES"
- "Get foreign key details for PRODUCT_CATEGORIES"

### GetForeignKeyRelationships

Retrieves all foreign key relationships in the database, mapping table names to their related tables.

**Example Usage:**
```
Show me all table relationships in the database
```

**Sample Prompts:**
- "Map all foreign key relationships in the system"
- "Show me a complete picture of table relationships"
- "How are tables connected through foreign keys?"
- "Generate a relationship map of database tables"

### GetUniqueConstraints

Retrieves metadata for unique constraints defined on the specified table.

**Example Usage:**
```
Show unique constraints on the EMPLOYEES table
```

**Sample Prompts:**
- "What unique constraints exist on CUSTOMERS table?"
- "List unique constraints for USER_ACCOUNTS"
- "Show me fields with uniqueness constraints in PRODUCTS"
- "Get all unique constraint details from ORDERS table"

### GetCheckConstraints

Retrieves metadata for check constraints defined on the specified table.

**Example Usage:**
```
What check constraints are defined on the ORDERS table?
```

**Sample Prompts:**
- "Show me all check constraints in PRODUCTS table"
- "List the data validation rules on EMPLOYEES table"
- "What check constraints validate INVENTORY data?"
- "Get all check constraints from USER_ACCOUNTS"

## Index Management

### ListIndexes

Fetches metadata for all indexes defined on the specified table.

**Example Usage:**
```
List all indexes on the CUSTOMERS table
```

**Sample Prompts:**
- "Show me all indexes on the ORDERS table"
- "What indexes exist for PRODUCTS?"
- "List all defined indexes on USER_ACCOUNTS table"
- "Get index information for INVENTORY table"

### GetIndexColumns

Fetches the names of columns associated with the specified index.

**Example Usage:**
```
What columns are included in the IDX_CUSTOMERS_EMAIL index?
```

**Sample Prompts:**
- "Show columns in the IDX_ORDERS_DATE index"
- "What fields make up the PK_PRODUCTS index?"
- "Get columns used in IDX_USER_SEARCH index"
- "List columns for IDX_INVENTORY_STATUS"

## Relationship Analysis

### DependentObjectsAnalysis

Analyzes dependencies for a given object and returns the dependent objects definitions.

**Example Usage:**
```
What depends on the CUSTOMERS table?
```

**Sample Prompts:**
- "Show me everything that references the ORDERS table"
- "What would be impacted if I modify the PRODUCTS table?"
- "Find all objects dependent on USER_ACCOUNTS table"
- "What stored procedures reference the INVENTORY table?"

### GetObjectsRelationships

Fetches and returns metadata about Oracle database objects that the specified object depends on or references.

**Example Usage:**
```
What other objects does the PROCESS_ORDER procedure depend on?
```

**Sample Prompts:**
- "Show me all tables and views the CALC_TOTALS function uses"
- "What dependencies does the CUSTOMER_DETAILS view have?"
- "Map out what objects the UPDATE_INVENTORY procedure needs"
- "What tables does the SALES_REPORT view reference?"

## Stored Procedures & Functions

### GetStoredProceduresMetadataByName

Get complete metadata for stored procedures including parameters and source code.

**Example Usage:**
```
Show me details for the PROCESS_ORDER procedure
```

**Sample Prompts:**
- "Get source code for the UPDATE_CUSTOMER procedure"
- "Show me the implementation of CREATE_ORDER procedure"
- "What does the CALCULATE_TOTALS procedure do?"
- "Get full definition of VALIDATE_USER procedure"

### GetStoredProcedureParameters

Get parameter information for stored procedures.

**Example Usage:**
```
What parameters does the UPDATE_CUSTOMER procedure accept?
```

**Sample Prompts:**
- "Show me input parameters for PROCESS_ORDER procedure"
- "What parameters does CREATE_USER procedure require?"
- "List all parameters of GENERATE_REPORT procedure"
- "Get parameter details for VALIDATE_PAYMENT"

### GetFunctionsMetadataByName

Retrieves comprehensive function metadata including return types and parameters.

**Example Usage:**
```
Show details for the CALCULATE_TAX function
```

**Sample Prompts:**
- "Get source code for GET_CUSTOMER_NAME function"
- "What does the VALIDATE_EMAIL function do?"
- "Show me the implementation of CALCULATE_DISCOUNT"
- "Get the full definition of IS_VALID_USER function"

### GetFunctionParameters

Get parameter details for database functions.

**Example Usage:**
```
What parameters does GET_CUSTOMER_BALANCE function require?
```

**Sample Prompts:**
- "Show parameters for CALCULATE_TAX function"
- "What inputs does the GET_PRODUCT_PRICE function need?"
- "List all parameters for IS_IN_STOCK function"
- "Get parameter details for FORMAT_ADDRESS function"

## Views & Triggers

### GetViewDefinition

Fetches metadata for views by their names.

**Example Usage:**
```
Show me the SQL definition of the ACTIVE_CUSTOMERS view
```

**Sample Prompts:**
- "What's the query behind the ORDER_SUMMARY view?"
- "Show me the definition of PRODUCT_INVENTORY view"
- "Get the SQL that makes up the USER_PROFILES view"
- "What tables does the SALES_DASHBOARD view reference?"

### GetTriggersByName

Fetches metadata for the given trigger names.

**Example Usage:**
```
Show me the definition of the BEFORE_ORDER_INSERT trigger
```

**Sample Prompts:**
- "Get details for the AFTER_PRODUCT_UPDATE trigger"
- "What does the BEFORE_USER_DELETE trigger do?"
- "Show me the source code for AFTER_INVENTORY_CHANGE"
- "Get trigger definition for BEFORE_CUSTOMER_UPDATE"

## Synonyms & Packages

### GetSynonymsByName

Fetches metadata for synonyms by their names.

**Example Usage:**
```
What does the PROD_CUSTOMERS synonym point to?
```

**Sample Prompts:**
- "Show me details for the CURRENT_ORDERS synonym"
- "What object is the ACTIVE_USERS synonym for?"
- "Get target information for the PRODUCT_CATALOG synonym"
- "List all public synonyms in the database"

### GetPackagesByName

Fetches metadata for Oracle packages by their names.

**Example Usage:**
```
Show me the definition of the ORDER_MANAGEMENT package
```

**Sample Prompts:**
- "What procedures and functions are in the CUSTOMER_API package?"
- "Get the source code for the REPORTING_UTILS package"
- "Show me the implementation of USER_SECURITY package"
- "Get package details for INVENTORY_MANAGEMENT"

## Performance Analytics & Optimization

### GetTopSqlByPerformance

Analyze top SQL statements by various performance metrics (executions, CPU time, elapsed time, disk reads, buffer gets).

**Example Usage:**
```
Show me the top 10 SQL statements by CPU time consumption
```

**Sample Prompts:**
- "What are the most resource-intensive queries in the database?"
- "Find SQL statements with the highest execution counts"
- "Show me queries with the worst performance overall"
- "Identify SQL statements causing the most disk I/O"

### GetTopSqlByExecutions

Find the most frequently executed SQL statements in the database.

**Example Usage:**
```
What are the top 5 most executed SQL queries in the last hour?
```

**Sample Prompts:**
- "Show me the most frequently run queries in the system"
- "Which SQL statements are executed most often?"
- "Get the top 20 queries by execution count"
- "What queries are running most frequently since midnight?"

### GetTopSqlByCpuTime

Identify SQL statements consuming the most CPU resources.

**Example Usage:**
```
Show me SQL statements with highest CPU time consumption
```

**Sample Prompts:**
- "Which queries use the most CPU resources?"
- "Identify CPU-intensive SQL statements in the database"
- "Get the top 15 SQL statements by CPU consumption"
- "What queries have used the most CPU in the last 24 hours?"

### GetTopSqlByElapsedTime

Find SQL statements with the longest execution times.

**Example Usage:**
```
Find the slowest running SQL queries in the database
```

**Sample Prompts:**
- "Show me the queries with the longest execution times"
- "What SQL statements take the most time to complete?"
- "Identify the slowest queries in the system"
- "Get top 10 SQL statements by total elapsed time"

### GetWaitEventAnalysis

Analyze database wait events to identify performance bottlenecks and contention points.

**Example Usage:**
```
What are the current database wait events and their impact?
```

**Sample Prompts:**
- "Show me all wait events affecting the ORDERS table"
- "What are the top database wait events right now?"
- "Identify the main bottlenecks in the database"
- "What's causing the most significant waits in the system?"

### GetTableUsageStatistics

Analyze table usage patterns including scans, lookups, DML operations, and access patterns.

**Example Usage:**
```
Show me usage statistics for the ORDERS and CUSTOMERS tables
```

**Sample Prompts:**
- "How frequently is the PRODUCTS table accessed?"
- "Get usage metrics for the USER_ACCOUNTS table"
- "Show me read vs write patterns for INVENTORY table"
- "Which operations are most common on the ORDERS table?"

### GetIndexUsageStatistics

Analyze how indexes are being used for a specific table.

**Example Usage:**
```
How are indexes being used on the CUSTOMERS table?
```

**Sample Prompts:**
- "Show me index efficiency for the ORDERS table"
- "Which indexes are most used on PRODUCTS table?"
- "Get usage statistics for indexes on USER_ACCOUNTS"
- "Are all indexes on INVENTORY table being used?"

### GetUnusedIndexes

Identifies unused indexes that could be dropped to improve DML performance.

**Example Usage:**
```
Find all unused indexes in the database
```

**Sample Prompts:**
- "Show me indexes that could be dropped to improve performance"
- "Are there any unused indexes on the ORDERS table?"
- "Identify inefficient indexes in the database"
- "List all indexes that haven't been used in the last month"

## Raw SQL Execution

### ExecuteRawSelect

Execute SELECT statements directly against the database.

**Example Usage:**
```
Execute: SELECT COUNT(*) FROM ORDERS WHERE STATUS = 'PENDING'
```

**Sample Prompts:**
- "Run query: SELECT * FROM CUSTOMERS LIMIT 10"
- "Execute: SELECT AVG(TOTAL_AMOUNT) FROM ORDERS GROUP BY STATUS"
- "Run: SELECT PRODUCT_NAME, PRICE FROM PRODUCTS WHERE CATEGORY='Electronics'"
- "Execute SQL: SELECT COUNT(DISTINCT USER_ID) FROM SESSION_LOGS"


