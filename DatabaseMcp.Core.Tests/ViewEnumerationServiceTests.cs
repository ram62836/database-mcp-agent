using System.Data;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using DatabaseMcp.Core;
using DatabaseMcp.Core.Models;
using DatabaseMcp.Core.Services;

namespace DatabaseMcp.Core.Tests
{
    public class ViewEnumerationServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
        private readonly Mock<IDbConnection> _connectionMock;
        private readonly Mock<IDbCommand> _commandMock;
        private readonly Mock<IDataReader> _readerMock;
        private readonly Mock<ILogger<ViewEnumerationService>> _loggerMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly ViewEnumerationService _service;
        private readonly string _metadataFilePath;

        public ViewEnumerationServiceTests()
        {
            _connectionFactoryMock = new Mock<IDbConnectionFactory>();
            _connectionMock = new Mock<IDbConnection>();
            _commandMock = new Mock<IDbCommand>();
            _readerMock = new Mock<IDataReader>();
            _loggerMock = TestHelper.CreateLoggerMock<ViewEnumerationService>();
            _configMock = new Mock<IConfiguration>();
            
            // Setup configuration to return the test directory for metadata files
            _configMock.Setup(c => c["MetadataJsonPath"]).Returns(Directory.GetCurrentDirectory());
            _metadataFilePath = Path.Combine(Directory.GetCurrentDirectory(), "views_metadata.json");
            
            _service = new ViewEnumerationService(_connectionFactoryMock.Object, _configMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllViewsAsync_ReturnsList()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(_metadataFilePath))
            {
                File.Delete(_metadataFilePath);
            }

            List<ViewMetadata> data = new()
            {
                new ViewMetadata { ViewName = "V1", Definition = "DEF1" },
                new ViewMetadata { ViewName = "V2", Definition = "DEF2" }
            };

            SetupReaderForViewMetadata(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            try
            {
                // Act
                List<ViewMetadata> result = await _service.GetAllViewsAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(data.Count, result.Count);
                Assert.Equal("V1", result[0].ViewName);
                Assert.Equal("DEF1", result[0].Definition);
                Assert.Equal("V2", result[1].ViewName);
                Assert.Equal("DEF2", result[1].Definition);

                // Verify the command was set up correctly
                _commandMock.VerifySet(c => c.CommandText = It.IsAny<string>());

                // Verify the cache file was created
                Assert.True(File.Exists(_metadataFilePath));
            }
            finally
            {
                // Clean up
                if (File.Exists(_metadataFilePath))
                {
                    File.Delete(_metadataFilePath);
                }
            }
        }

        [Fact]
        public async Task GetAllViewsAsync_UsesCache_WhenCacheFileExists()
        {
            // Arrange
            List<ViewMetadata> cachedViews = new()
            {
                new ViewMetadata { ViewName = "CACHED_VIEW", Definition = "CACHED DEFINITION" }
            };

            // Create cache directory if it doesn't exist
            _ = Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                _metadataFilePath,
                JsonSerializer.Serialize(cachedViews));

            try
            {
                // Act
                List<ViewMetadata> result = await _service.GetAllViewsAsync();

                // Assert
                Assert.NotNull(result);
                _ = Assert.Single(result);
                Assert.Equal("CACHED_VIEW", result[0].ViewName);
                Assert.Equal("CACHED DEFINITION", result[0].Definition);

                // Verify no database connection was made
                _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Never);
            }
            finally
            {
                // Clean up
                if (File.Exists(_metadataFilePath))
                {
                    File.Delete(_metadataFilePath);
                }
            }
        }

        [Fact]
        public async Task GetViewsDefinitionAsync_FiltersCorrectly()
        {
            // Arrange
            // Create cache file with test data
            List<ViewMetadata> cachedViews = new()
            {
                new ViewMetadata { ViewName = "EMP_VIEW", Definition = "DEF1" },
                new ViewMetadata { ViewName = "DEPT_VIEW", Definition = "DEF2" },
                new ViewMetadata { ViewName = "EMP_DEPT_VIEW", Definition = "DEF3" }
            };

            // Create cache directory if it doesn't exist
            _ = Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                _metadataFilePath,
                JsonSerializer.Serialize(cachedViews));

            try
            {
                // Names to filter by
                List<string> viewNames = new()
                { "EMP" };

                // Act
                List<ViewMetadata> result = await _service.GetViewsDefinitionAsync(viewNames);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Count);
                Assert.Contains(result, v => v.ViewName == "EMP_VIEW");
                Assert.Contains(result, v => v.ViewName == "EMP_DEPT_VIEW");
                Assert.DoesNotContain(result, v => v.ViewName == "DEPT_VIEW");
            }
            finally
            {
                // Clean up
                if (File.Exists(_metadataFilePath))
                {
                    File.Delete(_metadataFilePath);
                }
            }
        }

        [Fact]
        public async Task GetAllViewsAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(_metadataFilePath))
            {
                File.Delete(_metadataFilePath);
            }

            InvalidOperationException expectedException = new("Test exception");
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                _service.GetAllViewsAsync);
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            var configMock = new Mock<IConfiguration>();
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new ViewEnumerationService(null, configMock.Object, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }

        [Fact]
        public async Task GetAllViewsAsync_HandlesEmptyResult()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(_metadataFilePath))
            {
                File.Delete(_metadataFilePath);
            }

            _ = _readerMock.Setup(r => r.Read()).Returns(false); // No rows
            SetupMocksForCommand(_commandMock, _readerMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            try
            {
                // Act
                List<ViewMetadata> result = await _service.GetAllViewsAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }
            finally
            {
                // Clean up
                if (File.Exists(_metadataFilePath))
                {
                    File.Delete(_metadataFilePath);
                }
            }
        }

        [Fact]
        public async Task GetViewsDefinitionAsync_ReturnsEmptyList_WhenNoMatchesFound()
        {
            // Arrange
            // Create cache file with test data
            List<ViewMetadata> cachedViews = new()
            {
                new ViewMetadata { ViewName = "VIEW1", Definition = "DEF1" },
                new ViewMetadata { ViewName = "VIEW2", Definition = "DEF2" }
            };

            // Create cache directory if it doesn't exist
            _ = Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                _metadataFilePath,
                JsonSerializer.Serialize(cachedViews));

            try
            {
                // Names to filter by
                List<string> viewNames = new()
                { "NONEXISTENT" };

                // Act
                List<ViewMetadata> result = await _service.GetViewsDefinitionAsync(viewNames);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }
            finally
            {
                // Clean up
                if (File.Exists(_metadataFilePath))
                {
                    File.Delete(_metadataFilePath);
                }
            }
        }

        private void SetupReaderForViewMetadata(Mock<IDataReader> readerMock, List<ViewMetadata> data)
        {
            int callCount = -1;
            _ = readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);

            if (data.Count > 0)
            {
                _ = readerMock.Setup(r => r["VIEW_NAME"]).Returns(() => data[callCount].ViewName);
                _ = readerMock.Setup(r => r["TEXT_VC"]).Returns(() => data[callCount].Definition);
            }
        }

        private void SetupMocksForCommand(Mock<IDbCommand> commandMock, Mock<IDataReader> readerMock)
        {
            _ = commandMock.Setup(c => c.ExecuteReader()).Returns(readerMock.Object);
            _ = commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
        }
    }
}
