using System.Data;
using DatabaseMcp.Core.Models;
using DatabaseMcp.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DatabaseMcp.Core.Tests
{
    public class KeyIdentificationServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
        private readonly Mock<IDbConnection> _connectionMock;
        private readonly Mock<IDbCommand> _commandMock;
        private readonly Mock<IDataReader> _readerMock;
        private readonly Mock<ILogger<KeyIdentificationService>> _loggerMock;
        private readonly KeyIdentificationService _service;

        public KeyIdentificationServiceTests()
        {
            _connectionFactoryMock = new Mock<IDbConnectionFactory>();
            _connectionMock = new Mock<IDbConnection>();
            _commandMock = new Mock<IDbCommand>();
            _readerMock = new Mock<IDataReader>();
            _loggerMock = TestHelper.CreateLoggerMock<KeyIdentificationService>();
            _service = new KeyIdentificationService(_connectionFactoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetPrimaryKeysAsync_ReturnsKeys()
        {
            // Arrange
            string tableName = "SAMPLE";
            List<KeyMetadata> data =
            [
                new KeyMetadata {
                    ColumnName = "ID",
                    ConstraintName = "PK1",
                    KeyType = "Primary"
                }
            ];

            // Setup parameter mock
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            SetupReaderForKeyMetadata(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<KeyMetadata> result = await _service.GetPrimaryKeysAsync(tableName);

            // Assert
            Assert.NotNull(result);
            _ = Assert.Single(result);
            Assert.Equal("ID", result[0].ColumnName);
            Assert.Equal("PK1", result[0].ConstraintName);
            Assert.Equal("Primary", result[0].KeyType);

            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }

        [Fact]
        public async Task GetForeignKeysAsync_ReturnsKeys()
        {
            // Arrange
            string tableName = "ORDERS";
            List<KeyMetadata> data =
            [
                new KeyMetadata {
                    ColumnName = "CUSTOMER_ID",
                    ConstraintName = "FK_CUSTOMER",
                    ReferencedConstraintName = "PK_CUSTOMER",
                    KeyType = "Foreign"
                }
            ];

            // Setup parameter mock
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            SetupReaderForForeignKeyMetadata(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<KeyMetadata> result = await _service.GetForeignKeysAsync(tableName);

            // Assert
            Assert.NotNull(result);
            _ = Assert.Single(result);
            Assert.Equal("CUSTOMER_ID", result[0].ColumnName);
            Assert.Equal("FK_CUSTOMER", result[0].ConstraintName);
            Assert.Equal("PK_CUSTOMER", result[0].ReferencedConstraintName);
            Assert.Equal("Foreign", result[0].KeyType);

            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }

        [Fact]
        public async Task GetForeignKeyRelationshipsAsync_ReturnsRelationships()
        {
            // Arrange
            List<(string ConstraintName, string TableName, string ColumnName)> relationshipData =
            [
                ("FK_ORDERS_CUSTOMERS", "ORDERS", "CUSTOMER_ID"),
                ("FK_ORDERS_CUSTOMERS", "ORDERS", "STORE_ID"),
                ("FK_ITEMS_PRODUCTS", "ORDER_ITEMS", "PRODUCT_ID")
            ];

            SetupReaderForRelationships(_readerMock, relationshipData);
            SetupMocksForCommand(_commandMock, _readerMock, null);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            Dictionary<string, List<string>> result = await _service.GetForeignKeyRelationshipsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Two distinct constraint names
            Assert.Contains("FK_ORDERS_CUSTOMERS", result.Keys);
            Assert.Contains("FK_ITEMS_PRODUCTS", result.Keys);
            Assert.Equal(2, result["FK_ORDERS_CUSTOMERS"].Count); // Two columns in this constraint
            _ = Assert.Single(result["FK_ITEMS_PRODUCTS"]); // One column in this constraint
        }

        [Fact]
        public async Task GetPrimaryKeysAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            string tableName = "ERROR_TABLE";
            InvalidOperationException expectedException = new("Test exception");

            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetPrimaryKeysAsync(tableName));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetForeignKeysAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            string tableName = "ERROR_TABLE";
            InvalidOperationException expectedException = new("Test exception");

            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetForeignKeysAsync(tableName));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetForeignKeyRelationshipsAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            InvalidOperationException expectedException = new("Test exception");

            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                _service.GetForeignKeyRelationshipsAsync);
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new KeyIdentificationService(null, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }

        [Fact]
        public async Task GetPrimaryKeysAsync_HandlesEmptyResult()
        {
            // Arrange
            string tableName = "EMPTY_TABLE";

            // Setup parameter mock
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            _ = _readerMock.Setup(r => r.Read()).Returns(false); // No rows
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<KeyMetadata> result = await _service.GetPrimaryKeysAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }

        [Fact]
        public async Task GetForeignKeysAsync_HandlesEmptyResult()
        {
            // Arrange
            string tableName = "EMPTY_TABLE";

            // Setup parameter mock
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            _ = _readerMock.Setup(r => r.Read()).Returns(false); // No rows
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<KeyMetadata> result = await _service.GetForeignKeysAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }

        [Fact]
        public async Task GetForeignKeyRelationshipsAsync_HandlesEmptyResult()
        {
            // Arrange
            _ = _readerMock.Setup(r => r.Read()).Returns(false); // No rows
            SetupMocksForCommand(_commandMock, _readerMock, null);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            Dictionary<string, List<string>> result = await _service.GetForeignKeyRelationshipsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPrimaryKeysAsync_HandlesMultipleKeys()
        {
            // Arrange
            string tableName = "COMPOSITE_KEY_TABLE";
            List<KeyMetadata> data =
            [
                new KeyMetadata { ColumnName = "ID1", ConstraintName = "PK_COMPOSITE", KeyType = "Primary" },
                new KeyMetadata { ColumnName = "ID2", ConstraintName = "PK_COMPOSITE", KeyType = "Primary" }
            ];

            // Setup parameter mock
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            SetupReaderForKeyMetadata(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<KeyMetadata> result = await _service.GetPrimaryKeysAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("ID1", result[0].ColumnName);
            Assert.Equal("ID2", result[1].ColumnName);
            Assert.All(result, key => Assert.Equal("PK_COMPOSITE", key.ConstraintName));
            Assert.All(result, key => Assert.Equal("Primary", key.KeyType));

            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }

        private void SetupReaderForKeyMetadata(Mock<IDataReader> readerMock, List<KeyMetadata> data)
        {
            int callCount = -1;
            _ = readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);

            if (data.Count > 0)
            {
                _ = readerMock.Setup(r => r["COLUMN_NAME"]).Returns(() => data[callCount].ColumnName);
                _ = readerMock.Setup(r => r["CONSTRAINT_NAME"]).Returns(() => data[callCount].ConstraintName);
            }
        }

        private void SetupReaderForForeignKeyMetadata(Mock<IDataReader> readerMock, List<KeyMetadata> data)
        {
            int callCount = -1;
            _ = readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);

            if (data.Count > 0)
            {
                _ = readerMock.Setup(r => r["COLUMN_NAME"]).Returns(() => data[callCount].ColumnName);
                _ = readerMock.Setup(r => r["CONSTRAINT_NAME"]).Returns(() => data[callCount].ConstraintName);
                _ = readerMock.Setup(r => r["R_CONSTRAINT_NAME"]).Returns(() => data[callCount].ReferencedConstraintName);
            }
        }

        private void SetupReaderForRelationships(Mock<IDataReader> readerMock, List<(string ConstraintName, string TableName, string ColumnName)> data)
        {
            int callCount = -1;
            _ = readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);

            if (data.Count > 0)
            {
                _ = readerMock.Setup(r => r["CONSTRAINT_NAME"]).Returns(() => data[callCount].ConstraintName);
                _ = readerMock.Setup(r => r["TABLE_NAME"]).Returns(() => data[callCount].TableName);
                _ = readerMock.Setup(r => r["COLUMN_NAME"]).Returns(() => data[callCount].ColumnName);
            }
        }

        private void SetupMocksForCommand(Mock<IDbCommand> commandMock, Mock<IDataReader> readerMock, Mock<IDbDataParameter>? paramMock)
        {
            _ = commandMock.Setup(c => c.ExecuteReader()).Returns(readerMock.Object);

            if (paramMock != null)
            {
                _ = commandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            }

            _ = commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
        }
    }
}
