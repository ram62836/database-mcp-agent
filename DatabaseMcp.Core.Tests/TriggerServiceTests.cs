using System.Data;
using DatabaseMcp.Core.Models;
using DatabaseMcp.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DatabaseMcp.Core.Tests
{
    [Collection("Database Tests")]
    public class TriggerServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock = new();
        private readonly Mock<IDbConnection> _connectionMock = new();
        private readonly Mock<IDbCommand> _commandMock = new();
        private readonly Mock<IDataReader> _readerMock = new();
        private readonly Mock<IDbDataParameter> _parameterMock = new();
        private readonly Mock<IDataParameterCollection> _parametersCollectionMock = new();
        private readonly Mock<ILogger<TriggerService>> _loggerMock = TestHelper.CreateLoggerMock<TriggerService>();
        private readonly TriggerService _service;

        public TriggerServiceTests()
        {
            SetupBasicMocks();
            _service = new TriggerService(_connectionFactoryMock.Object, _loggerMock.Object);
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
        public async Task GetTriggersMetadatByNamesAsync_WithValidTriggerNames_ReturnsTriggerMetadata()
        {
            // Arrange
            List<string> triggerNames = ["EMP_INSERT_TRIGGER", "EMP_UPDATE_TRIGGER"];
            List<TriggerMetadata> triggerData =
            [
                new TriggerMetadata
                {
                    TriggerName = "EMP_INSERT_TRIGGER",
                    TriggerType = "BEFORE",
                    TriggeringEvent = "INSERT",
                    TableName = "EMPLOYEES",
                    Description = "Before insert trigger"
                },
                new TriggerMetadata
                {
                    TriggerName = "EMP_UPDATE_TRIGGER",
                    TriggerType = "AFTER",
                    TriggeringEvent = "UPDATE",
                    TableName = "EMPLOYEES",
                    Description = "After update trigger"
                }
            ];

            SetupReaderForTriggerMetadata(triggerData);

            // Act
            List<TriggerMetadata> result = await _service.GetTriggersMetadatByNamesAsync(triggerNames);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("EMP_INSERT_TRIGGER", result[0].TriggerName);
            Assert.Equal("BEFORE", result[0].TriggerType);
            Assert.Equal("INSERT", result[0].TriggeringEvent);
            Assert.Equal("EMPLOYEES", result[0].TableName);
            Assert.Equal("Before insert trigger", result[0].Description);
        }

        [Fact]
        public async Task GetTriggersMetadatByNamesAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            List<string> triggerNames = [];

            // Act
            List<TriggerMetadata> result = await _service.GetTriggersMetadatByNamesAsync(triggerNames);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTriggersMetadatByNamesAsync_WithNullList_ReturnsEmptyList()
        {
            // Act
            List<TriggerMetadata> result = await _service.GetTriggersMetadatByNamesAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTriggersMetadatByNamesAsync_WithSingleTriggerName_ReturnsSingleTrigger()
        {
            // Arrange
            List<string> triggerNames = ["SINGLE_TRIGGER"];
            List<TriggerMetadata> triggerData =
            [
                new TriggerMetadata
                {
                    TriggerName = "SINGLE_TRIGGER",
                    TriggerType = "BEFORE",
                    TriggeringEvent = "DELETE",
                    TableName = "TEST_TABLE",
                    Description = "Test trigger"
                }
            ];

            SetupReaderForTriggerMetadata(triggerData);

            // Act
            List<TriggerMetadata> result = await _service.GetTriggersMetadatByNamesAsync(triggerNames);

            // Assert
            Assert.NotNull(result);
            _ = Assert.Single(result);
            Assert.Equal("SINGLE_TRIGGER", result[0].TriggerName);
            Assert.Equal("BEFORE", result[0].TriggerType);
            Assert.Equal("DELETE", result[0].TriggeringEvent);
        }

        [Fact]
        public async Task GetTriggersMetadatByNamesAsync_WhenDatabaseThrowsException_PropagatesException()
        {
            // Arrange
            List<string> triggerNames = ["INVALID_TRIGGER"];
            InvalidOperationException expectedException = new("Database error");
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ThrowsAsync(expectedException);

            // Act & Assert
            InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetTriggersMetadatByNamesAsync(triggerNames));
            Assert.Equal(expectedException, actualException);
        }

        [Fact]
        public async Task GetTriggersMetadatByNamesAsync_WithNoMatchingTriggers_ReturnsEmptyList()
        {
            // Arrange
            List<string> triggerNames = ["NONEXISTENT_TRIGGER"];
            SetupReaderForTriggerMetadata([]); // No data

            // Act
            List<TriggerMetadata> result = await _service.GetTriggersMetadatByNamesAsync(triggerNames);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new TriggerService(null!, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }

        [Fact]
        public async Task GetTriggersMetadatByNamesAsync_GeneratesCorrectSqlQuery()
        {
            // Arrange
            List<string> triggerNames = ["TRIGGER1", "TRIGGER2"];
            SetupReaderForTriggerMetadata([]);

            // Act
            _ = await _service.GetTriggersMetadatByNamesAsync(triggerNames);

            // Assert
            _commandMock.VerifySet(c => c.CommandText = It.Is<string>(sql =>
                sql.Contains("USER_TRIGGERS") &&
                sql.Contains("TRIGGER_NAME") &&
                sql.Contains(":p0") &&
                sql.Contains(":p1")), Times.Once);

            // Verify parameters were set
            _parameterMock.VerifySet(p => p.ParameterName = "p0", Times.Once);
            _parameterMock.VerifySet(p => p.ParameterName = "p1", Times.Once);
        }

        [Fact]
        public async Task GetTriggersMetadatByNamesAsync_HandlesNullDescription()
        {
            // Arrange
            List<string> triggerNames = ["TEST_TRIGGER"];
            List<TriggerMetadata> triggerData =
            [
                new TriggerMetadata
                {
                    TriggerName = "TEST_TRIGGER",
                    TriggerType = "BEFORE",
                    TriggeringEvent = "INSERT",
                    TableName = "TEST_TABLE",
                    Description = null // Null description
                }
            ];

            SetupReaderForTriggerMetadata(triggerData);

            // Act
            List<TriggerMetadata> result = await _service.GetTriggersMetadatByNamesAsync(triggerNames);

            // Assert
            Assert.NotNull(result);
            _ = Assert.Single(result);
            Assert.Null(result[0].Description);
        }

        private void SetupReaderForTriggerMetadata(List<TriggerMetadata> data)
        {
            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);

            if (data.Count > 0)
            {
                _ = _readerMock.Setup(r => r["TRIGGER_NAME"]).Returns(() => data[callCount].TriggerName);
                _ = _readerMock.Setup(r => r["TRIGGER_TYPE"]).Returns(() => data[callCount].TriggerType);
                _ = _readerMock.Setup(r => r["TRIGGERING_EVENT"]).Returns(() => data[callCount].TriggeringEvent);
                _ = _readerMock.Setup(r => r["TABLE_NAME"]).Returns(() => data[callCount].TableName);
                _ = _readerMock.Setup(r => r["DESCRIPTION"]).Returns(() => data[callCount].Description);
            }
        }
    }
}

