using System.Data;
using DatabaseMcp.Core.Models;
using DatabaseMcp.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DatabaseMcp.Core.Tests
{
    public class ObjectRelationshipServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
        private readonly Mock<IDbConnection> _connectionMock;
        private readonly Mock<IDbCommand> _commandMock;
        private readonly Mock<IDataReader> _readerMock;
        private readonly Mock<ILogger<ObjectRelationshipService>> _loggerMock;
        private readonly ObjectRelationshipService _service;

        public ObjectRelationshipServiceTests()
        {
            _connectionFactoryMock = new Mock<IDbConnectionFactory>();
            _connectionMock = new Mock<IDbConnection>();
            _commandMock = new Mock<IDbCommand>();
            _readerMock = new Mock<IDataReader>();
            _loggerMock = TestHelper.CreateLoggerMock<ObjectRelationshipService>();
            _service = new ObjectRelationshipService(_connectionFactoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetReferenceObjects_ReturnsList()
        {
            // Arrange
            string objectName = "EMPLOYEES";
            string objectType = "TABLE";
            List<ObjectRelationshipMetadata> data =
            [
                new ObjectRelationshipMetadata {
                    ObjectName = "EMP_SALARY_VIEW",
                    ObjectType = "VIEW"
                }
            ];

            // Setup parameter mocks
            Mock<IDbDataParameter> paramNameMock = new();
            _ = paramNameMock.SetupProperty(p => p.ParameterName);
            _ = paramNameMock.SetupProperty(p => p.Value);

            Mock<IDbDataParameter> paramTypeMock = new();
            _ = paramTypeMock.SetupProperty(p => p.ParameterName);
            _ = paramTypeMock.SetupProperty(p => p.Value);

            // Setup reader
            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            _ = _readerMock.Setup(r => r["OBJECT_NAME"]).Returns(() => data[callCount].ObjectName);
            _ = _readerMock.Setup(r => r["OBJECT_TYPE"]).Returns(() => data[callCount].ObjectType);

            // Setup command to return different parameters for each call
            int paramCounter = 0;
            _ = _commandMock.Setup(c => c.CreateParameter()).Returns(() =>
            {
                paramCounter++;
                return paramCounter == 1 ? paramNameMock.Object : paramTypeMock.Object;
            });

            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _ = _commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<ObjectRelationshipMetadata> result = await _service.GetReferenceObjects(objectName, objectType);

            // Assert
            Assert.NotNull(result);
            _ = Assert.Single(result);
            Assert.Equal("EMP_SALARY_VIEW", result[0].ObjectName);
            Assert.Equal("VIEW", result[0].ObjectType);

            // Verify the parameters were set correctly
            paramNameMock.VerifySet(p => p.ParameterName = "objectName");
            paramNameMock.VerifySet(p => p.Value = objectName.ToUpper());
            paramTypeMock.VerifySet(p => p.ParameterName = "objectType");
            paramTypeMock.VerifySet(p => p.Value = objectType.ToUpper());
        }

        [Fact]
        public async Task GetReferenceObjects_ThrowsException_WhenDbFails()
        {
            // Arrange
            string objectName = "ERROR_OBJECT";
            string objectType = "TABLE";
            InvalidOperationException expectedException = new("Test exception");

            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetReferenceObjects(objectName, objectType));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetReferenceObjects_ThrowsArgumentException_WhenObjectNameIsNull()
        {
            // Arrange
            string? objectName = null;
            string objectType = "TABLE";

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetReferenceObjects(objectName, objectType));
            Assert.Contains("object name cannot be null or empty", exception.Message);
        }

        [Fact]
        public async Task GetReferenceObjects_ThrowsArgumentException_WhenObjectTypeIsNull()
        {
            // Arrange
            string objectName = "EMPLOYEES";
            string? objectType = null;

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetReferenceObjects(objectName, objectType));
            Assert.Contains("object type cannot be null or empty", exception.Message);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new ObjectRelationshipService(null, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }

        [Fact]
        public async Task GetReferenceObjects_HandlesEmptyResult()
        {
            // Arrange
            string objectName = "UNUSED_OBJECT";
            string objectType = "TABLE";

            // Setup parameter mocks
            Mock<IDbDataParameter> paramNameMock = new();
            _ = paramNameMock.SetupProperty(p => p.ParameterName);
            _ = paramNameMock.SetupProperty(p => p.Value);

            Mock<IDbDataParameter> paramTypeMock = new();
            _ = paramTypeMock.SetupProperty(p => p.ParameterName);
            _ = paramTypeMock.SetupProperty(p => p.Value);

            // Setup reader to return no rows
            _ = _readerMock.Setup(r => r.Read()).Returns(false);

            // Setup command to return different parameters for each call
            int paramCounter = 0;
            _ = _commandMock.Setup(c => c.CreateParameter()).Returns(() =>
            {
                paramCounter++;
                return paramCounter == 1 ? paramNameMock.Object : paramTypeMock.Object;
            });

            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _ = _commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<ObjectRelationshipMetadata> result = await _service.GetReferenceObjects(objectName, objectType);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            // Verify the parameters were set correctly
            paramNameMock.VerifySet(p => p.ParameterName = "objectName");
            paramNameMock.VerifySet(p => p.Value = objectName.ToUpper());
            paramTypeMock.VerifySet(p => p.ParameterName = "objectType");
            paramTypeMock.VerifySet(p => p.Value = objectType.ToUpper());
        }

        [Fact]
        public async Task GetReferenceObjects_HandlesMultipleResults()
        {
            // Arrange
            string objectName = "POPULAR_TABLE";
            string objectType = "TABLE";
            List<ObjectRelationshipMetadata> data =
            [
                new ObjectRelationshipMetadata { ObjectName = "VIEW1", ObjectType = "VIEW" },
                new ObjectRelationshipMetadata { ObjectName = "PROC1", ObjectType = "PROCEDURE" },
                new ObjectRelationshipMetadata { ObjectName = "VIEW2", ObjectType = "VIEW" }
            ];

            // Setup parameter mocks
            Mock<IDbDataParameter> paramNameMock = new();
            _ = paramNameMock.SetupProperty(p => p.ParameterName);
            _ = paramNameMock.SetupProperty(p => p.Value);

            Mock<IDbDataParameter> paramTypeMock = new();
            _ = paramTypeMock.SetupProperty(p => p.ParameterName);
            _ = paramTypeMock.SetupProperty(p => p.Value);

            // Setup reader
            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            _ = _readerMock.Setup(r => r["OBJECT_NAME"]).Returns(() => data[callCount].ObjectName);
            _ = _readerMock.Setup(r => r["OBJECT_TYPE"]).Returns(() => data[callCount].ObjectType);

            // Setup command to return different parameters for each call
            int paramCounter = 0;
            _ = _commandMock.Setup(c => c.CreateParameter()).Returns(() =>
            {
                paramCounter++;
                return paramCounter == 1 ? paramNameMock.Object : paramTypeMock.Object;
            });

            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _ = _commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<ObjectRelationshipMetadata> result = await _service.GetReferenceObjects(objectName, objectType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("VIEW1", result[0].ObjectName);
            Assert.Equal("PROC1", result[1].ObjectName);
            Assert.Equal("VIEW2", result[2].ObjectName);
            Assert.Contains(result, r => r.ObjectType == "VIEW");
            Assert.Contains(result, r => r.ObjectType == "PROCEDURE");

            // Verify the parameters were set correctly
            paramNameMock.VerifySet(p => p.ParameterName = "objectName");
            paramNameMock.VerifySet(p => p.Value = objectName.ToUpper());
            paramTypeMock.VerifySet(p => p.ParameterName = "objectType");
            paramTypeMock.VerifySet(p => p.Value = objectType.ToUpper());
        }
    }
}
