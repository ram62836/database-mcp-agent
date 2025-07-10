using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using OracleAgent.Core;
using OracleAgent.Core.Services;

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
            _ = _readerMock.SetupSequence(r => r.Read()).Returns(true).Returns(false);
            _ = _readerMock.Setup(r => r.FieldCount).Returns(1);
            _ = _readerMock.Setup(r => r.GetName(0)).Returns("DUMMY");
            _ = _readerMock.Setup(r => r.IsDBNull(0)).Returns(false);
            _ = _readerMock.Setup(r => r.GetValue(0)).Returns("X");
            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            string result = await _service.ExecuteRawSelectAsync(sql);
            Assert.Contains("DUMMY", result);
            Assert.Contains("X", result);
            _commandMock.VerifySet(c => c.CommandText = sql);
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_MultipleRows_ReturnsCorrectJson()
        {
            string sql = "SELECT * FROM EMPLOYEES";
            int readCount = 0;

            _ = _readerMock.Setup(r => r.Read()).Returns(() => readCount++ < 2);
            _ = _readerMock.Setup(r => r.FieldCount).Returns(2);
            _ = _readerMock.Setup(r => r.GetName(0)).Returns("ID");
            _ = _readerMock.Setup(r => r.GetName(1)).Returns("NAME");

            _ = _readerMock.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(false);
            _ = _readerMock.Setup(r => r.GetValue(It.Is<int>(i => i == 0 && readCount == 1))).Returns(1);
            _ = _readerMock.Setup(r => r.GetValue(It.Is<int>(i => i == 1 && readCount == 1))).Returns("John");
            _ = _readerMock.Setup(r => r.GetValue(It.Is<int>(i => i == 0 && readCount == 2))).Returns(2);
            _ = _readerMock.Setup(r => r.GetValue(It.Is<int>(i => i == 1 && readCount == 2))).Returns("Jane");

            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            string result = await _service.ExecuteRawSelectAsync(sql);

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

            _ = _readerMock.SetupSequence(r => r.Read()).Returns(true).Returns(false);
            _ = _readerMock.Setup(r => r.FieldCount).Returns(2);
            _ = _readerMock.Setup(r => r.GetName(0)).Returns("ID");
            _ = _readerMock.Setup(r => r.GetName(1)).Returns("DEPARTMENT");

            _ = _readerMock.Setup(r => r.IsDBNull(0)).Returns(false);
            _ = _readerMock.Setup(r => r.IsDBNull(1)).Returns(true);
            _ = _readerMock.Setup(r => r.GetValue(0)).Returns(1);

            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            string result = await _service.ExecuteRawSelectAsync(sql);

            Assert.Contains("ID", result);
            Assert.Contains("DEPARTMENT", result);
            Assert.Contains("null", result.ToLower());
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_NonSelectSql_ReturnsErrorMessage()
        {
            string sql = "UPDATE EMPLOYEES SET NAME = 'John' WHERE ID = 1";

            string result = await _service.ExecuteRawSelectAsync(sql);

            Assert.Contains("Invalid", result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Only select", result, StringComparison.OrdinalIgnoreCase);
            _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Never);
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_EmptySql_ReturnsErrorMessage()
        {
            string sql = "";

            string result = await _service.ExecuteRawSelectAsync(sql);

            Assert.Contains("Invalid", result, StringComparison.OrdinalIgnoreCase);
            _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Never);
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_SelectWithDisallowedCommand_ReturnsErrorMessage()
        {
            string sql = "SELECT * FROM EMPLOYEES; DELETE FROM EMPLOYEES";

            string result = await _service.ExecuteRawSelectAsync(sql);

            Assert.Contains("Invalid", result, StringComparison.OrdinalIgnoreCase);
            _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Never);
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_ThrowsException_WhenDbFails()
        {
            string sql = "SELECT * FROM DUAL";
            InvalidOperationException expectedException = new("Test exception");
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ExecuteRawSelectAsync(sql));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task ExecuteRawSelectAsync_NoResults_ReturnsEmptyJsonArray()
        {
            string sql = "SELECT * FROM EMPLOYEES WHERE 1=0";

            _ = _readerMock.Setup(r => r.Read()).Returns(false);
            _ = _readerMock.Setup(r => r.FieldCount).Returns(0);

            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            string result = await _service.ExecuteRawSelectAsync(sql);

            Assert.Equal("[]", result);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new RawSqlService(null, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }
    }
}
