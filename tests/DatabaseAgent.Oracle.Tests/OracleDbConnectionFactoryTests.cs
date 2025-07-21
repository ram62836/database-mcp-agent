using Hala.DatabaseAgent.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hala.DatabaseAgent.Oracle.Tests
{
    public class OracleDbConnectionFactoryTests
    {
        [Fact]
        public void Constructor_WithValidSettings_ShouldNotThrow()
        {
            // Arrange
            var settings = new ConnectionSettings
            {
                Provider = "Oracle",
                ConnectionString = "valid_connection_string",
                DefaultSchema = "SYSTEM"
            };
            var loggerMock = new Mock<ILogger<OracleDbConnectionFactory>>();

            // Act & Assert
            var exception = Record.Exception(() => new OracleDbConnectionFactory(settings, loggerMock.Object));
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_WithNonOracleProvider_ShouldThrowArgumentException()
        {
            // Arrange
            var settings = new ConnectionSettings
            {
                Provider = "SqlServer", // Not Oracle
                ConnectionString = "valid_connection_string",
                DefaultSchema = "SYSTEM"
            };
            var loggerMock = new Mock<ILogger<OracleDbConnectionFactory>>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new OracleDbConnectionFactory(settings, loggerMock.Object));
            Assert.Contains("Connection provider must be Oracle", exception.Message);
        }

        [Fact]
        public void GetProviderName_ShouldReturnOracle()
        {
            // Arrange
            var settings = new ConnectionSettings
            {
                Provider = "Oracle",
                ConnectionString = "valid_connection_string",
                DefaultSchema = "SYSTEM"
            };
            var loggerMock = new Mock<ILogger<OracleDbConnectionFactory>>();
            var factory = new OracleDbConnectionFactory(settings, loggerMock.Object);

            // Act
            var providerName = factory.GetProviderName();

            // Assert
            Assert.Equal("Oracle", providerName);
        }

        [Fact]
        public void GetDefaultSchema_ShouldReturnConfiguredSchema()
        {
            // Arrange
            var expectedSchema = "TEST_SCHEMA";
            var settings = new ConnectionSettings
            {
                Provider = "Oracle",
                ConnectionString = "valid_connection_string",
                DefaultSchema = expectedSchema
            };
            var loggerMock = new Mock<ILogger<OracleDbConnectionFactory>>();
            var factory = new OracleDbConnectionFactory(settings, loggerMock.Object);

            // Act
            var schema = factory.GetDefaultSchema();

            // Assert
            Assert.Equal(expectedSchema, schema);
        }
    }
}
