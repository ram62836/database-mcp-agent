using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OracleAgent.Core;
using OracleAgent.Core.Interfaces;
using OracleAgent.Core.Models;
using OracleAgent.Core.Services;
using Xunit;

namespace OracleAgentCore.Tests
{
    public class ColumnMetadataServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
        private readonly Mock<IDbConnection> _connectionMock;
        private readonly Mock<IDbCommand> _commandMock;
        private readonly Mock<IDataReader> _readerMock;
        private readonly Mock<ILogger<ColumnMetadataService>> _loggerMock;
        private readonly ColumnMetadataService _service;

        public ColumnMetadataServiceTests()
        {
            _connectionFactoryMock = new Mock<IDbConnectionFactory>();
            _connectionMock = new Mock<IDbConnection>();
            _commandMock = new Mock<IDbCommand>();
            _readerMock = new Mock<IDataReader>();
            _loggerMock = TestHelper.CreateLoggerMock<ColumnMetadataService>();
            _service = new ColumnMetadataService(_connectionFactoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetColumnMetadataAsync_ReturnsMetadataList()
        {
            // Arrange
            var tableName = "SAMPLE";
            var data = TestDataSeeder.GetSampleColumnMetadataList();
            
            // Setup parameter mock
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            SetupReaderForColumnMetadata(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetColumnMetadataAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data.Count, result.Count);
            
            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
            
            // Verify the command was setup correctly
            _commandMock.VerifySet(c => c.CommandText = It.IsAny<string>());
            _commandMock.Verify(c => c.CreateParameter(), Times.Once);
            _commandMock.Verify(c => c.Parameters.Add(It.IsAny<IDbDataParameter>()), Times.Once);
        }

        [Fact]
        public async Task GetColumnNamesAsync_ReturnsNamesList()
        {
            // Arrange
            var tableName = "SAMPLE";
            var data = TestDataSeeder.GetSampleColumnMetadataList();
            
            // Setup parameter mock
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            SetupReaderForColumnNames(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetColumnNamesAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data.Count, result.Count);
            
            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }

        [Fact]
        public async Task GetDataTypesAsync_ReturnsDataTypesList()
        {
            // Arrange
            var tableName = "SAMPLE";
            var data = TestDataSeeder.GetSampleColumnMetadataList();
            
            // Setup parameter mock
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            SetupReaderForDataTypes(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetDataTypesAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data.Count, result.Count);
            Assert.All(result, item => Assert.NotNull(item.Name));
            Assert.All(result, item => Assert.NotNull(item.DataType));
            
            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }

        [Fact]
        public async Task GetNullabilityAsync_ReturnsNullabilityList()
        {
            // Arrange
            var tableName = "SAMPLE";
            var data = TestDataSeeder.GetSampleColumnMetadataList();
            
            // Setup parameter mock
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            SetupReaderForNullability(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetNullabilityAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data.Count, result.Count);
            Assert.Contains(result, item => item.Name == "ID" && item.IsNullable == false);
            Assert.Contains(result, item => item.Name == "NAME" && item.IsNullable == true);
            
            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }

        [Fact]
        public async Task GetDefaultValuesAsync_ReturnsDefaultsList()
        {
            // Arrange
            var tableName = "SAMPLE";
            var data = TestDataSeeder.GetSampleColumnMetadataList();
            
            // Setup parameter mock
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            SetupReaderForDefaultValues(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetDefaultValuesAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data.Count, result.Count);
            Assert.Contains(result, item => item.Name == "ID" && item.DefaultValue == null);
            Assert.Contains(result, item => item.Name == "NAME" && item.DefaultValue == "'N/A'");
            
            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }

        [Fact]
        public async Task GetTablesByColumnNameAsync_ReturnsTablesList()
        {
            // Arrange
            var columnNamePattern = "ID";
            var expectedTables = new List<string> { "TABLE1", "TABLE2" };

            // Setup parameter mock
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            int callCount = -1;
            _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < expectedTables.Count);
            _readerMock.Setup(r => r["TABLE_NAME"]).Returns(() => expectedTables[callCount]);
            
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetTablesByColumnNameAsync(columnNamePattern);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTables.Count, result.Count);
            Assert.Equal(expectedTables, result);
            
            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "ColumnNamePattern");
            string expectedPattern = $"%{columnNamePattern.ToUpper()}%";
            paramMock.VerifySet(p => p.Value = It.Is<string>(s => s == expectedPattern));
        }

        [Fact]
        public async Task GetColumnMetadataAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            var tableName = "ERROR_TABLE";
            var expectedException = new InvalidOperationException("Test exception");
            
            // Setup parameter mock for completeness even though it won't be used
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetColumnMetadataAsync(tableName));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetColumnNamesAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            var tableName = "ERROR_TABLE";
            var expectedException = new InvalidOperationException("Test exception");
            
            // Setup parameter mock for completeness even though it won't be used
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetColumnNamesAsync(tableName));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetDataTypesAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            var tableName = "ERROR_TABLE";
            var expectedException = new InvalidOperationException("Test exception");
            
            // Setup parameter mock for completeness even though it won't be used
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetDataTypesAsync(tableName));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetNullabilityAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            var tableName = "ERROR_TABLE";
            var expectedException = new InvalidOperationException("Test exception");
            
            // Setup parameter mock for completeness even though it won't be used
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetNullabilityAsync(tableName));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetDefaultValuesAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            var tableName = "ERROR_TABLE";
            var expectedException = new InvalidOperationException("Test exception");
            
            // Setup parameter mock for completeness even though it won't be used
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetDefaultValuesAsync(tableName));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetTablesByColumnNameAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            var columnNamePattern = "ERROR_COLUMN";
            var expectedException = new InvalidOperationException("Test exception");
            
            // Setup parameter mock for completeness even though it won't be used
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetTablesByColumnNameAsync(columnNamePattern));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ColumnMetadataService(null, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ColumnMetadataService(_connectionFactoryMock.Object, null));
            Assert.Equal("logger", exception.ParamName);
        }
        
        [Fact]
        public async Task GetColumnMetadataAsync_HandlesNullTableName()
        {
            // Arrange
            string tableName = null;
            var data = new List<ColumnMetadata>();
            
            // Setup parameter mock
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            SetupReaderForColumnMetadata(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetColumnMetadataAsync(tableName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            
            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = null);
        }

        private void SetupReaderForColumnMetadata(Mock<IDataReader> readerMock, List<ColumnMetadata> data)
        {
            var queue = new Queue<ColumnMetadata>(data);
            int callCount = -1;
            readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            if (data.Count > 0)
            {
                readerMock.Setup(r => r["COLUMN_NAME"]).Returns(() => data[callCount].Name);
                readerMock.Setup(r => r["DATA_TYPE"]).Returns(() => data[callCount].DataType);
                readerMock.Setup(r => r["NULLABLE"]).Returns(() => data[callCount].IsNullable ? "Y" : "N");
                readerMock.Setup(r => r["DATA_DEFAULT"]).Returns(() => data[callCount].DefaultValue);
            }
        }

        private void SetupReaderForColumnNames(Mock<IDataReader> readerMock, List<ColumnMetadata> data)
        {
            var queue = new Queue<ColumnMetadata>(data);
            int callCount = -1;
            readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            if (data.Count > 0)
            {
                readerMock.Setup(r => r["COLUMN_NAME"]).Returns(() => data[callCount].Name);
            }
        }

        private void SetupReaderForDataTypes(Mock<IDataReader> readerMock, List<ColumnMetadata> data)
        {
            var queue = new Queue<ColumnMetadata>(data);
            int callCount = -1;
            readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            if (data.Count > 0)
            {
                readerMock.Setup(r => r["COLUMN_NAME"]).Returns(() => data[callCount].Name);
                readerMock.Setup(r => r["DATA_TYPE"]).Returns(() => data[callCount].DataType);
            }
        }

        private void SetupReaderForNullability(Mock<IDataReader> readerMock, List<ColumnMetadata> data)
        {
            var queue = new Queue<ColumnMetadata>(data);
            int callCount = -1;
            readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            if (data.Count > 0)
            {
                readerMock.Setup(r => r["COLUMN_NAME"]).Returns(() => data[callCount].Name);
                readerMock.Setup(r => r["NULLABLE"]).Returns(() => data[callCount].IsNullable ? "Y" : "N");
            }
        }

        private void SetupReaderForDefaultValues(Mock<IDataReader> readerMock, List<ColumnMetadata> data)
        {
            var queue = new Queue<ColumnMetadata>(data);
            int callCount = -1;
            readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            if (data.Count > 0)
            {
                readerMock.Setup(r => r["COLUMN_NAME"]).Returns(() => data[callCount].Name);
                readerMock.Setup(r => r["DATA_DEFAULT"]).Returns(() => data[callCount].DefaultValue);
            }
        }

        private void SetupMocksForCommand(Mock<IDbCommand> commandMock, Mock<IDataReader> readerMock, Mock<IDbDataParameter> paramMock)
        {
            commandMock.Setup(c => c.ExecuteReader()).Returns(readerMock.Object);
            commandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
        }
    }
}
