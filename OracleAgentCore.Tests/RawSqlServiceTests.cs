using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OracleAgent.Core;
using OracleAgent.Core.Interfaces;
using OracleAgent.Core.Services;
using Xunit;

namespace OracleAgentCore.Tests
{
    public class RawSqlServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock = new();
        private readonly Mock<IDbConnection> _connectionMock = new();
        private readonly Mock<IDbCommand> _commandMock = new();
        private readonly Mock<IDataReader> _readerMock = new();
        private readonly Mock<ILogger<RawSqlService>> _loggerMock = TestHelper.CreateLoggerMock<RawSqlService>();
        private readonly RawSqlService _service;

        public RawSqlServiceTests()
        {
            _service = new RawSqlService(_connectionFactoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_ValidSelect_ReturnsJson()
        {
            string sql = "SELECT * FROM DUAL";
            _readerMock.SetupSequence(r => r.Read()).Returns(true).Returns(false);
            _readerMock.Setup(r => r.FieldCount).Returns(1);
            _readerMock.Setup(r => r.GetName(0)).Returns("DUMMY");
            _readerMock.Setup(r => r.IsDBNull(0)).Returns(false);
            _readerMock.Setup(r => r.GetValue(0)).Returns("X");
            _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            var result = await _service.ExecuteRawSelectAsync(sql);
            Assert.Contains("DUMMY", result);
            Assert.Contains("X", result);
            _commandMock.VerifySet(c => c.CommandText = sql);
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_MultipleRows_ReturnsCorrectJson()
        {
            string sql = "SELECT * FROM EMPLOYEES";
            int readCount = 0;

            _readerMock.Setup(r => r.Read()).Returns(() => readCount++ < 2);
            _readerMock.Setup(r => r.FieldCount).Returns(2);
            _readerMock.Setup(r => r.GetName(0)).Returns("ID");
            _readerMock.Setup(r => r.GetName(1)).Returns("NAME");

            _readerMock.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(false);
            _readerMock.Setup(r => r.GetValue(It.Is<int>(i => i == 0 && readCount == 1))).Returns(1);
            _readerMock.Setup(r => r.GetValue(It.Is<int>(i => i == 1 && readCount == 1))).Returns("John");
            _readerMock.Setup(r => r.GetValue(It.Is<int>(i => i == 0 && readCount == 2))).Returns(2);
            _readerMock.Setup(r => r.GetValue(It.Is<int>(i => i == 1 && readCount == 2))).Returns("Jane");

            _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            var result = await _service.ExecuteRawSelectAsync(sql);

            Assert.Contains("ID", result);
            Assert.Contains("NAME", result);
            Assert.Contains("John", result);
            Assert.Contains("Jane", result);
            Assert.Contains("1", result);
            Assert.Contains("2", result);
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_NullValues_HandledCorrectly()
        {
            string sql = "SELECT * FROM EMPLOYEES WHERE ID = 1";

            _readerMock.SetupSequence(r => r.Read()).Returns(true).Returns(false);
            _readerMock.Setup(r => r.FieldCount).Returns(2);
            _readerMock.Setup(r => r.GetName(0)).Returns("ID");
            _readerMock.Setup(r => r.GetName(1)).Returns("DEPARTMENT");

            _readerMock.Setup(r => r.IsDBNull(0)).Returns(false);
            _readerMock.Setup(r => r.IsDBNull(1)).Returns(true);
            _readerMock.Setup(r => r.GetValue(0)).Returns(1);

            _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            var result = await _service.ExecuteRawSelectAsync(sql);

            Assert.Contains("ID", result);
            Assert.Contains("DEPARTMENT", result);
            Assert.Contains("null", result.ToLower());
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_NonSelectSql_ReturnsErrorMessage()
        {
            string sql = "UPDATE EMPLOYEES SET NAME = 'John' WHERE ID = 1";

            var result = await _service.ExecuteRawSelectAsync(sql);

            Assert.Contains("Invalid", result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Only select", result, StringComparison.OrdinalIgnoreCase);
            _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Never);
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_EmptySql_ReturnsErrorMessage()
        {
            string sql = "";

            var result = await _service.ExecuteRawSelectAsync(sql);

            Assert.Contains("Invalid", result, StringComparison.OrdinalIgnoreCase);
            _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Never);
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_SelectWithDisallowedCommand_ReturnsErrorMessage()
        {
            string sql = "SELECT * FROM EMPLOYEES; DELETE FROM EMPLOYEES";

            var result = await _service.ExecuteRawSelectAsync(sql);

            Assert.Contains("Invalid", result, StringComparison.OrdinalIgnoreCase);
            _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Never);
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_ThrowsException_WhenDbFails()
        {
            string sql = "SELECT * FROM DUAL";
            var expectedException = new InvalidOperationException("Test exception");
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ExecuteRawSelectAsync(sql));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_NoResults_ReturnsEmptyJsonArray()
        {
            string sql = "SELECT * FROM EMPLOYEES WHERE 1=0";

            _readerMock.Setup(r => r.Read()).Returns(false);
            _readerMock.Setup(r => r.FieldCount).Returns(0);

            _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            var result = await _service.ExecuteRawSelectAsync(sql);

            Assert.Equal("[]", result);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new RawSqlService(null, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }
    }
}
