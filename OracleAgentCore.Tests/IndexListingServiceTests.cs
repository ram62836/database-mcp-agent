using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using OracleAgent.Core;
using OracleAgent.Core.Models;
using OracleAgent.Core.Services;

namespace OracleAgentCore.Tests
{
    public class IndexListingServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
        private readonly Mock<IDbConnection> _connectionMock;
        private readonly Mock<IDbCommand> _commandMock;
        private readonly Mock<IDataReader> _readerMock;
        private readonly Mock<ILogger<IndexListingService>> _loggerMock;
        private readonly IndexListingService _service;

        public IndexListingServiceTests()
        {
            _connectionFactoryMock = new Mock<IDbConnectionFactory>();
            _connectionMock = new Mock<IDbConnection>();
            _commandMock = new Mock<IDbCommand>();
            _readerMock = new Mock<IDataReader>();
            _loggerMock = TestHelper.CreateLoggerMock<IndexListingService>();
            _service = new IndexListingService(_connectionFactoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ListIndexesAsync_ReturnsIndexes()
        {
            // Arrange
            string tableName = "SAMPLE";
            List<IndexMetadata> data = new()
            { new IndexMetadata { IndexName = "IDX1", IsUnique = true } };

            // Setup parameter mock
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            _ = _readerMock.Setup(r => r["INDEX_NAME"]).Returns(() => data[callCount].IndexName);
            _ = _readerMock.Setup(r => r["UNIQUENESS"]).Returns(() => data[callCount].IsUnique ? "UNIQUE" : "NONUNIQUE");
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<IndexMetadata> result = await _service.ListIndexesAsync(tableName);

            // Assert
            Assert.NotNull(result);
            _ = Assert.Single(result);
            Assert.Equal("IDX1", result[0].IndexName);
            Assert.True(result[0].IsUnique);

            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }

        [Fact]
        public async Task GetIndexColumnsAsync_ReturnsColumns()
        {
            // Arrange
            string indexName = "IDX1";
            List<string> expectedColumns = new()
            { "COL1", "COL2" };

            // Setup parameter mock
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < expectedColumns.Count);
            _ = _readerMock.Setup(r => r["COLUMN_NAME"]).Returns(() => expectedColumns[callCount]);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<string> result = await _service.GetIndexColumnsAsync(indexName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedColumns.Count, result.Count);
            Assert.Equal(expectedColumns, result);

            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "IndexName");
            paramMock.VerifySet(p => p.Value = indexName.ToUpper());
        }

        [Fact]
        public async Task ListIndexesAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            string tableName = "ERROR_TABLE";
            InvalidOperationException expectedException = new("Test exception");

            // Setup parameter mock for completeness
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ListIndexesAsync(tableName));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetIndexColumnsAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            string indexName = "ERROR_INDEX";
            InvalidOperationException expectedException = new("Test exception");

            // Setup parameter mock for completeness
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetIndexColumnsAsync(indexName));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new IndexListingService(null, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }

        [Fact]
        public async Task ListIndexesAsync_HandlesEmptyResult()
        {
            // Arrange
            string tableName = "EMPTY_TABLE";
            List<IndexMetadata> data = new();

            // Setup parameter mock
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            _ = _readerMock.Setup(r => r.Read()).Returns(false);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<IndexMetadata> result = await _service.ListIndexesAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }

        [Fact]
        public async Task GetIndexColumnsAsync_HandlesEmptyResult()
        {
            // Arrange
            string indexName = "EMPTY_INDEX";

            // Setup parameter mock
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            _ = _readerMock.Setup(r => r.Read()).Returns(false);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<string> result = await _service.GetIndexColumnsAsync(indexName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "IndexName");
            paramMock.VerifySet(p => p.Value = indexName.ToUpper());
        }

        private void SetupMocksForCommand(Mock<IDbCommand> commandMock, Mock<IDataReader> readerMock, Mock<IDbDataParameter> paramMock)
        {
            _ = commandMock.Setup(c => c.ExecuteReader()).Returns(readerMock.Object);
            _ = commandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _ = commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
        }
    }
}
