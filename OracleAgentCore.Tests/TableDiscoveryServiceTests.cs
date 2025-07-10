using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using OracleAgent.Core;
using OracleAgent.Core.Models;
using OracleAgent.Core.Services;

namespace OracleAgentCore.Tests
{
    public class TableDiscoveryServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock = new();
        private readonly Mock<IDbConnection> _connectionMock = new();
        private readonly Mock<IDbCommand> _commandMock = new();
        private readonly Mock<IDataReader> _readerMock = new();
        private readonly Mock<ILogger<TableDiscoveryService>> _loggerMock = TestHelper.CreateLoggerMock<TableDiscoveryService>();
        private readonly TableDiscoveryService _service;

        public TableDiscoveryServiceTests()
        {
            _service = new TableDiscoveryService(_connectionFactoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllUserDefinedTablesAsync_ReturnsList()
        {
            // Arrange
            // Ensure the cache file does not exist so the DB path is used
            if (File.Exists(AppConstants.TablesMetadatJsonFile))
            {
                File.Delete(AppConstants.TablesMetadatJsonFile);
            }

            List<TableMetadata> data = new()
            {
                new TableMetadata { TableName = "T1", Definition = "DDL1" },
                new TableMetadata { TableName = "T2", Definition = "DDL2" }
            };

            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            _ = _readerMock.Setup(r => r["TABLE_NAME"]).Returns(() => data[callCount].TableName);
            _ = _readerMock.Setup(r => r["TABLE_DDL"]).Returns(() => data[callCount].Definition);

            SetupMocksForCommand(_commandMock, _readerMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<TableMetadata> result = await _service.GetAllUserDefinedTablesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data.Count, result.Count);
            Assert.Equal("T1", result[0].TableName);
            Assert.Equal("DDL1", result[0].Definition);
            Assert.Equal("T2", result[1].TableName);
            Assert.Equal("DDL2", result[1].Definition);

            // Verify the command setup
            _commandMock.VerifySet(c => c.CommandText = It.IsAny<string>());
        }

        [Fact]
        public async Task GetAllUserDefinedTablesAsync_UsesCacheWhenAvailable()
        {
            // Arrange
            // Create a cache file with test data
            List<TableMetadata> cacheData = new()
            {
                new TableMetadata { TableName = "CACHED1", Definition = "CACHED_DDL1" },
                new TableMetadata { TableName = "CACHED2", Definition = "CACHED_DDL2" }
            };

            _ = Directory.CreateDirectory(Directory.GetCurrentDirectory());
            await File.WriteAllTextAsync(AppConstants.TablesMetadatJsonFile,
                System.Text.Json.JsonSerializer.Serialize(cacheData));

            // No database mock setup needed as it should use the cache

            try
            {
                // Act
                List<TableMetadata> result = await _service.GetAllUserDefinedTablesAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(cacheData.Count, result.Count);
                Assert.Equal("CACHED1", result[0].TableName);
                Assert.Equal("CACHED_DDL1", result[0].Definition);

                // Verify that database was not called
                _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Never);
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.TablesMetadatJsonFile))
                {
                    File.Delete(AppConstants.TablesMetadatJsonFile);
                }
            }
        }

        [Fact]
        public async Task GetTablesByNameAsync_FiltersByTableName()
        {
            // Arrange
            // Ensure the cache file does not exist
            if (File.Exists(AppConstants.TablesMetadatJsonFile))
            {
                File.Delete(AppConstants.TablesMetadatJsonFile);
            }

            List<TableMetadata> allTables = new()
            {
                new TableMetadata { TableName = "EMPLOYEE", Definition = "DDL1" },
                new TableMetadata { TableName = "CUSTOMER", Definition = "DDL2" },
                new TableMetadata { TableName = "PRODUCT", Definition = "DDL3" }
            };

            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < allTables.Count);
            _ = _readerMock.Setup(r => r["TABLE_NAME"]).Returns(() => allTables[callCount].TableName);
            _ = _readerMock.Setup(r => r["TABLE_DDL"]).Returns(() => allTables[callCount].Definition);

            SetupMocksForCommand(_commandMock, _readerMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Filter criteria
            List<string> tableNamesToFilter = new()
            { "EMPLOYEE", "PRODUCT" };

            // Act
            List<TableMetadata> result = await _service.GetTablesByNameAsync(tableNamesToFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.TableName == "EMPLOYEE");
            Assert.Contains(result, t => t.TableName == "PRODUCT");
            Assert.DoesNotContain(result, t => t.TableName == "CUSTOMER");
        }

        [Fact]
        public async Task GetAllUserDefinedTablesAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            if (File.Exists(AppConstants.TablesMetadatJsonFile))
            {
                File.Delete(AppConstants.TablesMetadatJsonFile);
            }

            InvalidOperationException expectedException = new("Test exception");
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                _service.GetAllUserDefinedTablesAsync);
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetTablesByNameAsync_ReturnsEmptyList_WhenNoMatch()
        {
            // Arrange
            // Create a cache file with test data to avoid DB call
            List<TableMetadata> cacheData = new()
            {
                new TableMetadata { TableName = "TABLE1", Definition = "DDL1" },
                new TableMetadata { TableName = "TABLE2", Definition = "DDL2" }
            };

            _ = Directory.CreateDirectory(Directory.GetCurrentDirectory());
            await File.WriteAllTextAsync(AppConstants.TablesMetadatJsonFile,
                System.Text.Json.JsonSerializer.Serialize(cacheData));

            try
            {
                // Act
                List<TableMetadata> result = await _service.GetTablesByNameAsync(["NONEXISTENT"]);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.TablesMetadatJsonFile))
                {
                    File.Delete(AppConstants.TablesMetadatJsonFile);
                }
            }
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new TableDiscoveryService(null, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }

        [Fact]
        public async Task GetAllUserDefinedTablesAsync_HandlesEmptyResult()
        {
            // Arrange
            if (File.Exists(AppConstants.TablesMetadatJsonFile))
            {
                File.Delete(AppConstants.TablesMetadatJsonFile);
            }

            _ = _readerMock.Setup(r => r.Read()).Returns(false);

            SetupMocksForCommand(_commandMock, _readerMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<TableMetadata> result = await _service.GetAllUserDefinedTablesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        private void SetupMocksForCommand(Mock<IDbCommand> commandMock, Mock<IDataReader> readerMock)
        {
            _ = commandMock.Setup(c => c.ExecuteReader()).Returns(readerMock.Object);
            _ = commandMock.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
            _ = commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
        }
    }
}
