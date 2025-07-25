using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using DatabaseMcp.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DatabaseMcp.Core.Tests
{
    [Collection("Database Tests")]
    public class PerformanceAnalyticsServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock = new();
        private readonly Mock<IDbConnection> _connectionMock = new();
        private readonly Mock<IDbCommand> _commandMock = new();
        private readonly Mock<IDataReader> _readerMock = new();
        private readonly Mock<IDbDataParameter> _parameterMock = new();
        private readonly Mock<IDataParameterCollection> _parametersCollectionMock = new();
        private readonly Mock<ILogger<PerformanceAnalyticsService>> _loggerMock = TestHelper.CreateLoggerMock<PerformanceAnalyticsService>();
        private readonly PerformanceAnalyticsService _service;

        public PerformanceAnalyticsServiceTests()
        {
            SetupBasicMocks();
            _service = new PerformanceAnalyticsService(_connectionFactoryMock.Object, _loggerMock.Object);
        }

        private void SetupBasicMocks()
        {
            // Setup parameter mock
            _ = _parameterMock.SetupAllProperties();

            // Setup parameters collection mock
            _ = _parametersCollectionMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

            // Setup command mock
            _ = _commandMock.Setup(c => c.CreateParameter()).Returns(_parameterMock.Object);
            _ = _commandMock.SetupGet(c => c.Parameters).Returns(_parametersCollectionMock.Object);
            _ = _commandMock.SetupProperty(c => c.CommandText);
            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);

            // Setup connection mock
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);

            // Setup connection factory mock
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new PerformanceAnalyticsService(null!, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }

        [Fact]
        public async Task GetTopSqlByPerformanceAsync_WithValidRequest_ReturnsSqlMetrics()
        {
            // Arrange
            var request = new PerformanceAnalysisRequest
            {
                TopN = 5,
                OrderBy = "ELAPSED_TIME",
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now
            };

            var sqlData = new List<SqlPerformanceMetrics>
            {
                new SqlPerformanceMetrics
                {
                    SqlId = "TEST_SQL_ID_1",
                    SqlText = "SELECT * FROM EMPLOYEES",
                    Executions = 100,
                    ElapsedTimeSeconds = 150.5,
                    CpuTimeSeconds = 45.2,
                    DiskReads = 1000,
                    BufferGets = 5000,
                    AvgElapsedTimeSeconds = 1.505,
                    AvgCpuTimeSeconds = 0.452,
                    RowsProcessed = 500,
                    FirstLoadTime = DateTime.Now.AddHours(-2),
                    LastLoadTime = DateTime.Now.AddMinutes(-30),
                    PlanHash = "123456789",
                    Module = "TESTAPP",
                    ParsingSchemaName = "HR"
                },
                new SqlPerformanceMetrics
                {
                    SqlId = "TEST_SQL_ID_2",
                    SqlText = "UPDATE EMPLOYEES SET SALARY = :1",
                    Executions = 50,
                    ElapsedTimeSeconds = 75.3,
                    CpuTimeSeconds = 25.1,
                    DiskReads = 500,
                    BufferGets = 2500,
                    AvgElapsedTimeSeconds = 1.506,
                    AvgCpuTimeSeconds = 0.502,
                    RowsProcessed = 250,
                    FirstLoadTime = DateTime.Now.AddHours(-1),
                    LastLoadTime = DateTime.Now.AddMinutes(-15),
                    PlanHash = "987654321",
                    Module = "TESTAPP",
                    ParsingSchemaName = "HR"
                }
            };

            SetupReaderForSqlPerformanceMetrics(sqlData);

            // Act
            var result = await _service.GetTopSqlByPerformanceAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            
            var firstSql = result[0];
            Assert.Equal("TEST_SQL_ID_1", firstSql.SqlId);
            Assert.Equal("SELECT * FROM EMPLOYEES", firstSql.SqlText);
            Assert.Equal(100, firstSql.Executions);
            Assert.Equal(150.5, firstSql.ElapsedTimeSeconds);
            Assert.Equal(45.2, firstSql.CpuTimeSeconds);
        }

        [Fact]
        public async Task GetTopSqlByPerformanceAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.GetTopSqlByPerformanceAsync(null!));
            Assert.Equal("request", exception.ParamName);
        }

        [Fact]
        public async Task GetTopSqlByExecutionsAsync_WithValidParameters_ReturnsSqlMetrics()
        {
            // Arrange
            var sqlData = new List<SqlPerformanceMetrics>
            {
                new SqlPerformanceMetrics
                {
                    SqlId = "HIGH_EXEC_SQL",
                    SqlText = "SELECT COUNT(*) FROM ORDERS",
                    Executions = 1000,
                    ElapsedTimeSeconds = 50.0,
                    CpuTimeSeconds = 20.0
                }
            };

            SetupReaderForSqlPerformanceMetrics(sqlData);

            // Act
            var result = await _service.GetTopSqlByExecutionsAsync(10, DateTime.Now.AddHours(-1), DateTime.Now);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("HIGH_EXEC_SQL", result[0].SqlId);
            Assert.Equal(1000, result[0].Executions);
        }

        [Fact]
        public async Task GetTopSqlByCpuTimeAsync_WithValidParameters_ReturnsSqlMetrics()
        {
            // Arrange
            var sqlData = new List<SqlPerformanceMetrics>
            {
                new SqlPerformanceMetrics
                {
                    SqlId = "HIGH_CPU_SQL",
                    SqlText = "SELECT * FROM LARGE_TABLE ORDER BY COMPLEX_CALCULATION",
                    Executions = 10,
                    ElapsedTimeSeconds = 200.0,
                    CpuTimeSeconds = 180.0
                }
            };

            SetupReaderForSqlPerformanceMetrics(sqlData);

            // Act
            var result = await _service.GetTopSqlByCpuTimeAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("HIGH_CPU_SQL", result[0].SqlId);
            Assert.Equal(180.0, result[0].CpuTimeSeconds);
        }

        [Fact]
        public async Task GetTopSqlByElapsedTimeAsync_WithValidParameters_ReturnsSqlMetrics()
        {
            // Arrange
            var sqlData = new List<SqlPerformanceMetrics>
            {
                new SqlPerformanceMetrics
                {
                    SqlId = "SLOW_SQL",
                    SqlText = "SELECT * FROM HUGE_TABLE WHERE UNINDEXED_COLUMN = 'VALUE'",
                    Executions = 5,
                    ElapsedTimeSeconds = 500.0,
                    CpuTimeSeconds = 50.0
                }
            };

            SetupReaderForSqlPerformanceMetrics(sqlData);

            // Act
            var result = await _service.GetTopSqlByElapsedTimeAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("SLOW_SQL", result[0].SqlId);
            Assert.Equal(500.0, result[0].ElapsedTimeSeconds);
        }

        [Fact]
        public async Task GetWaitEventAnalysisAsync_WithValidParameters_ReturnsWaitEventMetrics()
        {
            // Arrange
            var waitEventData = new List<WaitEventMetrics>
            {
                new WaitEventMetrics
                {
                    EventName = "db file sequential read",
                    WaitClass = "User I/O",
                    TotalWaits = 50000,
                    TotalWaitTimeSeconds = 125.5,
                    AvgWaitTimeMs = 2.51,
                    TimeWaitedPercent = 45.2,
                    Description = "Single block I/O operations",
                    SampleTime = DateTime.Now,
                    ObjectName = "EMPLOYEES",
                    ObjectOwner = "HR"
                },
                new WaitEventMetrics
                {
                    EventName = "CPU time",
                    WaitClass = "CPU",
                    TotalWaits = 0,
                    TotalWaitTimeSeconds = 200.3,
                    AvgWaitTimeMs = 0,
                    TimeWaitedPercent = 54.8,
                    Description = "CPU processing time",
                    SampleTime = DateTime.Now,
                    ObjectName = "",
                    ObjectOwner = ""
                }
            };

            SetupReaderForWaitEventMetrics(waitEventData);

            // Act
            var result = await _service.GetWaitEventAnalysisAsync("EMPLOYEES");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            
            var ioWait = result.First(w => w.EventName == "db file sequential read");
            Assert.Equal("User I/O", ioWait.WaitClass);
            Assert.Equal(50000, ioWait.TotalWaits);
            Assert.Equal(125.5, ioWait.TotalWaitTimeSeconds);
        }

        [Fact]
        public async Task GetTableUsageStatisticsAsync_WithValidTableNames_ReturnsTableUsageStats()
        {
            // Arrange
            var tableNames = new List<string> { "EMPLOYEES", "DEPARTMENTS" };
            var tableUsageData = new List<TableUsageStatistics>
            {
                new TableUsageStatistics
                {
                    TableName = "EMPLOYEES",
                    SchemaName = "HR",
                    TableScans = 150,
                    RowLookups = 5000,
                    RowsInserted = 100,
                    RowsUpdated = 200,
                    RowsDeleted = 10,
                    LastUsed = DateTime.Now.AddHours(-2),
                    AvgRowLength = 120.5,
                    TableSize = 1500000,
                    NumRows = 12500,
                    Tablespace = "USERS",
                    IsPartitioned = false
                },
                new TableUsageStatistics
                {
                    TableName = "DEPARTMENTS",
                    SchemaName = "HR",
                    TableScans = 50,
                    RowLookups = 1000,
                    RowsInserted = 10,
                    RowsUpdated = 20,
                    RowsDeleted = 2,
                    LastUsed = DateTime.Now.AddHours(-1),
                    AvgRowLength = 80.3,
                    TableSize = 25000,
                    NumRows = 311,
                    Tablespace = "USERS",
                    IsPartitioned = false
                }
            };

            SetupReaderForTableUsageStatistics(tableUsageData);

            // Act
            var result = await _service.GetTableUsageStatisticsAsync(tableNames);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            
            var employeesTable = result.First(t => t.TableName == "EMPLOYEES");
            Assert.Equal("HR", employeesTable.SchemaName);
            Assert.Equal(150, employeesTable.TableScans);
            Assert.Equal(5000, employeesTable.RowLookups);
        }

        [Fact]
        public async Task GetTableUsageStatisticsAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Act
            var result = await _service.GetTableUsageStatisticsAsync(new List<string>());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTableUsageStatisticsAsync_WithNullList_ReturnsEmptyList()
        {
            // Act
            var result = await _service.GetTableUsageStatisticsAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }



        [Fact]
        public async Task GetIndexUsageStatisticsAsync_WithValidTableName_ReturnsIndexStats()
        {
            // Arrange
            const string tableName = "EMPLOYEES";
            var indexUsageData = new List<IndexUsageStatistics>
            {
                new IndexUsageStatistics
                {
                    IndexName = "PK_EMPLOYEES",
                    TableName = "EMPLOYEES",
                    SchemaName = "HR",
                    IndexType = "NORMAL",
                    IsUnique = true,
                    TotalAccess = 10000,
                    LastUsed = DateTime.Now.AddHours(-1),
                    BlevelDepth = 1,
                    LeafBlocks = 25,
                    DistinctKeys = 1000,
                    ClusteringFactor = 850.5,
                    Status = "VALID",
                    ColumnNames = new List<string> { "EMPLOYEE_ID" },
                    IsUnused = false,
                    MonitoringStarted = DateTime.Now.AddDays(-30)
                },
                new IndexUsageStatistics
                {
                    IndexName = "IDX_EMP_EMAIL",
                    TableName = "EMPLOYEES",
                    SchemaName = "HR",
                    IndexType = "NORMAL",
                    IsUnique = false,
                    TotalAccess = 500,
                    LastUsed = DateTime.Now.AddHours(-5),
                    BlevelDepth = 1,
                    LeafBlocks = 10,
                    DistinctKeys = 1000,
                    ClusteringFactor = 900.2,
                    Status = "VALID",
                    ColumnNames = new List<string> { "EMAIL" },
                    IsUnused = false,
                    MonitoringStarted = DateTime.Now.AddDays(-30)
                }
            };

            SetupReaderForIndexUsageStatistics(indexUsageData);

            // Act
            var result = await _service.GetIndexUsageStatisticsAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            
            var pkIndex = result.First(i => i.IndexName == "PK_EMPLOYEES");
            Assert.True(pkIndex.IsUnique);
            Assert.Equal(10000, pkIndex.TotalAccess);
            Assert.Contains("EMPLOYEE_ID", pkIndex.ColumnNames);
        }

        [Fact]
        public async Task GetIndexUsageStatisticsAsync_WithEmptyTableName_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetIndexUsageStatisticsAsync(""));
            Assert.Equal("tableName", exception.ParamName);
        }

        [Fact]
        public async Task GetUnusedIndexesAsync_ReturnsUnusedIndexes()
        {
            // Arrange
            var unusedIndexData = new List<IndexUsageStatistics>
            {
                new IndexUsageStatistics
                {
                    IndexName = "IDX_UNUSED_1",
                    TableName = "EMPLOYEES",
                    SchemaName = "HR",
                    IndexType = "NORMAL",
                    IsUnique = false,
                    TotalAccess = 0,
                    IsUnused = true,
                    Status = "VALID",
                    ColumnNames = new List<string> { "MIDDLE_NAME" }
                },
                new IndexUsageStatistics
                {
                    IndexName = "IDX_UNUSED_2",
                    TableName = "DEPARTMENTS",
                    SchemaName = "HR",
                    IndexType = "NORMAL",
                    IsUnique = false,
                    TotalAccess = 0,
                    IsUnused = true,
                    Status = "VALID",
                    ColumnNames = new List<string> { "DESCRIPTION" }
                }
            };

            SetupReaderForIndexUsageStatistics(unusedIndexData);

            // Act
            var result = await _service.GetUnusedIndexesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, index => Assert.True(index.IsUnused));
            Assert.All(result, index => Assert.Equal(0, index.TotalAccess));
        }

        [Fact]
        public async Task GetUnusedIndexesAsync_WithSpecificTables_ReturnsFilteredUnusedIndexes()
        {
            // Arrange
            var tableNames = new List<string> { "EMS_RS_AWARD", "EMPLOYEES" };
            var unusedIndexData = new List<IndexUsageStatistics>
            {
                new IndexUsageStatistics
                {
                    IndexName = "IDX_AWARD_UNUSED",
                    TableName = "EMS_RS_AWARD",
                    SchemaName = "HR",
                    IndexType = "NORMAL",
                    IsUnique = false,
                    TotalAccess = 0,
                    IsUnused = true,
                    Status = "VALID",
                    ColumnNames = new List<string> { "AWARD_DATE" }
                },
                new IndexUsageStatistics
                {
                    IndexName = "IDX_EMP_UNUSED",
                    TableName = "EMPLOYEES",
                    SchemaName = "HR",
                    IndexType = "NORMAL",
                    IsUnique = false,
                    TotalAccess = 0,
                    IsUnused = true,
                    Status = "VALID",
                    ColumnNames = new List<string> { "MIDDLE_NAME" }
                }
            };

            SetupReaderForIndexUsageStatistics(unusedIndexData);

            // Act
            var result = await _service.GetUnusedIndexesAsync(tableNames);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, index => Assert.True(index.IsUnused));
            Assert.All(result, index => Assert.Equal(0, index.TotalAccess));
            Assert.Contains(result, index => index.TableName == "EMS_RS_AWARD");
            Assert.Contains(result, index => index.TableName == "EMPLOYEES");
        }

        [Fact]
        public async Task GetUnusedIndexesAsync_WithEmptyTableList_ReturnsAllUnusedIndexes()
        {
            // Arrange
            var unusedIndexData = new List<IndexUsageStatistics>
            {
                new IndexUsageStatistics
                {
                    IndexName = "IDX_UNUSED_ALL",
                    TableName = "ANY_TABLE",
                    SchemaName = "HR",
                    IndexType = "NORMAL",
                    IsUnique = false,
                    TotalAccess = 0,
                    IsUnused = true,
                    Status = "VALID",
                    ColumnNames = new List<string> { "SOME_COLUMN" }
                }
            };

            SetupReaderForIndexUsageStatistics(unusedIndexData);

            // Act
            var result = await _service.GetUnusedIndexesAsync(new List<string>());

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result[0].IsUnused);
        }





        [Fact]
        public async Task GetTopSqlByPerformanceAsync_WhenDatabaseThrowsException_PropagatesException()
        {
            // Arrange
            var request = new PerformanceAnalysisRequest { TopN = 5 };
            var expectedException = new InvalidOperationException("Database error");
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ThrowsAsync(expectedException);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetTopSqlByPerformanceAsync(request));
            Assert.Equal(expectedException, actualException);
        }

        [Fact]
        public async Task GetWaitEventAnalysisAsync_WhenDatabaseThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Database error");
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ThrowsAsync(expectedException);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetWaitEventAnalysisAsync());
            Assert.Equal(expectedException, actualException);
        }

        // Helper methods for setting up mock data
        private void SetupReaderForSqlPerformanceMetrics(List<SqlPerformanceMetrics> data)
        {
            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);

            if (data.Count > 0)
            {
                _ = _readerMock.Setup(r => r["SQL_ID"]).Returns(() => data[callCount].SqlId);
                _ = _readerMock.Setup(r => r["SQL_TEXT"]).Returns(() => data[callCount].SqlText);
                _ = _readerMock.Setup(r => r["EXECUTIONS"]).Returns(() => data[callCount].Executions);
                _ = _readerMock.Setup(r => r["ELAPSED_TIME_SECONDS"]).Returns(() => data[callCount].ElapsedTimeSeconds);
                _ = _readerMock.Setup(r => r["CPU_TIME_SECONDS"]).Returns(() => data[callCount].CpuTimeSeconds);
                _ = _readerMock.Setup(r => r["DISK_READS"]).Returns(() => data[callCount].DiskReads);
                _ = _readerMock.Setup(r => r["BUFFER_GETS"]).Returns(() => data[callCount].BufferGets);
                _ = _readerMock.Setup(r => r["AVG_ELAPSED_TIME_SECONDS"]).Returns(() => data[callCount].AvgElapsedTimeSeconds);
                _ = _readerMock.Setup(r => r["AVG_CPU_TIME_SECONDS"]).Returns(() => data[callCount].AvgCpuTimeSeconds);
                _ = _readerMock.Setup(r => r["ROWS_PROCESSED"]).Returns(() => data[callCount].RowsProcessed);
                _ = _readerMock.Setup(r => r["FIRST_LOAD_TIME"]).Returns(() => data[callCount].FirstLoadTime.ToString("yyyy-MM-dd HH:mm:ss"));
                _ = _readerMock.Setup(r => r["LAST_LOAD_TIME"]).Returns(() => data[callCount].LastLoadTime.ToString("yyyy-MM-dd HH:mm:ss"));
                _ = _readerMock.Setup(r => r["PLAN_HASH_VALUE"]).Returns(() => data[callCount].PlanHash);
                _ = _readerMock.Setup(r => r["MODULE"]).Returns(() => data[callCount].Module);
                _ = _readerMock.Setup(r => r["PARSING_SCHEMA_NAME"]).Returns(() => data[callCount].ParsingSchemaName);
            }
        }

        private void SetupReaderForWaitEventMetrics(List<WaitEventMetrics> data)
        {
            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);

            if (data.Count > 0)
            {
                _ = _readerMock.Setup(r => r["EVENT_NAME"]).Returns(() => data[callCount].EventName);
                _ = _readerMock.Setup(r => r["WAIT_CLASS"]).Returns(() => data[callCount].WaitClass);
                _ = _readerMock.Setup(r => r["TOTAL_WAITS"]).Returns(() => data[callCount].TotalWaits);
                _ = _readerMock.Setup(r => r["TOTAL_WAIT_TIME_SECONDS"]).Returns(() => data[callCount].TotalWaitTimeSeconds);
                _ = _readerMock.Setup(r => r["AVG_WAIT_TIME_MS"]).Returns(() => data[callCount].AvgWaitTimeMs);
                _ = _readerMock.Setup(r => r["TIME_WAITED_PERCENT"]).Returns(() => data[callCount].TimeWaitedPercent);
                _ = _readerMock.Setup(r => r["DESCRIPTION"]).Returns(() => data[callCount].Description);
                _ = _readerMock.Setup(r => r["SAMPLE_TIME"]).Returns(() => data[callCount].SampleTime);
                _ = _readerMock.Setup(r => r["OBJECT_NAME"]).Returns(() => data[callCount].ObjectName);
                _ = _readerMock.Setup(r => r["OBJECT_OWNER"]).Returns(() => data[callCount].ObjectOwner);
            }
        }

        private void SetupReaderForTableUsageStatistics(List<TableUsageStatistics> data)
        {
            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);

            if (data.Count > 0)
            {
                _ = _readerMock.Setup(r => r["TABLE_NAME"]).Returns(() => data[Math.Min(callCount, data.Count - 1)].TableName);
                _ = _readerMock.Setup(r => r["SCHEMA_NAME"]).Returns(() => data[Math.Min(callCount, data.Count - 1)].SchemaName);
                _ = _readerMock.Setup(r => r["TABLE_SCANS"]).Returns(() => data[Math.Min(callCount, data.Count - 1)].TableScans);
                _ = _readerMock.Setup(r => r["ROW_LOOKUPS"]).Returns(() => data[Math.Min(callCount, data.Count - 1)].RowLookups);
                _ = _readerMock.Setup(r => r["ROWS_INSERTED"]).Returns(() => data[Math.Min(callCount, data.Count - 1)].RowsInserted);
                _ = _readerMock.Setup(r => r["ROWS_UPDATED"]).Returns(() => data[Math.Min(callCount, data.Count - 1)].RowsUpdated);
                _ = _readerMock.Setup(r => r["ROWS_DELETED"]).Returns(() => data[Math.Min(callCount, data.Count - 1)].RowsDeleted);
                _ = _readerMock.Setup(r => r["LAST_USED"]).Returns(() => data[Math.Min(callCount, data.Count - 1)].LastUsed);
                _ = _readerMock.Setup(r => r["AVG_ROW_LENGTH"]).Returns(() => data[Math.Min(callCount, data.Count - 1)].AvgRowLength);
                _ = _readerMock.Setup(r => r["TABLE_SIZE"]).Returns(() => data[Math.Min(callCount, data.Count - 1)].TableSize);
                _ = _readerMock.Setup(r => r["NUM_ROWS"]).Returns(() => data[Math.Min(callCount, data.Count - 1)].NumRows);
                _ = _readerMock.Setup(r => r["TABLESPACE_NAME"]).Returns(() => data[Math.Min(callCount, data.Count - 1)].Tablespace);
                _ = _readerMock.Setup(r => r["IS_PARTITIONED"]).Returns(() => data[Math.Min(callCount, data.Count - 1)].IsPartitioned ? 1 : 0);
            }
        }

        private void SetupReaderForIndexUsageStatistics(List<IndexUsageStatistics> data)
        {
            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);

            if (data.Count > 0)
            {
                _ = _readerMock.Setup(r => r["INDEX_NAME"]).Returns(() => data[callCount].IndexName);
                _ = _readerMock.Setup(r => r["TABLE_NAME"]).Returns(() => data[callCount].TableName);
                _ = _readerMock.Setup(r => r["SCHEMA_NAME"]).Returns(() => data[callCount].SchemaName);
                _ = _readerMock.Setup(r => r["INDEX_TYPE"]).Returns(() => data[callCount].IndexType);
                _ = _readerMock.Setup(r => r["UNIQUENESS"]).Returns(() => data[callCount].IsUnique ? "UNIQUE" : "NONUNIQUE");
                _ = _readerMock.Setup(r => r["TOTAL_ACCESS"]).Returns(() => data[callCount].TotalAccess);
                _ = _readerMock.Setup(r => r["LAST_USED"]).Returns(() => data[callCount].LastUsed);
                _ = _readerMock.Setup(r => r["BLEVEL"]).Returns(() => data[callCount].BlevelDepth);
                _ = _readerMock.Setup(r => r["LEAF_BLOCKS"]).Returns(() => data[callCount].LeafBlocks);
                _ = _readerMock.Setup(r => r["DISTINCT_KEYS"]).Returns(() => data[callCount].DistinctKeys);
                _ = _readerMock.Setup(r => r["CLUSTERING_FACTOR"]).Returns(() => data[callCount].ClusteringFactor);
                _ = _readerMock.Setup(r => r["STATUS"]).Returns(() => data[callCount].Status);
                _ = _readerMock.Setup(r => r["COLUMN_NAMES"]).Returns(() => string.Join(",", data[callCount].ColumnNames));
                _ = _readerMock.Setup(r => r["IS_UNUSED"]).Returns(() => data[callCount].IsUnused ? "Y" : "N");
                _ = _readerMock.Setup(r => r["MONITORING_STARTED"]).Returns(() => data[callCount].MonitoringStarted);
            }
        }

        [Fact]
        public async Task GetTopSqlByExecutionsAsync_WithSpecificDateFormat_HandlesDateParametersCorrectly()
        {
            // Arrange - Test the specific date format issue from the MCP client
            var startTime = DateTime.Parse("2025-07-23T00:00:00");
            var endTime = DateTime.Parse("2025-07-25T23:59:59");
            
            SetupReaderForSqlPerformanceMetrics(new List<SqlPerformanceMetrics>
            {
                new SqlPerformanceMetrics
                {
                    SqlId = "TEST_SQL_1",
                    SqlText = "SELECT * FROM TEST_TABLE",
                    Executions = 100,
                    ElapsedTimeSeconds = 50.0,
                    CpuTimeSeconds = 25.0
                }
            });

            // Act
            var result = await _service.GetTopSqlByExecutionsAsync(10, startTime, endTime);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("TEST_SQL_1", result[0].SqlId);
            _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Once);
            _commandMock.Verify(c => c.ExecuteReader(), Times.Once);
        }
    }
}
