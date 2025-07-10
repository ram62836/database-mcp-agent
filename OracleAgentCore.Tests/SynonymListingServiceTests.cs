using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OracleAgent.Core;
using OracleAgent.Core.Interfaces;
using OracleAgent.Core.Models;
using OracleAgent.Core.Services;
using Xunit;

namespace OracleAgentCore.Tests
{
    public class SynonymListingServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
        private readonly Mock<IDbConnection> _connectionMock;
        private readonly Mock<IDbCommand> _commandMock;
        private readonly Mock<IDataReader> _readerMock;
        private readonly Mock<ILogger<SynonymListingService>> _loggerMock;
        private readonly SynonymListingService _service;

        public SynonymListingServiceTests()
        {
            _connectionFactoryMock = new Mock<IDbConnectionFactory>();
            _connectionMock = new Mock<IDbConnection>();
            _commandMock = new Mock<IDbCommand>();
            _readerMock = new Mock<IDataReader>();
            _loggerMock = TestHelper.CreateLoggerMock<SynonymListingService>();
            _service = new SynonymListingService(_connectionFactoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ListSynonymsAsync_ReturnsList()
        {
            // Arrange
            var data = new List<SynonymMetadata> { 
                new SynonymMetadata { 
                    SynonymName = "SYN1", 
                    TableOwner = "OWNER", 
                    BaseObjectName = "TABLE1" 
                } 
            };
            
            SetupReaderForSynonymMetadata(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.ListSynonymsAsync();
            
            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("SYN1", result[0].SynonymName);
            Assert.Equal("OWNER", result[0].TableOwner);
            Assert.Equal("TABLE1", result[0].BaseObjectName);
            
            // Verify the command was set up correctly
            _commandMock.VerifySet(c => c.CommandText = It.IsAny<string>());
        }
        
        [Fact]
        public async Task ListSynonymsAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Test exception");
            
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ListSynonymsAsync());
            Assert.Same(expectedException, exception);
        }
        
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new SynonymListingService(null, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }
        
        [Fact]
        public async Task ListSynonymsAsync_HandlesEmptyResult()
        {
            // Arrange
            var data = new List<SynonymMetadata>();
            
            _readerMock.Setup(r => r.Read()).Returns(false); // No rows
            SetupMocksForCommand(_commandMock, _readerMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.ListSynonymsAsync();
            
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        
        [Fact]
        public async Task ListSynonymsAsync_HandlesMultipleResults()
        {
            // Arrange
            var data = new List<SynonymMetadata> { 
                new SynonymMetadata { SynonymName = "SYN1", TableOwner = "OWNER1", BaseObjectName = "TABLE1" },
                new SynonymMetadata { SynonymName = "SYN2", TableOwner = "OWNER2", BaseObjectName = "TABLE2" },
                new SynonymMetadata { SynonymName = "SYN3", TableOwner = "OWNER1", BaseObjectName = "TABLE3" }
            };
            
            SetupReaderForSynonymMetadata(_readerMock, data);
            SetupMocksForCommand(_commandMock, _readerMock);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.ListSynonymsAsync();
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("SYN1", result[0].SynonymName);
            Assert.Equal("SYN2", result[1].SynonymName);
            Assert.Equal("SYN3", result[2].SynonymName);
            Assert.Contains(result, s => s.TableOwner == "OWNER1");
            Assert.Contains(result, s => s.TableOwner == "OWNER2");
            Assert.Contains(result, s => s.BaseObjectName == "TABLE1");
            Assert.Contains(result, s => s.BaseObjectName == "TABLE2");
            Assert.Contains(result, s => s.BaseObjectName == "TABLE3");
        }
        
        private void SetupReaderForSynonymMetadata(Mock<IDataReader> readerMock, List<SynonymMetadata> data)
        {
            int callCount = -1;
            readerMock.Setup(r => r.Read()).Returns(() => ++callCount < data.Count);
            
            if (data.Count > 0)
            {
                readerMock.Setup(r => r["SYNONYM_NAME"]).Returns(() => data[callCount].SynonymName);
                readerMock.Setup(r => r["TABLE_OWNER"]).Returns(() => data[callCount].TableOwner);
                readerMock.Setup(r => r["TABLE_NAME"]).Returns(() => data[callCount].BaseObjectName);
            }
        }
        
        private void SetupMocksForCommand(Mock<IDbCommand> commandMock, Mock<IDataReader> readerMock)
        {
            commandMock.Setup(c => c.ExecuteReader()).Returns(readerMock.Object);
            commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
        }
    }
}
