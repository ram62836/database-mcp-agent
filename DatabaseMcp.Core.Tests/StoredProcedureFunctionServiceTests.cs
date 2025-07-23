using System.Data;
using System.Text.Json;
using DatabaseMcp.Core.Models;
using DatabaseMcp.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DatabaseMcp.Core.Tests
{
    [Collection("Database Tests")]
    public class StoredProcedureFunctionServiceTests
    {
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
        private readonly Mock<ILogger<StoredProcedureFunctionService>> _loggerMock;
        private readonly Mock<IDbConnection> _connectionMock;
        private readonly Mock<IDbCommand> _commandMock;
        private readonly Mock<IDataReader> _readerMock;
        private readonly StoredProcedureFunctionService _service;

        public StoredProcedureFunctionServiceTests()
        {
            _connectionFactoryMock = new Mock<IDbConnectionFactory>();
            _loggerMock = TestHelper.CreateLoggerMock<StoredProcedureFunctionService>();
            _connectionMock = new Mock<IDbConnection>();
            _commandMock = new Mock<IDbCommand>();
            _readerMock = new Mock<IDataReader>();

            _service = new StoredProcedureFunctionService(
                _connectionFactoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllStoredProceduresAsync_ReturnsList()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(AppConstants.ProceduresMetadatJsonFile))
            {
                File.Delete(AppConstants.ProceduresMetadatJsonFile);
            }

            List<string> procNames = ["PROC1", "PROC2"];

            // Setup the reader for the main query
            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < procNames.Count);
            _ = _readerMock.Setup(r => r["OBJECT_NAME"]).Returns(() => procNames[callCount]);
            _ = _readerMock.Setup(r => r["OBJECT_TYPE"]).Returns("PROCEDURE");

            // Setup parameter mock for definition query
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            // Setup the command for both ExecuteReader and ExecuteScalar
            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _ = _commandMock.Setup(c => c.ExecuteScalar()).Returns("CREATE PROCEDURE PROC1 AS BEGIN NULL; END;");
            _ = _commandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _ = _commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);

            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<ProcedureFunctionMetadata> result = await _service.GetAllStoredProceduresAsync();

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
            List<ProcedureFunctionMetadata> cachedProcedures =
            [
                new ProcedureFunctionMetadata { Name = "CACHED_PROC", Definition = "CACHED DEFINITION" }
            ];

            // Create cache directory if it doesn't exist
            _ = Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create directory if it doesn't exist
            string? dir = Path.GetDirectoryName(AppConstants.ProceduresMetadatJsonFile);
            if (!string.IsNullOrEmpty(dir))
            {
                _ = Directory.CreateDirectory(dir);
            }
            await File.WriteAllTextAsync(
                AppConstants.ProceduresMetadatJsonFile,
                JsonSerializer.Serialize(cachedProcedures));

            try
            {
                // Act
                List<ProcedureFunctionMetadata> result = await _service.GetAllStoredProceduresAsync();

                // Assert
                Assert.NotNull(result);
                _ = Assert.Single(result);
                Assert.Equal("CACHED_PROC", result[0].Name);
                Assert.Equal("CACHED DEFINITION", result[0].Definition);

                // Verify no database connection was made
                _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(), Times.Never);
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.ProceduresMetadatJsonFile))
                {
                    File.Delete(AppConstants.ProceduresMetadatJsonFile);
                }
            }
        }

        [Fact]
        public async Task GetAllFunctionsAsync_ReturnsList()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(AppConstants.FunctionsMetadataJsonFile))
            {
                File.Delete(AppConstants.FunctionsMetadataJsonFile);
            }

            List<string> funcNames = ["FUNC1", "FUNC2"];

            // Setup the reader for the main query
            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < funcNames.Count);
            _ = _readerMock.Setup(r => r["OBJECT_NAME"]).Returns(() => funcNames[callCount]);
            _ = _readerMock.Setup(r => r["OBJECT_TYPE"]).Returns("FUNCTION");

            // Setup parameter mock for definition query
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            // Setup the command for both ExecuteReader and ExecuteScalar
            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _ = _commandMock.Setup(c => c.ExecuteScalar()).Returns("CREATE FUNCTION FUNC1 RETURN NUMBER AS BEGIN RETURN 1; END;");
            _ = _commandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _ = _commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);

            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<ProcedureFunctionMetadata> result = await _service.GetAllFunctionsAsync();

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
            string procName = "GET_EMPLOYEE";
            List<ParameterMetadata> parameters =
            [
                new ParameterMetadata { Name = "P_EMP_ID", DataType = "NUMBER", Direction = "IN" },
                new ParameterMetadata { Name = "P_RESULT", DataType = "VARCHAR2", Direction = "OUT" }
            ];

            // Setup parameter mock
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            // Setup reader for parameters
            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < parameters.Count);
            _ = _readerMock.Setup(r => r["ARGUMENT_NAME"]).Returns(() => parameters[callCount].Name);
            _ = _readerMock.Setup(r => r["DATA_TYPE"]).Returns(() => parameters[callCount].DataType);
            _ = _readerMock.Setup(r => r["IN_OUT"]).Returns(() => parameters[callCount].Direction);

            // Setup command
            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _ = _commandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _ = _commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);

            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<ParameterMetadata> result = await _service.GetStoredProcedureParametersAsync(procName);

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
            string funcName = "CALC_SALARY";
            List<ParameterMetadata> parameters =
            [
                new ParameterMetadata { Name = "P_YEARS", DataType = "NUMBER", Direction = "IN" },
                new ParameterMetadata { Name = "RETURN", DataType = "NUMBER", Direction = "OUT" }
            ];

            // Setup parameter mock
            Mock<IDbDataParameter> paramMock = new();
            _ = paramMock.SetupProperty(p => p.ParameterName);
            _ = paramMock.SetupProperty(p => p.Value);

            // Setup reader for parameters
            int callCount = -1;
            _ = _readerMock.Setup(r => r.Read()).Returns(() => ++callCount < parameters.Count);
            _ = _readerMock.Setup(r => r["ARGUMENT_NAME"]).Returns(() => parameters[callCount].Name);
            _ = _readerMock.Setup(r => r["DATA_TYPE"]).Returns(() => parameters[callCount].DataType);
            _ = _readerMock.Setup(r => r["IN_OUT"]).Returns(() => parameters[callCount].Direction);

            // Setup command
            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _ = _commandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _ = _commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);

            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<ParameterMetadata> result = await _service.GetFunctionParametersAsync(funcName);

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
            List<ProcedureFunctionMetadata> cachedProcedures =
            [
                new ProcedureFunctionMetadata { Name = "PROC1", Definition = "DEF1" },
                new ProcedureFunctionMetadata { Name = "PROC2", Definition = "DEF2" },
                new ProcedureFunctionMetadata { Name = "OTHER_PROC", Definition = "DEF3" }
            ];

            // Create cache directory if it doesn't exist
            _ = Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                AppConstants.ProceduresMetadatJsonFile,
                JsonSerializer.Serialize(cachedProcedures));

            try
            {
                // Names to filter by
                List<string> procNames = ["PROC1", "PROC2"];

                // Act
                List<ProcedureFunctionMetadata> result = await _service.GetStoredProceduresMetadataByNameAsync(procNames);

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
                {
                    File.Delete(AppConstants.ProceduresMetadatJsonFile);
                }
            }
        }

        [Fact]
        public async Task GetFunctionsMetadataByNameAsync_FiltersCorrectly()
        {
            // Arrange
            // Create cache file with test data
            List<ProcedureFunctionMetadata> cachedFunctions =
            [
                new ProcedureFunctionMetadata { Name = "FUNC1", Definition = "DEF1" },
                new ProcedureFunctionMetadata { Name = "FUNC2", Definition = "DEF2" },
                new ProcedureFunctionMetadata { Name = "OTHER_FUNC", Definition = "DEF3" }
            ];

            // Create cache directory if it doesn't exist
            _ = Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                AppConstants.FunctionsMetadataJsonFile,
                JsonSerializer.Serialize(cachedFunctions));

            try
            {
                // Names to filter by
                List<string> funcNames = ["FUNC1", "FUNC2"];

                // Act
                List<ProcedureFunctionMetadata> result = await _service.GetFunctionsMetadataByNameAsync(funcNames);

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
                {
                    File.Delete(AppConstants.FunctionsMetadataJsonFile);
                }
            }
        }

        [Fact]
        public async Task GetAllStoredProceduresAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(AppConstants.ProceduresMetadatJsonFile))
            {
                File.Delete(AppConstants.ProceduresMetadatJsonFile);
            }

            InvalidOperationException expectedException = new("Test exception");
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                _service.GetAllStoredProceduresAsync);
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetStoredProcedureParametersAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            string procName = "ERROR_PROC";
            InvalidOperationException expectedException = new("Test exception");
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetStoredProcedureParametersAsync(procName));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task Constructor_ThrowsArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Arrange, Act & Assert
            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                _ = await Task.Run(() => new StoredProcedureFunctionService(null, _loggerMock.Object));
            });
            Assert.Equal("connectionFactory", exception.ParamName);
        }

        [Fact]
        public async Task GetAllFunctionsAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(AppConstants.FunctionsMetadataJsonFile))
            {
                File.Delete(AppConstants.FunctionsMetadataJsonFile);
            }

            InvalidOperationException expectedException = new("Test exception");
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                _service.GetAllFunctionsAsync);
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetFunctionParametersAsync_ThrowsException_WhenDbFails()
        {
            // Arrange
            string funcName = "ERROR_FUNC";
            InvalidOperationException expectedException = new("Test exception");
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetFunctionParametersAsync(funcName));
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetFunctionParametersAsync_ThrowsArgumentException_WhenNameIsEmpty()
        {
            // Arrange
            string funcName = string.Empty;

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetFunctionParametersAsync(funcName));
            Assert.Contains("The value cannot be an empty string", exception.Message);
        }

        [Fact]
        public async Task GetStoredProcedureParametersAsync_ThrowsArgumentException_WhenNameIsEmpty()
        {
            // Arrange
            string procName = string.Empty;

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetStoredProcedureParametersAsync(procName));
            Assert.Contains("The value cannot be an empty string", exception.Message);
        }

        [Fact]
        public async Task GetAllStoredProceduresAsync_HandlesEmptyResult()
        {
            // Arrange
            // Ensure cache file does not exist
            if (File.Exists(AppConstants.ProceduresMetadatJsonFile))
            {
                File.Delete(AppConstants.ProceduresMetadatJsonFile);
            }

            // Reset mocks to clear previous setups
            _readerMock.Reset();
            _commandMock.Reset();
            _connectionMock.Reset();
            _connectionFactoryMock.Reset();

            _ = _readerMock.Setup(r => r.Read()).Returns(false); // No rows
            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            _ = _commandMock.Setup(c => c.ExecuteScalar()).Returns(default(object)); // No definition
            _ = _commandMock.SetupGet(c => c.Parameters).Returns(new Mock<IDataParameterCollection>().Object);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);

            // Act
            List<ProcedureFunctionMetadata> result = await _service.GetAllStoredProceduresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // Additional tests to cover null parameter validation

        [Fact]
        public async Task GetFunctionParametersAsync_ThrowsArgumentException_WhenNameIsNull()
        {
            // Arrange
            string? funcName = null;

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetFunctionParametersAsync(funcName));
            Assert.Contains("The value cannot be an empty string", exception.Message);
        }

        [Fact]
        public async Task GetStoredProcedureParametersAsync_ThrowsArgumentException_WhenNameIsNull()
        {
            // Arrange
            string? procName = null;

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetStoredProcedureParametersAsync(procName));
            Assert.Contains("The value cannot be an empty string", exception.Message);
        }

        [Fact]
        public async Task GetStoredProceduresMetadataByNameAsync_HandlesEmptyList()
        {
            // Arrange
            List<string> emptyList = [];

            // Create cache file with test data
            List<ProcedureFunctionMetadata> cachedProcedures =
            [
                new ProcedureFunctionMetadata { Name = "PROC1", Definition = "DEF1" }
            ];

            // Create cache directory if it doesn't exist
            _ = Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                AppConstants.ProceduresMetadatJsonFile,
                JsonSerializer.Serialize(cachedProcedures));

            try
            {
                // Act
                List<ProcedureFunctionMetadata> result = await _service.GetStoredProceduresMetadataByNameAsync(emptyList);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.ProceduresMetadatJsonFile))
                {
                    File.Delete(AppConstants.ProceduresMetadatJsonFile);
                }
            }
        }

        [Fact]
        public async Task GetFunctionsMetadataByNameAsync_HandlesEmptyList()
        {
            // Arrange
            List<string> emptyList = [];

            // Create cache file with test data
            List<ProcedureFunctionMetadata> cachedFunctions =
            [
                new ProcedureFunctionMetadata { Name = "FUNC1", Definition = "DEF1" }
            ];

            // Create cache directory if it doesn't exist
            _ = Directory.CreateDirectory(Directory.GetCurrentDirectory());

            // Create the cache file
            await File.WriteAllTextAsync(
                AppConstants.FunctionsMetadataJsonFile,
                JsonSerializer.Serialize(cachedFunctions));

            try
            {
                // Act
                List<ProcedureFunctionMetadata> result = await _service.GetFunctionsMetadataByNameAsync(emptyList);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }
            finally
            {
                // Clean up
                if (File.Exists(AppConstants.FunctionsMetadataJsonFile))
                {
                    File.Delete(AppConstants.FunctionsMetadataJsonFile);
                }
            }
        }
    }
}

