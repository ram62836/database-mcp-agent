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
            var tableName = "SAMPLE";
            var data = new List<IndexMetadata> { new IndexMetadata { IndexName = "IDX1", IsUnique = true } };
            
            // Setup parameter mock
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            int callCount = -1;
            _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            _readerMock.Setup(r => r["INDEX_NAME"]).Returns(() => data[callCount].IndexName);
            _readerMock.Setup(r => r["UNIQUENESS"]).Returns(() => data[callCount].IsUnique ? "UNIQUE" : "NONUNIQUE");
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.ListIndexesAsync(tableName);
            
            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
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
            var indexName = "IDX1";
            var expectedColumns = new List<string> { "COL1", "COL2" };
            
            // Setup parameter mock
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            int callCount = -1;
            _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < expectedColumns.Count);
            _readerMock.Setup(r => r["COLUMN_NAME"]).Returns(() => expectedColumns[callCount]);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetIndexColumnsAsync(indexName);
            
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
            var tableName = "ERROR_TABLE";
            var expectedException = new InvalidOperationException("Test exception");
            
            // Setup parameter mock for completeness
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ListIndexesAsync(tableName));
            Assert.Same(expectedException, exception);
        }
        
        [Fact]
        public async Task GetIndexColumnsAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            var indexName = "ERROR_INDEX";
            var expectedException = new InvalidOperationException("Test exception");
            
            // Setup parameter mock for completeness
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetIndexColumnsAsync(indexName));
            Assert.Same(expectedException, exception);
        }
        
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new IndexListingService(null, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }
        
        [Fact]
        public async Task ListIndexesAsync_HandlesEmptyResult()
        {
            // Arrange
            var tableName = "EMPTY_TABLE";
            var data = new List<IndexMetadata>();
            
            // Setup parameter mock
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            _readerMock.Setup(r => r.Read()).Returns(false);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.ListIndexesAsync(tableName);
            
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
            var indexName = "EMPTY_INDEX";
            
            // Setup parameter mock
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            _readerMock.Setup(r => r.Read()).Returns(false);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetIndexColumnsAsync(indexName);
            
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            
            // Verify the parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "IndexName");
            paramMock.VerifySet(p => p.Value = indexName.ToUpper());
        }
        
        private void SetupMocksForCommand(Mock<IDbCommand> commandMock, Mock<IDataReader> readerMock, Mock<IDbDataParameter> paramMock)
        {
            commandMock.Setup(c => c.ExecuteReader()).Returns(readerMock.Object);
            commandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
        }
    }
}
