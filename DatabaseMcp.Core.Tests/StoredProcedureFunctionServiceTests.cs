using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using DatabaseMcp.Core;
using DatabaseMcp.Core.Models;
using DatabaseMcp.Core.Services;
using Xunit;

namespace DatabaseMcp.Core.Tests
{
    public class StoredProcedureFunctionServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<ILogger<StoredProcedureFunctionService>> _loggerMock;
        private readonly Mock<IDbConnection> _connectionMock;
        private readonly Mock<IDbCommand> _commandMock;
        private readonly Mock<IDataReader> _readerMock;
        private readonly StoredProcedureFunctionService _service;

        public StoredProcedureFunctionServiceTests()
        {
            _connectionFactoryMock = new Mock<IDbConnectionFactory>();
            _cacheMock = new Mock<IMemoryCache>();
            _loggerMock = TestHelper.CreateLoggerMock<StoredProcedureFunctionService>();
            _connectionMock = new Mock<IDbConnection>();
            _commandMock = new Mock<IDbCommand>();
            _readerMock = new Mock<IDataReader>();
            
            _service = new StoredProcedureFunctionService(
                _connectionFactoryMock.Object, 
                _cacheMock.Object, 
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllStoredProceduresAsync_ReturnsList()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(AppConstants.ProceduresMetadatJsonFile))
                File.Delete(AppConstants.ProceduresMetadatJsonFile);
                
            var procNames = new List<string> { "PROC1", "PROC2" };
            
            // Setup the reader for the main query
            int callCount = -1;
            _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < procNames.Count);
            _readerMock.Setup(r => r["OBJECT_NAME"]).Returns(() => procNames[callCount]);
            _readerMock.Setup(r => r["OBJECT_TYPE"]).Returns("PROCEDURE");
            
            // Setup parameter mock for definition query
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            // Setup the command for both ExecuteReader and ExecuteScalar
            _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _commandMock.Setup(c => c.ExecuteScalar()).Returns("CREATE PROCEDURE PROC1 AS BEGIN NULL; END;");
            _commandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
            
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetAllStoredProceduresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(procNames.Count, result.Count);
            Assert.Contains(result, p => p.Name == "PROC1");
            Assert.Contains(result, p => p.Name == "PROC2");
            
            // Verify the command was set up correctly
            _commandMock.VerifySet(c => c.CommandText = It.IsAny<string>(), Times.AtLeastOnce());
            
