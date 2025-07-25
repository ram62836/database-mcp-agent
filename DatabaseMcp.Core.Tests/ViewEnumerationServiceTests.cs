using System.Data;
using DatabaseMcp.Core.Models;
using DatabaseMcp.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DatabaseMcp.Core.Tests
{
    [Collection("Database Tests")]
    public class ViewEnumerationServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock = new();
        private readonly Mock<IDbConnection> _connectionMock = new();
        private readonly Mock<IDbCommand> _commandMock = new();
        private readonly Mock<IDbDataParameter> _parameterMock = new();
        private readonly Mock<IDataParameterCollection> _parametersCollectionMock = new();
        private readonly Mock<ILogger<ViewEnumerationService>> _loggerMock = TestHelper.CreateLoggerMock<ViewEnumerationService>();
        private readonly ViewEnumerationService _service;

        public ViewEnumerationServiceTests()
        {
            SetupBasicMocks();
            _service = new ViewEnumerationService(_connectionFactoryMock.Object, _loggerMock.Object);
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
            _ = _commandMock.Setup(c => c.ExecuteScalar()).Returns("CREATE VIEW TEST_VIEW AS SELECT * FROM DUAL");

            // Setup connection mock
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);

            // Setup connection factory mock
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);
        }

        [Fact]
        public async Task GetViewsDefinitionAsync_WithValidViewNames_ReturnsViewMetadata()
        {
            // Arrange
            List<string> viewNames = ["EMP_VIEW", "DEPT_VIEW"];
            const string expectedDdl = "CREATE VIEW EMP_VIEW AS SELECT * FROM EMPLOYEES";
            _ = _commandMock.Setup(c => c.ExecuteScalar()).Returns(expectedDdl);

            // Act
            List<ViewMetadata> result = await _service.GetViewsDefinitionByNamesAsync(viewNames);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, view => Assert.Equal(expectedDdl, view.Definition));
        }

        [Fact]
        public async Task GetViewsDefinitionAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            List<string> viewNames = [];

            // Act
            List<ViewMetadata> result = await _service.GetViewsDefinitionByNamesAsync(viewNames);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetViewsDefinitionAsync_WithSingleViewName_ReturnsSingleViewMetadata()
        {
            // Arrange
            List<string> viewNames = ["EMP_VIEW"];
            const string expectedDdl = "CREATE VIEW EMP_VIEW AS SELECT * FROM EMPLOYEES";
            _ = _commandMock.Setup(c => c.ExecuteScalar()).Returns(expectedDdl);

            // Act
            List<ViewMetadata> result = await _service.GetViewsDefinitionByNamesAsync(viewNames);

            // Assert
            Assert.NotNull(result);
            _ = Assert.Single(result);
            Assert.Equal(expectedDdl, result[0].Definition);
        }

        [Fact]
        public async Task GetViewsDefinitionAsync_WhenDatabaseThrowsException_PropagatesException()
        {
            // Arrange
            List<string> viewNames = ["INVALID_VIEW"];
            _ = _commandMock.Setup(c => c.ExecuteScalar()).Throws(new InvalidOperationException("View not found"));

            // Act & Assert
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetViewsDefinitionByNamesAsync(viewNames));
        }

        [Fact]
        public async Task GetViewsDefinitionAsync_WithNullScalarResult_ReturnsEmptyDefinition()
        {
            // Arrange
            List<string> viewNames = ["EMP_VIEW"];
            _ = _commandMock.Setup(c => c.ExecuteScalar()).Returns((object?)null);

            // Act
            List<ViewMetadata> result = await _service.GetViewsDefinitionByNamesAsync(viewNames);

            // Assert
            Assert.NotNull(result);
            _ = Assert.Single(result);
            Assert.Equal(string.Empty, result[0].Definition);
        }

        [Fact]
        public async Task GetViewsDefinitionAsync_WithEmptyViewName_ThrowsArgumentException()
        {
            // Arrange
            List<string> viewNames = [""];

            // Act & Assert
            _ = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetViewsDefinitionByNamesAsync(viewNames));
        }

        [Fact]
        public async Task GetViewsDefinitionAsync_WithNullViewName_ThrowsArgumentException()
        {
            // Arrange
            List<string> viewNames = [null!];

            // Act & Assert
            _ = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetViewsDefinitionByNamesAsync(viewNames));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new ViewEnumerationService(null!, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }

        [Fact]
        public async Task GetViewsDefinitionAsync_CallsCorrectSqlCommand()
        {
            // Arrange
            List<string> viewNames = ["TEST_VIEW"];
            const string expectedSql = "SELECT DBMS_METADATA.GET_DDL('VIEW', :ViewName) AS DDL FROM DUAL";

            // Act
            _ = await _service.GetViewsDefinitionByNamesAsync(viewNames);

            // Assert
            _commandMock.VerifySet(c => c.CommandText = expectedSql, Times.Once);
            _parameterMock.VerifySet(p => p.ParameterName = "ViewName", Times.Once);
            _parameterMock.VerifySet(p => p.Value = "TEST_VIEW", Times.Once);
        }
    }
}

