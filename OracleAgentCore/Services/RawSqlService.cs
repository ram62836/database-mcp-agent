using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Interfaces;

namespace OracleAgent.Core.Services
{
    public class RawSqlService : IRawSqlService
    {
        private readonly string _connectionString;
        private readonly ILogger<RawSqlService> _logger;

        public RawSqlService(IConfiguration config, ILogger<RawSqlService> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<string> ExecuteRawSelectAsync(string rawSelectSql)
        {
            _logger.LogInformation("Executing raw select SQL.");
            if (!IsSelectOnly(rawSelectSql))
            {
                _logger.LogWarning("Invalid select statement detected.");
                return "Inavlid select statement. Only select statements are allowed.";
            }

            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new OracleCommand(rawSelectSql, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var results = new List<Dictionary<string, object>>();
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var value = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                                    row[reader.GetName(i)] = value;
                                }
                                results.Add(row);
                            }
                            _logger.LogInformation("Raw select SQL executed successfully. Rows returned: {Count}", results.Count);
                            return System.Text.Json.JsonSerializer.Serialize(results);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing raw select SQL.");
                throw;
            }
        }

        static bool IsSelectOnly(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return false;
            sql = sql.Trim().ToUpperInvariant();

            if (!sql.StartsWith("SELECT")) return false;

            string[] disallowed = { "INSERT", "UPDATE", "DELETE", "MERGE", "DROP", "ALTER", "TRUNCATE", "EXECUTE" };
            foreach (var bad in disallowed)
                if (sql.Contains(bad)) return false;

            return true;
        }
    }
}
