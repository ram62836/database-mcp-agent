using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OracleAgent.Core;
using OracleAgent.Core.Models;
using OracleAgent.Core.Services;
using Xunit;

namespace OracleAgentCore.Tests
{
    public class ViewEnumerationServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
        private readonly Mock<IDbConnection> _connectionMock;
        private readonly Mock<IDbCommand> _commandMock;
        private readonly Mock<IDataReader> _readerMock;
        private readonly Mock<ILogger<ViewEnumerationService>> _loggerMock;
        private readonly ViewEnumerationService _service;

        public ViewEnumerationServiceTests()
        {
            _connectionFactoryMock = new Mock<IDbConnectionFactory>();
            _connectionMock = new Mock<IDbConnection>();
            _commandMock = new Mock<IDbCommand>();
            _readerMock = new Mock<IDataReader>();
            _loggerMock = TestHelper.CreateLoggerMock<ViewEnumerationService>();
            _service = new ViewEnumerationService(_connectionFactoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllViewsAsync_ReturnsList()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(AppConstants.ViewsMetadatJsonFile))
                File.Delete(AppConstants.ViewsMetadatJsonFile);

            var data = new List<ViewMetadata> {
                new ViewMetadata { ViewName = "V1", Definition = "DEF1" },
                new ViewMetadata { ViewName = "V2", Definition = "DEF2" }
            };
            
            SetupReaderForViewMetadata(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            try
            {
                // Act
                var result = await _service.GetAllViewsAsync();

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
                Assert.True(File.Exists(AppConstants.ViewsMetadatJsonFile));
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.ViewsMetadatJsonFile))
                    File.Delete(AppConstants.ViewsMetadatJsonFile);
            }
        }
        
        [Fact]
        public async Task GetAllViewsAsync_UsesCache_WhenCacheFileExists()
        {
            // Arrange
            var cachedViews = new List<ViewMetadata> {
                new ViewMetadata { ViewName = "CACHED_VIEW", Definition = "CACHED DEFINITION" }
            };

            // Create cache directory if it doesn't exist
            Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                AppConstants.ViewsMetadatJsonFile, 
                JsonSerializer.Serialize(cachedViews));

            try
            {
                // Act
                var result = await _service.GetAllViewsAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Single(result);
                Assert.Equal("CACHED_VIEW", result[0].ViewName);
                Assert.Equal("CACHED DEFINITION", result[0].Definition);
                
                // Verify no database connection was made
                _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Never);
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.ViewsMetadatJsonFile))
                    File.Delete(AppConstants.ViewsMetadatJsonFile);
            }
        }
        
        [Fact]
        public async Task GetViewsDefinitionAsync_FiltersCorrectly()
        {
            // Arrange
            // Create cache file with test data
            var cachedViews = new List<ViewMetadata> {
                new ViewMetadata { ViewName = "EMP_VIEW", Definition = "DEF1" },
                new ViewMetadata { ViewName = "DEPT_VIEW", Definition = "DEF2" },
                new ViewMetadata { ViewName = "EMP_DEPT_VIEW", Definition = "DEF3" }
            };

            // Create cache directory if it doesn't exist
            Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                AppConstants.ViewsMetadatJsonFile, 
                JsonSerializer.Serialize(cachedViews));

            try
            {
                // Names to filter by
                var viewNames = new List<string> { "EMP" };
                
                // Act
                var result = await _service.GetViewsDefinitionAsync(viewNames);

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
                if (File.Exists(AppConstants.ViewsMetadatJsonFile))
                    File.Delete(AppConstants.ViewsMetadatJsonFile);
            }
        }
        
        [Fact]
        public async Task GetAllViewsAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(AppConstants.ViewsMetadatJsonFile))
                File.Delete(AppConstants.ViewsMetadatJsonFile);
                
            var expectedException = new InvalidOperationException("Test exception");
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetAllViewsAsync());
            Assert.Same(expectedException, exception);
        }
        
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ViewEnumerationService(null, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }
        
        [Fact]
        public async Task GetAllViewsAsync_HandlesEmptyResult()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(AppConstants.ViewsMetadatJsonFile))
                File.Delete(AppConstants.ViewsMetadatJsonFile);
            
            _readerMock.Setup(r => r.Read()).Returns(false); // No rows
            SetupMocksForCommand(_commandMock, _readerMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            try
            {
                // Act
                var result = await _service.GetAllViewsAsync();
                
                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.ViewsMetadatJsonFile))
                    File.Delete(AppConstants.ViewsMetadatJsonFile);
            }
        }
        
        [Fact]
        public async Task GetViewsDefinitionAsync_ReturnsEmptyList_WhenNoMatchesFound()
        {
            // Arrange
            // Create cache file with test data
            var cachedViews = new List<ViewMetadata> {
                new ViewMetadata { ViewName = "VIEW1", Definition = "DEF1" },
                new ViewMetadata { ViewName = "VIEW2", Definition = "DEF2" }
            };

            // Create cache directory if it doesn't exist
            Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                AppConstants.ViewsMetadatJsonFile, 
                JsonSerializer.Serialize(cachedViews));

            try
            {
                // Names to filter by
                var viewNames = new List<string> { "NONEXISTENT" };
                
                // Act
                var result = await _service.GetViewsDefinitionAsync(viewNames);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.ViewsMetadatJsonFile))
                    File.Delete(AppConstants.ViewsMetadatJsonFile);
            }
        }
        
        private void SetupReaderForViewMetadata(Mock<IDataReader> readerMock, List<ViewMetadata> data)
        {
            int callCount = -1;
            readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            
            if (data.Count > 0)
            {
                readerMock.Setup(r => r["VIEW_NAME"]).Returns(() => data[callCount].ViewName);
                readerMock.Setup(r => r["TEXT_VC"]).Returns(() => data[callCount].Definition);
            }
        }
        
        private void SetupMocksForCommand(Mock<IDbCommand> commandMock, Mock<IDataReader> readerMock)
        {
            commandMock.Setup(c => c.ExecuteReader()).Returns(readerMock.Object);
            commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
        }
    }
}
