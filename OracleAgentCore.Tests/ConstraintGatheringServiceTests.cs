using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OracleAgent.Core;
using OracleAgent.Core.Models;
using OracleAgent.Core.Services;
using Xunit;

namespace OracleAgentCore.Tests
{
    public class ConstraintGatheringServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock = new();
        private readonly Mock<IDbConnection> _connectionMock = new();
        private readonly Mock<IDbCommand> _commandMock = new();
        private readonly Mock<IDataReader> _readerMock = new();
        private readonly Mock<ILogger<ConstraintGatheringService>> _loggerMock = TestHelper.CreateLoggerMock<ConstraintGatheringService>();
        private readonly ConstraintGatheringService _service;

        public ConstraintGatheringServiceTests()
        {
            _service = new ConstraintGatheringService(_connectionFactoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetUniqueConstraintsAsync_ReturnsList()
        {
            var tableName = "SAMPLE";
            var data = new List<ConstraintMetadata> { 
                new ConstraintMetadata { 
                    ConstraintName = "UQ1", 
                    ColumnName = "COL1", 
                    ConstraintType = "Unique" 
                } 
            };
            
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            SetupReaderForUniqueConstraint(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            var result = await _service.GetUniqueConstraintsAsync(tableName);
            
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("UQ1", result[0].ConstraintName);
            Assert.Equal("COL1", result[0].ColumnName);
            Assert.Equal("Unique", result[0].ConstraintType);
            
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }
        
        [Fact]
        public async Task GetCheckConstraintsAsync_ReturnsList()
        {
            var tableName = "SAMPLE";
            var data = new List<ConstraintMetadata> { 
                new ConstraintMetadata { 
                    ConstraintName = "CK1", 
                    SearchCondition = "COL1 > 0", 
                    ConstraintType = "Check" 
                } 
            };
            
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            SetupReaderForCheckConstraint(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            var result = await _service.GetCheckConstraintsAsync(tableName);
            
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("CK1", result[0].ConstraintName);
            Assert.Equal("COL1 > 0", result[0].SearchCondition);
            Assert.Equal("Check", result[0].ConstraintType);
            
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }
        
        [Fact]
        public async Task GetUniqueConstraintsAsync_ThrowsException_WhenDbFails()
        {
            var tableName = "ERROR_TABLE";
            var expectedException = new InvalidOperationException("Test exception");
            
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetUniqueConstraintsAsync(tableName));
            Assert.Same(expectedException, exception);
        }
        
        [Fact]
        public async Task GetCheckConstraintsAsync_ThrowsException_WhenDbFails()
        {
            var tableName = "ERROR_TABLE";
            var expectedException = new InvalidOperationException("Test exception");
            
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetCheckConstraintsAsync(tableName));
            Assert.Same(expectedException, exception);
        }
        
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ConstraintGatheringService(null, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }
        
        [Fact]
        public async Task GetUniqueConstraintsAsync_HandlesEmptyResult()
        {
            var tableName = "EMPTY_TABLE";
            
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            _readerMock.Setup(r => r.Read()).Returns(false);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            var result = await _service.GetUniqueConstraintsAsync(tableName);
            
            Assert.NotNull(result);
            Assert.Empty(result);
            
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }
        
        [Fact]
        public async Task GetCheckConstraintsAsync_HandlesEmptyResult()
        {
            var tableName = "EMPTY_TABLE";
            
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            _readerMock.Setup(r => r.Read()).Returns(false);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            var result = await _service.GetCheckConstraintsAsync(tableName);
            
            Assert.NotNull(result);
            Assert.Empty(result);
            
            paramMock.VerifySet(p => p.ParameterName = "TableName");
            paramMock.VerifySet(p => p.Value = tableName.ToUpper());
        }
        
        [Fact]
        public async Task GetUniqueConstraintsAsync_HandlesMultipleConstraints()
        {
            var tableName = "MULTI_CONSTRAINT";
            var data = new List<ConstraintMetadata> { 
                new ConstraintMetadata { 
                    ConstraintName = "UQ1", 
                    ColumnName = "COL1", 
                    ConstraintType = "Unique" 
                },
                new ConstraintMetadata { 
                    ConstraintName = "UQ2", 
                    ColumnName = "COL2", 
                    ConstraintType = "Unique" 
                }
            };
            
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            SetupReaderForUniqueConstraint(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock, paramMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            var result = await _service.GetUniqueConstraintsAsync(tableName);
            
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("UQ1", result[0].ConstraintName);
            Assert.Equal("COL1", result[0].ColumnName);
            Assert.Equal("UQ2", result[1].ConstraintName);
            Assert.Equal("COL2", result[1].ColumnName);
        }

        private void SetupReaderForUniqueConstraint(Mock<IDataReader> readerMock, List<ConstraintMetadata> data)
        {
            int callCount = -1;
            readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            
            if (data.Count > 0)
            {
                readerMock.Setup(r => r["CONSTRAINT_NAME"]).Returns(() => data[callCount].ConstraintName);
                readerMock.Setup(r => r["COLUMN_NAME"]).Returns(() => data[callCount].ColumnName);
            }
        }
        
        private void SetupReaderForCheckConstraint(Mock<IDataReader> readerMock, List<ConstraintMetadata> data)
        {
            int callCount = -1;
            readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            
            if (data.Count > 0)
            {
                readerMock.Setup(r => r["CONSTRAINT_NAME"]).Returns(() => data[callCount].ConstraintName);
                readerMock.Setup(r => r["SEARCH_CONDITION"]).Returns(() => data[callCount].SearchCondition);
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