            // Verify parameters were set correctly for the definition queries
            paramMock.VerifySet(p => p.ParameterName = "ProcedureName", Times.AtLeastOnce());
            paramMock.VerifySet(p => p.Value = It.IsAny<string>(), Times.AtLeastOnce());
        }
        
        [Fact]
        public async Task GetAllStoredProceduresAsync_UsesCache_WhenCacheFileExists()
        {
            // Arrange
            var cachedProcedures = new List<ProcedureFunctionMetadata> {
                new ProcedureFunctionMetadata { Name = "CACHED_PROC", Definition = "CACHED DEFINITION" }
            };
            
            // Create cache directory if it doesn't exist
            Directory.CreateDirectory(Directory.GetCurrentDirectory());
            
            // Create the cache file
            await File.WriteAllTextAsync(
                AppConstants.ProceduresMetadatJsonFile, 
                JsonSerializer.Serialize(cachedProcedures));

            try
            {
                // Act
                var result = await _service.GetAllStoredProceduresAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Single(result);
                Assert.Equal("CACHED_PROC", result[0].Name);
                Assert.Equal("CACHED DEFINITION", result[0].Definition);
                
                // Verify no database connection was made
                _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Never);
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.ProceduresMetadatJsonFile))
                    File.Delete(AppConstants.ProceduresMetadatJsonFile);
            }
        }
        
        [Fact]
        public async Task GetAllFunctionsAsync_ReturnsList()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(AppConstants.FunctionsMetadataJsonFile))
                File.Delete(AppConstants.FunctionsMetadataJsonFile);
                
            var funcNames = new List<string> { "FUNC1", "FUNC2" };
            
            // Setup the reader for the main query
            int callCount = -1;
            _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < funcNames.Count);
            _readerMock.Setup(r => r["OBJECT_NAME"]).Returns(() => funcNames[callCount]);
            _readerMock.Setup(r => r["OBJECT_TYPE"]).Returns("FUNCTION");
            
            // Setup parameter mock for definition query
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            // Setup the command for both ExecuteReader and ExecuteScalar
            _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _commandMock.Setup(c => c.ExecuteScalar()).Returns("CREATE FUNCTION FUNC1 RETURN NUMBER AS BEGIN RETURN 1; END;");
            _commandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
            
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetAllFunctionsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(funcNames.Count, result.Count);
            Assert.Contains(result, f => f.Name == "FUNC1");
            Assert.Contains(result, f => f.Name == "FUNC2");
            
            // Verify parameters were set correctly for the definition queries
            paramMock.VerifySet(p => p.ParameterName = "FunctionName", Times.AtLeastOnce());
            paramMock.VerifySet(p => p.Value = It.IsAny<string>(), Times.AtLeastOnce());
        }
        
        [Fact]
        public async Task GetStoredProcedureParametersAsync_ReturnsParameters()
        {
            // Arrange
            var procName = "GET_EMPLOYEE";
            var parameters = new List<ParameterMetadata>
            {
                new ParameterMetadata { Name = "P_EMP_ID", DataType = "NUMBER", Direction = "IN" },
                new ParameterMetadata { Name = "P_RESULT", DataType = "VARCHAR2", Direction = "OUT" }
            };
            
            // Setup parameter mock
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            // Setup reader for parameters
            int callCount = -1;
            _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < parameters.Count);
            _readerMock.Setup(r => r["ARGUMENT_NAME"]).Returns(() => parameters[callCount].Name);
            _readerMock.Setup(r => r["DATA_TYPE"]).Returns(() => parameters[callCount].DataType);
            _readerMock.Setup(r => r["IN_OUT"]).Returns(() => parameters[callCount].Direction);
            
            // Setup command
            _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _commandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
            
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetStoredProcedureParametersAsync(procName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(parameters.Count, result.Count);
            Assert.Contains(result, p => p.Name == "P_EMP_ID" && p.DataType == "NUMBER" && p.Direction == "IN");
            Assert.Contains(result, p => p.Name == "P_RESULT" && p.DataType == "VARCHAR2" && p.Direction == "OUT");
            
            // Verify parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "objectName");
            paramMock.VerifySet(p => p.Value = procName);
        }
        
        [Fact]
        public async Task GetFunctionParametersAsync_ReturnsParameters()
        {
            // Arrange
            var funcName = "CALC_SALARY";
            var parameters = new List<ParameterMetadata>
            {
                new ParameterMetadata { Name = "P_YEARS", DataType = "NUMBER", Direction = "IN" },
                new ParameterMetadata { Name = "RETURN", DataType = "NUMBER", Direction = "OUT" }
            };
            
            // Setup parameter mock
            var paramMock = new Mock<IDbDataParameter>();
            paramMock.SetupProperty(p => p.ParameterName);
            paramMock.SetupProperty(p => p.Value);
            
            // Setup reader for parameters
            int callCount = -1;
            _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < parameters.Count);
            _readerMock.Setup(r => r["ARGUMENT_NAME"]).Returns(() => parameters[callCount].Name);
            _readerMock.Setup(r => r["DATA_TYPE"]).Returns(() => parameters[callCount].DataType);
            _readerMock.Setup(r => r["IN_OUT"]).Returns(() => parameters[callCount].Direction);
            
            // Setup command
            _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _commandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
            
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetFunctionParametersAsync(funcName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(parameters.Count, result.Count);
            Assert.Contains(result, p => p.Name == "P_YEARS" && p.DataType == "NUMBER" && p.Direction == "IN");
            Assert.Contains(result, p => p.Name == "RETURN" && p.DataType == "NUMBER" && p.Direction == "OUT");
            
            // Verify parameter was set correctly
            paramMock.VerifySet(p => p.ParameterName = "objectName");
            paramMock.VerifySet(p => p.Value = funcName);
        }
        
        [Fact]
        public async Task GetStoredProceduresMetadataByNameAsync_FiltersCorrectly()
        {
            // Arrange
            // Create cache file with test data
            var cachedProcedures = new List<ProcedureFunctionMetadata> {
                new ProcedureFunctionMetadata { Name = "PROC1", Definition = "DEF1" },
                new ProcedureFunctionMetadata { Name = "PROC2", Definition = "DEF2" },
                new ProcedureFunctionMetadata { Name = "OTHER_PROC", Definition = "DEF3" }
            };
            
            // Create cache directory if it doesn't exist
            Directory.CreateDirectory(Directory.GetCurrentDirectory());
            
            // Create the cache file
            await File.WriteAllTextAsync(
                AppConstants.ProceduresMetadatJsonFile, 
                JsonSerializer.Serialize(cachedProcedures));

            try
            {
                // Names to filter by
                var procNames = new List<string> { "PROC1", "PROC2" };
                
                // Act
                var result = await _service.GetStoredProceduresMetadataByNameAsync(procNames);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Count);
                Assert.Contains(result, p => p.Name == "PROC1");
                Assert.Contains(result, p => p.Name == "PROC2");
                Assert.DoesNotContain(result, p => p.Name == "OTHER_PROC");
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.ProceduresMetadatJsonFile))
                    File.Delete(AppConstants.ProceduresMetadatJsonFile);
            }
        }
        
        [Fact]
        public async Task GetFunctionsMetadataByNameAsync_FiltersCorrectly()
        {
            // Arrange
            // Create cache file with test data
            var cachedFunctions = new List<ProcedureFunctionMetadata> {
                new ProcedureFunctionMetadata { Name = "FUNC1", Definition = "DEF1" },
                new ProcedureFunctionMetadata { Name = "FUNC2", Definition = "DEF2" },
                new ProcedureFunctionMetadata { Name = "OTHER_FUNC", Definition = "DEF3" }
            };
            
            // Create cache directory if it doesn't exist
            Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                AppConstants.FunctionsMetadataJsonFile, 
                JsonSerializer.Serialize(cachedFunctions));

            try
            {
                // Names to filter by
                var funcNames = new List<string> { "FUNC1", "FUNC2" };
                
                // Act
                var result = await _service.GetFunctionsMetadataByNameAsync(funcNames);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Count);
                Assert.Contains(result, f => f.Name == "FUNC1");
                Assert.Contains(result, f => f.Name == "FUNC2");
                Assert.DoesNotContain(result, f => f.Name == "OTHER_FUNC");
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.FunctionsMetadataJsonFile))
                    File.Delete(AppConstants.FunctionsMetadataJsonFile);
            }
        }
        
        [Fact]
        public async Task GetAllStoredProceduresAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(AppConstants.ProceduresMetadatJsonFile))
                File.Delete(AppConstants.ProceduresMetadatJsonFile);
                
            var expectedException = new InvalidOperationException("Test exception");
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetAllStoredProceduresAsync());
            Assert.Same(expectedException, exception);
        }
        
        [Fact]
        public async Task GetStoredProcedureParametersAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            var procName = "ERROR_PROC";
            var expectedException = new InvalidOperationException("Test exception");
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetStoredProcedureParametersAsync(procName));
            Assert.Same(expectedException, exception);
        }
        
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new StoredProcedureFunctionService(null, _cacheMock.Object, _loggerMock.Object));
            Assert.Equal("connectionFactory", exception.ParamName);
        }
        
        [Fact]
        public async Task GetAllFunctionsAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(AppConstants.FunctionsMetadataJsonFile))
                File.Delete(AppConstants.FunctionsMetadataJsonFile);
                
            var expectedException = new InvalidOperationException("Test exception");
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetAllFunctionsAsync());
            Assert.Same(expectedException, exception);
        }
        
        [Fact]
        public async Task GetFunctionParametersAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            var funcName = "ERROR_FUNC";
            var expectedException = new InvalidOperationException("Test exception");
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetFunctionParametersAsync(funcName));
            Assert.Same(expectedException, exception);
        }
        
        [Fact]
        public async Task GetFunctionParametersAsync_ThrowsArgumentException_WhenNameIsEmpty()
        {
            // Arrange
            var funcName = string.Empty;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetFunctionParametersAsync(funcName));
            Assert.Contains("The value cannot be an empty string", exception.Message);
        }
        
        [Fact]
        public async Task GetStoredProcedureParametersAsync_ThrowsArgumentException_WhenNameIsEmpty()
        {
            // Arrange
            var procName = string.Empty;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetStoredProcedureParametersAsync(procName));
            Assert.Contains("The value cannot be an empty string", exception.Message);
        }
        
        [Fact]
        public async Task GetAllStoredProceduresAsync_HandlesEmptyResult()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(AppConstants.ProceduresMetadatJsonFile))
                File.Delete(AppConstants.ProceduresMetadatJsonFile);
            
            _readerMock.Setup(r => r.Read()).Returns(false); // No rows
            _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
            _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            var result = await _service.GetAllStoredProceduresAsync();
            
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        
        // Additional tests to cover null parameter validation
        
        [Fact]
        public async Task GetFunctionParametersAsync_ThrowsArgumentException_WhenNameIsNull()
        {
            // Arrange
            string funcName = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetFunctionParametersAsync(funcName));
            Assert.Contains("The value cannot be an empty string", exception.Message);
        }

        [Fact]
        public async Task GetStoredProcedureParametersAsync_ThrowsArgumentException_WhenNameIsNull()
        {
            // Arrange
            string procName = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetStoredProcedureParametersAsync(procName));
            Assert.Contains("The value cannot be an empty string", exception.Message);
        }
        
        [Fact]
        public async Task GetStoredProceduresMetadataByNameAsync_HandlesEmptyList()
        {
            // Arrange
            var emptyList = new List<string>();
            
            // Create cache file with test data
            var cachedProcedures = new List<ProcedureFunctionMetadata> {
                new ProcedureFunctionMetadata { Name = "PROC1", Definition = "DEF1" }
            };

            // Create cache directory if it doesn't exist
            Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                AppConstants.ProceduresMetadatJsonFile, 
                JsonSerializer.Serialize(cachedProcedures));

            try
            {
                // Act
                var result = await _service.GetStoredProceduresMetadataByNameAsync(emptyList);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.ProceduresMetadatJsonFile))
                    File.Delete(AppConstants.ProceduresMetadatJsonFile);
            }
        }
        
        [Fact]
        public async Task GetFunctionsMetadataByNameAsync_HandlesEmptyList()
        {
            // Arrange
            var emptyList = new List<string>();
            
            // Create cache file with test data
            var cachedFunctions = new List<ProcedureFunctionMetadata> {
                new ProcedureFunctionMetadata { Name = "FUNC1", Definition = "DEF1" }
            };
            
            // Create cache directory if it doesn't exist
            Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                AppConstants.FunctionsMetadataJsonFile, 
                JsonSerializer.Serialize(cachedFunctions));

            try
            {
                // Act
                var result = await _service.GetFunctionsMetadataByNameAsync(emptyList);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.FunctionsMetadataJsonFile))
                    File.Delete(AppConstants.FunctionsMetadataJsonFile);
            }
        }
    }
}
