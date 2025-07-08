using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Interfaces;

namespace OracleAgent.Core.Services
{
    public  class RawSqlService : IRawSqlService
    {
        private readonly string _connectionString;

        public RawSqlService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<string> ExecuteRawSelectAsync(string rawSelectSql)
        {

            if (!IsSelectOnly(rawSelectSql))
            {
                return "Inavlid select statement. Only select statements are allowed.";
            }

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
                        return System.Text.Json.JsonSerializer.Serialize(results);
                    }
                }
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
