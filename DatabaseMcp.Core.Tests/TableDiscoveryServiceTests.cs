using System.Data;
using DatabaseMcp.Core.Models;
using DatabaseMcp.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DatabaseMcp.Core.Tests
{
    [Collection("Database Tests")]
    public class TableDiscoveryServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock = new();
        private readonly Mock<IDbConnection> _connectionMock = new();
        private readonly Mock<IDbCommand> _commandMock = new();
        private readonly Mock<IDataReader> _readerMock = new();
        private readonly Mock<IDbDataParameter> _parameterMock = new();
        private readonly Mock<IDataParameterCollection> _parametersCollectionMock = new();
        private readonly Mock<ILogger<TableDiscoveryService>> _loggerMock = TestHelper.CreateLoggerMock<TableDiscoveryService>();
        private readonly TableDiscoveryService _service;

        public TableDiscoveryServiceTests()
        {
            SetupBasicMocks();
            _service = new TableDiscoveryService(_connectionFactoryMock.Object, _loggerMock.Object);
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
            _ = _commandMock.Setup(c => c.ExecuteScalar()).Returns("CREATE TABLE TEST_TABLE (ID NUMBER)");

            // Setup connection mock
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);

            // Setup connection factory mock
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);
        }

        [Fact]
        public async Task GetTablesMetadataByNameAsync_WithValidTableNames_ReturnsTableMetadata()
        {
            // Arrange
            List<string> tableNames = ["EMPLOYEE", "CUSTOMER"];
            const string expectedDdl = "CREATE TABLE EMPLOYEE (ID NUMBER, NAME VARCHAR2(100))";
            _ = _commandMock.Setup(c => c.ExecuteScalar()).Returns(expectedDdl);

            // Act
            List<TableMetadata> result = await _service.GetTablesMetadataByNamesAsync(tableNames);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, table => Assert.Equal(expectedDdl, table.Definition));
        }

        [Fact]
        public async Task GetTablesMetadataByNameAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            List<string> tableNames = [];

            // Act
            List<TableMetadata> result = await _service.GetTablesMetadataByNamesAsync(tableNames);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTablesMetadataByNameAsync_WithNullList_ReturnsEmptyList()
        {
            // Act
            List<TableMetadata> result = await _service.GetTablesMetadataByNamesAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTablesMetadataByNameAsync_WithSingleTableName_ReturnsSingleTableMetadata()
        {
            // Arrange
            List<string> tableNames = ["EMPLOYEE"];
            const string expectedDdl = "CREATE TABLE EMPLOYEE (ID NUMBER, NAME VARCHAR2(100))";
            _ = _commandMock.Setup(c => c.ExecuteScalar()).Returns(expectedDdl);

            // Act
            List<TableMetadata> result = await _service.GetTablesMetadataByNamesAsync(tableNames);

            // Assert
            Assert.NotNull(result);
            _ = Assert.Single(result);
            Assert.Equal(expectedDdl, result[0].Definition);
        }

        [Fact]
        public async Task GetTablesMetadataByNameAsync_WhenDatabaseThrowsException_PropagatesException()
        {
            // Arrange
            List<string> tableNames = ["INVALID_TABLE"];
            _ = _commandMock.Setup(c => c.ExecuteScalar()).Throws(new InvalidOperationException("Table not found"));

            // Act & Assert
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetTablesMetadataByNamesAsync(tableNames));
        }

        [Fact]
        public async Task GetTablesMetadataByNameAsync_WithNullScalarResult_ReturnsEmptyDefinition()
        {
            // Arrange
            List<string> tableNames = ["EMPLOYEE"];
            _ = _commandMock.Setup(c => c.ExecuteScalar()).Returns((object?)null);

            // Act
            List<TableMetadata> result = await _service.GetTablesMetadataByNamesAsync(tableNames);

            // Assert
            Assert.NotNull(result);
            _ = Assert.Single(result);
            Assert.Equal(string.Empty, result[0].Definition);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new TableDiscoveryService(null!, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }

        [Fact]
        public async Task GetTablesMetadataByNameAsync_CallsCorrectSqlCommand()
        {
            // Arrange
            List<string> tableNames = ["TEST_TABLE"];
            const string expectedSql = "SELECT DBMS_METADATA.GET_DDL('TABLE', :TableName) AS DDL FROM DUAL";

            // Act
            _ = await _service.GetTablesMetadataByNamesAsync(tableNames);

            // Assert
            _commandMock.VerifySet(c => c.CommandText = expectedSql, Times.Once);
            _parameterMock.VerifySet(p => p.ParameterName = "TableName", Times.Once);
            _parameterMock.VerifySet(p => p.Value = "TEST_TABLE", Times.Once);
        }
    }
}

