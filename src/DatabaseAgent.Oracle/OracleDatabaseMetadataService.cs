using Hala.DatabaseAgent.Core.Interfaces;
using Hala.DatabaseAgent.Core.Models;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Hala.DatabaseAgent.Oracle
{
    /// <summary>
    /// Oracle implementation of database metadata service
    /// </summary>
    public class OracleDatabaseMetadataService : IDatabaseMetadataService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ISqlExecutionService _sqlExecutionService;
        private readonly ILogger<OracleDatabaseMetadataService> _logger;

        // Cache for metadata
        private Dictionary<string, object> _metadataCache = new Dictionary<string, object>();
        private Dictionary<string, DateTime> _cacheExpiration = new Dictionary<string, DateTime>();
        private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(60);

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDatabaseMetadataService"/> class.
        /// </summary>
        /// <param name="connectionFactory">The connection factory.</param>
        /// <param name="sqlExecutionService">The SQL execution service.</param>
        /// <param name="logger">The logger.</param>
        public OracleDatabaseMetadataService(
            IDbConnectionFactory connectionFactory,
            ISqlExecutionService sqlExecutionService,
            ILogger<OracleDatabaseMetadataService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _sqlExecutionService = sqlExecutionService ?? throw new ArgumentNullException(nameof(sqlExecutionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<List<TableMetadata>> GetAllUserDefinedTablesAsync()
        {
            const string cacheKey = "all_tables";
            
            if (TryGetFromCache<List<TableMetadata>>(cacheKey, out var cachedTables))
            {
                return cachedTables;
            }
            
            var tables = new List<TableMetadata>();
            
            string schema = _connectionFactory.GetDefaultSchema().ToUpper();
            string sql = @"
                SELECT 
                    t.TABLE_NAME,
                    t.OWNER,
                    t.NUM_ROWS,
                    t.LAST_ANALYZED,
                    TO_CHAR(o.CREATED, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE,
                    TO_CHAR(o.LAST_DDL_TIME, 'YYYY-MM-DD HH24:MI:SS') AS LAST_DDL_TIME
                FROM 
                    ALL_TABLES t
                JOIN
                    ALL_OBJECTS o ON t.TABLE_NAME = o.OBJECT_NAME AND t.OWNER = o.OWNER AND o.OBJECT_TYPE = 'TABLE'
                WHERE 
                    t.OWNER = :schema
                ORDER BY 
                    t.TABLE_NAME";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var tableName = reader["TABLE_NAME"].ToString();
                    var tableOwner = reader["OWNER"].ToString();
                    
                    var table = new TableMetadata
                    {
                        TableName = tableName,
                        Owner = tableOwner,
                        RowCount = reader["NUM_ROWS"] != DBNull.Value ? Convert.ToInt64(reader["NUM_ROWS"]) : null,
                        CreatedDate = reader["CREATED_DATE"] != DBNull.Value ? DateTime.Parse(reader["CREATED_DATE"].ToString()) : null,
                        LastModifiedDate = reader["LAST_DDL_TIME"] != DBNull.Value ? DateTime.Parse(reader["LAST_DDL_TIME"].ToString()) : null
                    };
                    
                    tables.Add(table);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle tables");
                throw;
            }

            // For each table, get its columns
            foreach (var table in tables)
            {
                table.Columns = await GetTableColumnsAsync(table.Owner, table.TableName);
                table.PrimaryKeyConstraints = await GetPrimaryKeyConstraintsAsync(table.Owner, table.TableName);
                table.ForeignKeyConstraints = await GetForeignKeyConstraintsAsync(table.Owner, table.TableName);
                table.UniqueConstraints = await GetUniqueConstraintsAsync(table.Owner, table.TableName);
                table.CheckConstraints = await GetCheckConstraintsAsync(table.Owner, table.TableName);
                table.Indexes = await GetIndexesAsync(table.Owner, table.TableName);
            }
            
            AddToCache(cacheKey, tables, _defaultCacheDuration);
            return tables;
        }

        /// <inheritdoc/>
        public async Task<TableMetadata> GetTableMetadataAsync(string schema, string tableName)
        {
            string cacheKey = $"table_{schema}_{tableName}";
            
            if (TryGetFromCache<TableMetadata>(cacheKey, out var cachedTable))
            {
                return cachedTable;
            }
            
            string sql = @"
                SELECT 
                    t.TABLE_NAME,
                    t.OWNER,
                    t.NUM_ROWS,
                    t.LAST_ANALYZED,
                    TO_CHAR(o.CREATED, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE,
                    TO_CHAR(o.LAST_DDL_TIME, 'YYYY-MM-DD HH24:MI:SS') AS LAST_DDL_TIME
                FROM 
                    ALL_TABLES t
                JOIN
                    ALL_OBJECTS o ON t.TABLE_NAME = o.OBJECT_NAME AND t.OWNER = o.OWNER AND o.OBJECT_TYPE = 'TABLE'
                WHERE 
                    t.OWNER = :schema AND t.TABLE_NAME = :tableName";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("tableName", OracleDbType.Varchar2) { Value = tableName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return null;
                }
                
                var table = new TableMetadata
                {
                    TableName = reader["TABLE_NAME"].ToString(),
                    Owner = reader["OWNER"].ToString(),
                    RowCount = reader["NUM_ROWS"] != DBNull.Value ? Convert.ToInt64(reader["NUM_ROWS"]) : null,
                    CreatedDate = reader["CREATED_DATE"] != DBNull.Value ? DateTime.Parse(reader["CREATED_DATE"].ToString()) : null,
                    LastModifiedDate = reader["LAST_DDL_TIME"] != DBNull.Value ? DateTime.Parse(reader["LAST_DDL_TIME"].ToString()) : null,
                    Columns = await GetTableColumnsAsync(schema, tableName),
                    PrimaryKeyConstraints = await GetPrimaryKeyConstraintsAsync(schema, tableName),
                    ForeignKeyConstraints = await GetForeignKeyConstraintsAsync(schema, tableName),
                    UniqueConstraints = await GetUniqueConstraintsAsync(schema, tableName),
                    CheckConstraints = await GetCheckConstraintsAsync(schema, tableName),
                    Indexes = await GetIndexesAsync(schema, tableName)
                };
                
                AddToCache(cacheKey, table, _defaultCacheDuration);
                return table;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle table metadata for {Schema}.{TableName}", schema, tableName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ViewMetadata>> GetAllViewsAsync()
        {
            const string cacheKey = "all_views";
            
            if (TryGetFromCache<List<ViewMetadata>>(cacheKey, out var cachedViews))
            {
                return cachedViews;
            }
            
            var views = new List<ViewMetadata>();
            
            string schema = _connectionFactory.GetDefaultSchema().ToUpper();
            string sql = @"
                SELECT 
                    v.VIEW_NAME,
                    v.OWNER,
                    TO_CHAR(o.CREATED, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE,
                    TO_CHAR(o.LAST_DDL_TIME, 'YYYY-MM-DD HH24:MI:SS') AS LAST_DDL_TIME,
                    v.TEXT AS DEFINITION
                FROM 
                    ALL_VIEWS v
                JOIN
                    ALL_OBJECTS o ON v.VIEW_NAME = o.OBJECT_NAME AND v.OWNER = o.OWNER AND o.OBJECT_TYPE = 'VIEW'
                WHERE 
                    v.OWNER = :schema
                ORDER BY 
                    v.VIEW_NAME";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var viewName = reader["VIEW_NAME"].ToString();
                    var viewOwner = reader["OWNER"].ToString();
                    var createdDateStr = reader["CREATED_DATE"].ToString();
                    var lastDdlTimeStr = reader["LAST_DDL_TIME"].ToString();
                    
                    var view = new ViewMetadata
                    {
                        ViewName = viewName,
                        Owner = viewOwner,
                        CreatedDate = !string.IsNullOrEmpty(createdDateStr) ? DateTime.Parse(createdDateStr) : null,
                        LastModifiedDate = !string.IsNullOrEmpty(lastDdlTimeStr) ? DateTime.Parse(lastDdlTimeStr) : null,
                        Definition = reader["DEFINITION"].ToString(),
                        IsUpdatable = false // Oracle views are generally not updatable by default
                    };
                    
                    // Get columns for this view
                    view.Columns = await GetViewColumnsAsync(viewOwner, viewName);
                    
                    views.Add(view);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle views");
                throw;
            }
            
            AddToCache(cacheKey, views, _defaultCacheDuration);
            return views;
        }

        /// <inheritdoc/>
        public async Task<ViewMetadata> GetViewMetadataAsync(string schema, string viewName)
        {
            string cacheKey = $"view_{schema}_{viewName}";
            
            if (TryGetFromCache<ViewMetadata>(cacheKey, out var cachedView))
            {
                return cachedView;
            }
            
            string sql = @"
                SELECT 
                    v.VIEW_NAME,
                    v.OWNER,
                    TO_CHAR(o.CREATED, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE,
                    TO_CHAR(o.LAST_DDL_TIME, 'YYYY-MM-DD HH24:MI:SS') AS LAST_DDL_TIME,
                    v.TEXT AS DEFINITION
                FROM 
                    ALL_VIEWS v
                JOIN
                    ALL_OBJECTS o ON v.VIEW_NAME = o.OBJECT_NAME AND v.OWNER = o.OWNER AND o.OBJECT_TYPE = 'VIEW'
                WHERE 
                    v.OWNER = :schema AND v.VIEW_NAME = :viewName";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("viewName", OracleDbType.Varchar2) { Value = viewName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return null;
                }
                
                var view = new ViewMetadata
                {
                    ViewName = reader["VIEW_NAME"].ToString(),
                    Owner = reader["OWNER"].ToString(),
                    CreatedDate = reader["CREATED_DATE"] != DBNull.Value ? DateTime.Parse(reader["CREATED_DATE"].ToString()) : null,
                    LastModifiedDate = reader["LAST_DDL_TIME"] != DBNull.Value ? DateTime.Parse(reader["LAST_DDL_TIME"].ToString()) : null,
                    Definition = reader["DEFINITION"].ToString(),
                    IsUpdatable = false, // Oracle views are generally not updatable by default
                    Columns = await GetViewColumnsAsync(schema, viewName)
                };
                
                AddToCache(cacheKey, view, _defaultCacheDuration);
                return view;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle view metadata for {Schema}.{ViewName}", schema, viewName);
                throw;
            }
        }

        /// <inheritdoc/>
        public void ClearCache()
        {
            _metadataCache.Clear();
            _cacheExpiration.Clear();
            _logger.LogInformation("Oracle metadata cache cleared");
        }

        #region Private Helper Methods

        private async Task<List<ColumnMetadata>> GetTableColumnsAsync(string schema, string tableName)
        {
            var columns = new List<ColumnMetadata>();
            
            string sql = @"
                SELECT 
                    c.COLUMN_NAME,
                    c.DATA_TYPE,
                    c.CHAR_LENGTH AS MAX_LENGTH,
                    c.DATA_PRECISION AS PRECISION,
                    c.DATA_SCALE AS SCALE,
                    DECODE(c.NULLABLE, 'Y', 1, 0) AS IS_NULLABLE,
                    c.COLUMN_ID AS POSITION,
                    c.DATA_DEFAULT AS DEFAULT_VALUE,
                    DECODE(
                        (SELECT COUNT(*) FROM ALL_CONS_COLUMNS cc
                         JOIN ALL_CONSTRAINTS co ON cc.CONSTRAINT_NAME = co.CONSTRAINT_NAME AND cc.OWNER = co.OWNER
                         WHERE cc.OWNER = c.OWNER AND cc.TABLE_NAME = c.TABLE_NAME 
                         AND cc.COLUMN_NAME = c.COLUMN_NAME AND co.CONSTRAINT_TYPE = 'P'),
                        0, 0, 1
                    ) AS IS_PRIMARY_KEY,
                    CASE WHEN c.IDENTITY_COLUMN = 'YES' THEN 1 ELSE 0 END AS IS_IDENTITY,
                    0 AS IS_COMPUTED,
                    NULL AS COMPUTED_EXPRESSION
                FROM 
                    ALL_TAB_COLUMNS c
                WHERE 
                    c.OWNER = :schema AND c.TABLE_NAME = :tableName
                ORDER BY 
                    c.COLUMN_ID";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("tableName", OracleDbType.Varchar2) { Value = tableName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(new ColumnMetadata
                    {
                        ColumnName = reader["COLUMN_NAME"].ToString(),
                        DataType = reader["DATA_TYPE"].ToString(),
                        MaxLength = reader["MAX_LENGTH"] != DBNull.Value ? Convert.ToInt32(reader["MAX_LENGTH"]) : null,
                        Precision = reader["PRECISION"] != DBNull.Value ? Convert.ToInt32(reader["PRECISION"]) : null,
                        Scale = reader["SCALE"] != DBNull.Value ? Convert.ToInt32(reader["SCALE"]) : null,
                        IsNullable = Convert.ToBoolean(reader["IS_NULLABLE"]),
                        Position = Convert.ToInt32(reader["POSITION"]),
                        DefaultValue = reader["DEFAULT_VALUE"] != DBNull.Value ? reader["DEFAULT_VALUE"].ToString() : null,
                        IsPrimaryKey = Convert.ToBoolean(reader["IS_PRIMARY_KEY"]),
                        IsIdentity = Convert.ToBoolean(reader["IS_IDENTITY"]),
                        IsComputed = Convert.ToBoolean(reader["IS_COMPUTED"]),
                        ComputedExpression = reader["COMPUTED_EXPRESSION"] != DBNull.Value ? reader["COMPUTED_EXPRESSION"].ToString() : null
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle column metadata for {Schema}.{TableName}", schema, tableName);
                throw;
            }
            
            return columns;
        }

        private async Task<List<ColumnMetadata>> GetViewColumnsAsync(string schema, string viewName)
        {
            var columns = new List<ColumnMetadata>();
            
            string sql = @"
                SELECT 
                    c.COLUMN_NAME,
                    c.DATA_TYPE,
                    c.CHAR_LENGTH AS MAX_LENGTH,
                    c.DATA_PRECISION AS PRECISION,
                    c.DATA_SCALE AS SCALE,
                    DECODE(c.NULLABLE, 'Y', 1, 0) AS IS_NULLABLE,
                    c.COLUMN_ID AS POSITION
                FROM 
                    ALL_TAB_COLUMNS c
                WHERE 
                    c.OWNER = :schema AND c.TABLE_NAME = :viewName
                ORDER BY 
                    c.COLUMN_ID";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("viewName", OracleDbType.Varchar2) { Value = viewName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(new ColumnMetadata
                    {
                        ColumnName = reader["COLUMN_NAME"].ToString(),
                        DataType = reader["DATA_TYPE"].ToString(),
                        MaxLength = reader["MAX_LENGTH"] != DBNull.Value ? Convert.ToInt32(reader["MAX_LENGTH"]) : null,
                        Precision = reader["PRECISION"] != DBNull.Value ? Convert.ToInt32(reader["PRECISION"]) : null,
                        Scale = reader["SCALE"] != DBNull.Value ? Convert.ToInt32(reader["SCALE"]) : null,
                        IsNullable = Convert.ToBoolean(reader["IS_NULLABLE"]),
                        Position = Convert.ToInt32(reader["POSITION"]),
                        IsPrimaryKey = false
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle view column metadata for {Schema}.{ViewName}", schema, viewName);
                throw;
            }
            
            return columns;
        }

        private async Task<List<ConstraintMetadata>> GetPrimaryKeyConstraintsAsync(string schema, string tableName)
        {
            var constraints = new List<ConstraintMetadata>();
            
            string sql = @"
                SELECT 
                    c.CONSTRAINT_NAME,
                    c.CONSTRAINT_TYPE,
                    c.TABLE_NAME,
                    c.OWNER,
                    c.STATUS,
                    TO_CHAR(c.CREATED, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE
                FROM 
                    ALL_CONSTRAINTS c
                WHERE 
                    c.OWNER = :schema 
                    AND c.TABLE_NAME = :tableName
                    AND c.CONSTRAINT_TYPE = 'P'
                ORDER BY 
                    c.CONSTRAINT_NAME";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("tableName", OracleDbType.Varchar2) { Value = tableName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var constraintName = reader["CONSTRAINT_NAME"].ToString();
                    var constraint = new ConstraintMetadata
                    {
                        ConstraintName = constraintName,
                        ConstraintType = "PRIMARY KEY",
                        TableName = reader["TABLE_NAME"].ToString(),
                        Owner = reader["OWNER"].ToString(),
                        IsEnabled = reader["STATUS"].ToString() == "ENABLED",
                        CreatedDate = reader["CREATED_DATE"] != DBNull.Value ? DateTime.Parse(reader["CREATED_DATE"].ToString()) : null,
                        Columns = await GetConstraintColumnsAsync(schema, constraintName)
                    };
                    
                    constraints.Add(constraint);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle primary key constraints for {Schema}.{TableName}", schema, tableName);
                throw;
            }
            
            return constraints;
        }

        private async Task<List<ConstraintMetadata>> GetForeignKeyConstraintsAsync(string schema, string tableName)
        {
            var constraints = new List<ConstraintMetadata>();
            
            string sql = @"
                SELECT 
                    c.CONSTRAINT_NAME,
                    c.CONSTRAINT_TYPE,
                    c.TABLE_NAME,
                    c.OWNER,
                    c.R_OWNER AS REFERENCED_OWNER,
                    c.R_CONSTRAINT_NAME,
                    c.DELETE_RULE,
                    c.STATUS,
                    TO_CHAR(c.CREATED, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE,
                    (SELECT rc.TABLE_NAME FROM ALL_CONSTRAINTS rc WHERE rc.OWNER = c.R_OWNER AND rc.CONSTRAINT_NAME = c.R_CONSTRAINT_NAME) AS REFERENCED_TABLE
                FROM 
                    ALL_CONSTRAINTS c
                WHERE 
                    c.OWNER = :schema 
                    AND c.TABLE_NAME = :tableName
                    AND c.CONSTRAINT_TYPE = 'R'
                ORDER BY 
                    c.CONSTRAINT_NAME";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("tableName", OracleDbType.Varchar2) { Value = tableName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var constraintName = reader["CONSTRAINT_NAME"].ToString();
                    var refConstraintName = reader["R_CONSTRAINT_NAME"].ToString();
                    var refOwner = reader["REFERENCED_OWNER"].ToString();
                    
                    var constraint = new ConstraintMetadata
                    {
                        ConstraintName = constraintName,
                        ConstraintType = "FOREIGN KEY",
                        TableName = reader["TABLE_NAME"].ToString(),
                        Owner = reader["OWNER"].ToString(),
                        ReferencedTableName = reader["REFERENCED_TABLE"].ToString(),
                        ReferencedOwner = refOwner,
                        DeleteRule = reader["DELETE_RULE"].ToString(),
                        IsEnabled = reader["STATUS"].ToString() == "ENABLED",
                        CreatedDate = reader["CREATED_DATE"] != DBNull.Value ? DateTime.Parse(reader["CREATED_DATE"].ToString()) : null,
                        Columns = await GetConstraintColumnsAsync(schema, constraintName),
                        ReferencedColumns = await GetConstraintColumnsAsync(refOwner, refConstraintName)
                    };
                    
                    constraints.Add(constraint);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle foreign key constraints for {Schema}.{TableName}", schema, tableName);
                throw;
            }
            
            return constraints;
        }

        private async Task<List<ConstraintMetadata>> GetUniqueConstraintsAsync(string schema, string tableName)
        {
            var constraints = new List<ConstraintMetadata>();
            
            string sql = @"
                SELECT 
                    c.CONSTRAINT_NAME,
                    c.CONSTRAINT_TYPE,
                    c.TABLE_NAME,
                    c.OWNER,
                    c.STATUS,
                    TO_CHAR(c.CREATED, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE
                FROM 
                    ALL_CONSTRAINTS c
                WHERE 
                    c.OWNER = :schema 
                    AND c.TABLE_NAME = :tableName
                    AND c.CONSTRAINT_TYPE = 'U'
                ORDER BY 
                    c.CONSTRAINT_NAME";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("tableName", OracleDbType.Varchar2) { Value = tableName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var constraintName = reader["CONSTRAINT_NAME"].ToString();
                    var constraint = new ConstraintMetadata
                    {
                        ConstraintName = constraintName,
                        ConstraintType = "UNIQUE",
                        TableName = reader["TABLE_NAME"].ToString(),
                        Owner = reader["OWNER"].ToString(),
                        IsEnabled = reader["STATUS"].ToString() == "ENABLED",
                        CreatedDate = reader["CREATED_DATE"] != DBNull.Value ? DateTime.Parse(reader["CREATED_DATE"].ToString()) : null,
                        Columns = await GetConstraintColumnsAsync(schema, constraintName)
                    };
                    
                    constraints.Add(constraint);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle unique constraints for {Schema}.{TableName}", schema, tableName);
                throw;
            }
            
            return constraints;
        }

        private async Task<List<ConstraintMetadata>> GetCheckConstraintsAsync(string schema, string tableName)
        {
            var constraints = new List<ConstraintMetadata>();
            
            string sql = @"
                SELECT 
                    c.CONSTRAINT_NAME,
                    c.CONSTRAINT_TYPE,
                    c.TABLE_NAME,
                    c.OWNER,
                    c.SEARCH_CONDITION AS DEFINITION,
                    c.STATUS,
                    TO_CHAR(c.CREATED, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE
                FROM 
                    ALL_CONSTRAINTS c
                WHERE 
                    c.OWNER = :schema 
                    AND c.TABLE_NAME = :tableName
                    AND c.CONSTRAINT_TYPE = 'C'
                    AND c.CONSTRAINT_NAME NOT LIKE 'SYS_%'
                ORDER BY 
                    c.CONSTRAINT_NAME";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("tableName", OracleDbType.Varchar2) { Value = tableName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var constraintName = reader["CONSTRAINT_NAME"].ToString();
                    var constraint = new ConstraintMetadata
                    {
                        ConstraintName = constraintName,
                        ConstraintType = "CHECK",
                        TableName = reader["TABLE_NAME"].ToString(),
                        Owner = reader["OWNER"].ToString(),
                        Definition = reader["DEFINITION"].ToString(),
                        IsEnabled = reader["STATUS"].ToString() == "ENABLED",
                        CreatedDate = reader["CREATED_DATE"] != DBNull.Value ? DateTime.Parse(reader["CREATED_DATE"].ToString()) : null
                    };
                    
                    constraints.Add(constraint);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle check constraints for {Schema}.{TableName}", schema, tableName);
                throw;
            }
            
            return constraints;
        }

        private async Task<List<string>> GetConstraintColumnsAsync(string schema, string constraintName)
        {
            var columns = new List<string>();
            
            string sql = @"
                SELECT 
                    cc.COLUMN_NAME
                FROM 
                    ALL_CONS_COLUMNS cc
                WHERE 
                    cc.OWNER = :schema 
                    AND cc.CONSTRAINT_NAME = :constraintName
                ORDER BY 
                    cc.POSITION";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("constraintName", OracleDbType.Varchar2) { Value = constraintName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(reader["COLUMN_NAME"].ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle constraint columns for {Schema}.{ConstraintName}", schema, constraintName);
                throw;
            }
            
            return columns;
        }

        private async Task<List<IndexMetadata>> GetIndexesAsync(string schema, string tableName)
        {
            var indexes = new List<IndexMetadata>();
            
            string sql = @"
                SELECT 
                    i.INDEX_NAME,
                    i.TABLE_NAME,
                    i.OWNER,
                    i.INDEX_TYPE,
                    i.UNIQUENESS,
                    TO_CHAR(o.CREATED, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE,
                    TO_CHAR(o.LAST_DDL_TIME, 'YYYY-MM-DD HH24:MI:SS') AS LAST_DDL_TIME,
                    i.STATUS
                FROM 
                    ALL_INDEXES i
                JOIN
                    ALL_OBJECTS o ON i.INDEX_NAME = o.OBJECT_NAME AND i.OWNER = o.OWNER AND o.OBJECT_TYPE = 'INDEX'
                WHERE 
                    i.OWNER = :schema AND i.TABLE_NAME = :tableName
                ORDER BY 
                    i.INDEX_NAME";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("tableName", OracleDbType.Varchar2) { Value = tableName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var indexName = reader["INDEX_NAME"].ToString();
                    var isPrimary = await IsPrimaryKeyIndexAsync(schema, indexName);
                    
                    var index = new IndexMetadata
                    {
                        IndexName = indexName,
                        TableName = reader["TABLE_NAME"].ToString(),
                        Owner = reader["OWNER"].ToString(),
                        IndexType = reader["INDEX_TYPE"].ToString(),
                        IsUnique = reader["UNIQUENESS"].ToString() == "UNIQUE",
                        IsPrimaryKey = isPrimary,
                        CreatedDate = reader["CREATED_DATE"] != DBNull.Value ? DateTime.Parse(reader["CREATED_DATE"].ToString()) : null,
                        LastModifiedDate = reader["LAST_DDL_TIME"] != DBNull.Value ? DateTime.Parse(reader["LAST_DDL_TIME"].ToString()) : null,
                        IsValid = reader["STATUS"].ToString() == "VALID",
                        Columns = await GetIndexColumnsAsync(schema, indexName)
                    };
                    
                    indexes.Add(index);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle indexes for {Schema}.{TableName}", schema, tableName);
                throw;
            }
            
            return indexes;
        }

        private async Task<bool> IsPrimaryKeyIndexAsync(string schema, string indexName)
        {
            string sql = @"
                SELECT 
                    COUNT(*) AS IS_PK
                FROM 
                    ALL_CONSTRAINTS c
                WHERE 
                    c.OWNER = :schema 
                    AND c.INDEX_NAME = :indexName
                    AND c.CONSTRAINT_TYPE = 'P'";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("indexName", OracleDbType.Varchar2) { Value = indexName.ToUpper() });
                
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining if index is primary key: {Schema}.{IndexName}", schema, indexName);
                return false;
            }
        }

        private async Task<List<IndexColumnMetadata>> GetIndexColumnsAsync(string schema, string indexName)
        {
            var columns = new List<IndexColumnMetadata>();
            
            string sql = @"
                SELECT 
                    ic.COLUMN_NAME,
                    ic.COLUMN_POSITION AS POSITION,
                    CASE WHEN ic.DESCEND = 'DESC' THEN 1 ELSE 0 END AS IS_DESCENDING
                FROM 
                    ALL_IND_COLUMNS ic
                WHERE 
                    ic.INDEX_OWNER = :schema 
                    AND ic.INDEX_NAME = :indexName
                ORDER BY 
                    ic.COLUMN_POSITION";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("indexName", OracleDbType.Varchar2) { Value = indexName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(new IndexColumnMetadata
                    {
                        ColumnName = reader["COLUMN_NAME"].ToString(),
                        Position = Convert.ToInt32(reader["POSITION"]),
                        IsDescending = Convert.ToBoolean(reader["IS_DESCENDING"])
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle index columns for {Schema}.{IndexName}", schema, indexName);
                throw;
            }
            
            return columns;
        }

        #endregion

        #region Cache Methods

        private void AddToCache<T>(string key, T value, TimeSpan duration)
        {
            _metadataCache[key] = value;
            _cacheExpiration[key] = DateTime.UtcNow.Add(duration);
        }

        private bool TryGetFromCache<T>(string key, out T value)
        {
            value = default;
            
            if (!_metadataCache.ContainsKey(key) || !_cacheExpiration.ContainsKey(key))
            {
                return false;
            }
            
            if (DateTime.UtcNow > _cacheExpiration[key])
            {
                _metadataCache.Remove(key);
                _cacheExpiration.Remove(key);
                return false;
            }
            
            if (_metadataCache[key] is T cachedValue)
            {
                value = cachedValue;
                return true;
            }
            
            return false;
        }

        #endregion
    }
}
