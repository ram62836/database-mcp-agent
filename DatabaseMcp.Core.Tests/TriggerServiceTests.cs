using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using DatabaseMcp.Core;
using DatabaseMcp.Core.Models;
using DatabaseMcp.Core.Services;
using Xunit;

namespace DatabaseMcp.Core.Tests
{
    public class TriggerServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
        private readonly Mock<IDbConnection> _connectionMock;
        private readonly Mock<IDbCommand> _commandMock;
        private readonly Mock<IDataReader> _readerMock;
        private readonly Mock<ILogger<TriggerService>> _loggerMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly TriggerService _service;
        private readonly string _metadataFilePath;

        public TriggerServiceTests()
        {
            _connectionFactoryMock = new Mock<IDbConnectionFactory>();
            _connectionMock = new Mock<IDbConnection>();
            _commandMock = new Mock<IDbCommand>();
            _readerMock = new Mock<IDataReader>();
            _loggerMock = TestHelper.CreateLoggerMock<TriggerService>();
            _configMock = new Mock<IConfiguration>();
            
            // Setup configuration to return the test directory for metadata files
            _configMock.Setup(c => c["MetadataJsonPath"]).Returns(Directory.GetCurrentDirectory());
            _metadataFilePath = Path.Combine(Directory.GetCurrentDirectory(), "triggers_metadata.json");
            
            _service = new TriggerService(_connectionFactoryMock.Object, _configMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllTriggersAsync_ReturnsList()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(_metadataFilePath))
            {
                File.Delete(_metadataFilePath);
            }

            List<TriggerMetadata> data = new()
            {
                new TriggerMetadata {
                    TriggerName = "TR1",
                    TriggerType = "BEFORE",
                    TriggeringEvent = "INSERT",
                    TableName = "T1",
                    Description = "desc1"
                },
                new TriggerMetadata {
                    TriggerName = "TR2",
                    TriggerType = "AFTER",
                    TriggeringEvent = "UPDATE",
                    TableName = "T2",
                    Description = "desc2"
                }
            };

            SetupReaderForTriggerMetadata(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<TriggerMetadata> result = await _service.GetAllTriggersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data.Count, result.Count);
            Assert.Equal("TR1", result[0].TriggerName);
            Assert.Equal("BEFORE", result[0].TriggerType);
            Assert.Equal("INSERT", result[0].TriggeringEvent);
            Assert.Equal("T1", result[0].TableName);
            Assert.Equal("desc1", result[0].Description);

            // Verify the command was set up correctly
            _commandMock.VerifySet(c => c.CommandText = It.IsAny<string>());

            // Verify the cache file was created
            Assert.True(File.Exists(_metadataFilePath));

            // Clean up
            if (File.Exists(_metadataFilePath))
            {
                File.Delete(_metadataFilePath);
            }
        }

        [Fact]
        public async Task GetAllTriggersAsync_UsesCache_WhenCacheFileExists()
        {
            // Arrange
            List<TriggerMetadata> cachedTriggers = new()
            {
                new TriggerMetadata {
                    TriggerName = "CACHED_TRIGGER",
                    TriggerType = "BEFORE",
                    TriggeringEvent = "DELETE",
                    TableName = "CACHED_TABLE",
                    Description = "cached description"
                }
            };

            // Create cache directory if it doesn't exist
            _ = Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file with the new path
            await File.WriteAllTextAsync(
                _metadataFilePath,
                JsonSerializer.Serialize(cachedTriggers));

            try
            {
                // Act
                List<TriggerMetadata> result = await _service.GetAllTriggersAsync();

                // Assert
                Assert.NotNull(result);
                _ = Assert.Single(result);
                Assert.Equal("CACHED_TRIGGER", result[0].TriggerName);
                Assert.Equal("BEFORE", result[0].TriggerType);
                Assert.Equal("DELETE", result[0].TriggeringEvent);
                Assert.Equal("CACHED_TABLE", result[0].TableName);
                Assert.Equal("cached description", result[0].Description);

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
        public async Task GetTriggersByNameAsync_FiltersCorrectly()
        {
            // Arrange
            // Create cache file with test data
            List<TriggerMetadata> cachedTriggers = new()
            {
                new TriggerMetadata { TriggerName = "EMP_INSERT_TRIGGER", TriggerType = "BEFORE", TriggeringEvent = "INSERT" },
                new TriggerMetadata { TriggerName = "EMP_UPDATE_TRIGGER", TriggerType = "AFTER", TriggeringEvent = "UPDATE" },
                new TriggerMetadata { TriggerName = "CUST_INSERT_TRIGGER", TriggerType = "BEFORE", TriggeringEvent = "INSERT" }
            };

            // Create cache directory if it doesn't exist
            _ = Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                _metadataFilePath,
                JsonSerializer.Serialize(cachedTriggers));

            try
            {
                // Names to filter by
                List<string> triggerNames = new()
                { "EMP" };

                // Act
                List<TriggerMetadata> result = await _service.GetTriggersByNameAsync(triggerNames);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Count);
                Assert.Contains(result, t => t.TriggerName == "EMP_INSERT_TRIGGER");
                Assert.Contains(result, t => t.TriggerName == "EMP_UPDATE_TRIGGER");
                Assert.DoesNotContain(result, t => t.TriggerName == "CUST_INSERT_TRIGGER");
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
        public async Task GetAllTriggersAsync_ThrowsException_WhenDbFails()
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
                _service.GetAllTriggersAsync);
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            var configMock = new Mock<IConfiguration>();
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new TriggerService(null, configMock.Object, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }

        [Fact]
        public async Task GetAllTriggersAsync_HandlesEmptyResult()
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
                List<TriggerMetadata> result = await _service.GetAllTriggersAsync();

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

        private void SetupReaderForTriggerMetadata(Mock<IDataReader> readerMock, List<TriggerMetadata> data)
        {
            int callCount = -1;
            _ = readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);

            if (data.Count > 0)
            {
                _ = readerMock.Setup(r => r["TRIGGER_NAME"]).Returns(() => data[callCount].TriggerName);
                _ = readerMock.Setup(r => r["TRIGGER_TYPE"]).Returns(() => data[callCount].TriggerType);
                _ = readerMock.Setup(r => r["TRIGGERING_EVENT"]).Returns(() => data[callCount].TriggeringEvent);
                _ = readerMock.Setup(r => r["TABLE_NAME"]).Returns(() => data[callCount].TableName);
                _ = readerMock.Setup(r => r["DESCRIPTION"]).Returns(() => data[callCount].Description);
            }
        }

        private void SetupMocksForCommand(Mock<IDbCommand> commandMock, Mock<IDataReader> readerMock)
        {
            _ = commandMock.Setup(c => c.ExecuteReader()).Returns(readerMock.Object);
            _ = commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
        }
    }
}
