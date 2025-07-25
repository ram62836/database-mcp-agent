using DatabaseMcp.Core.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace DatabaseMcp.Core.Tests
{
    [Collection("Database Tests")]
    public class DatabaseConnectionServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly DatabaseConnectionService _service;

        public DatabaseConnectionServiceTests()
        {
            _service = new DatabaseConnectionService(_configurationMock.Object);
        }

        [Fact]
        public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
        {
            // Act & Assert
            _ = Assert.Throws<ArgumentNullException>(() => new DatabaseConnectionService(null));
        }

        [Fact]
        public void GetOracleConnectionString_WithValidConnectionString_ReturnsConnectionString()
        {
            // Arrange
            const string expectedConnectionString = "Data Source=localhost:1521/XE;User Id=testuser;Password=testpass;";
            _ = _configurationMock.Setup(c => c["OracleConnectionString"]).Returns(expectedConnectionString);

            // Act
            string result = _service.GetOracleConnectionString();

            // Assert
            Assert.Equal(expectedConnectionString, result);
        }

        [Fact]
        public void GetOracleConnectionString_WithNullConnectionString_ThrowsInvalidOperationException()
        {
            // Arrange
            _ = _configurationMock.Setup(c => c["OracleConnectionString"]).Returns((string?)null);

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(_service.GetOracleConnectionString);
            Assert.Contains("No Oracle connection configuration found", exception.Message);
        }

        [Fact]
        public void GetOracleConnectionString_WithEmptyConnectionString_ThrowsInvalidOperationException()
        {
            // Arrange
            _ = _configurationMock.Setup(c => c["OracleConnectionString"]).Returns(string.Empty);

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(_service.GetOracleConnectionString);
            Assert.Contains("No Oracle connection configuration found", exception.Message);
        }

        [Fact]
        public void GetOracleConnectionString_WithWhitespaceConnectionString_ThrowsInvalidOperationException()
        {
            // Arrange
            _ = _configurationMock.Setup(c => c["OracleConnectionString"]).Returns("   ");

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(_service.GetOracleConnectionString);
            Assert.Contains("No Oracle connection configuration found", exception.Message);
        }
    }
}