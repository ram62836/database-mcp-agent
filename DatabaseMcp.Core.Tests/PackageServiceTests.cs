using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Services;
using Moq;
using System.Data;

namespace DatabaseMcp.Core.Tests
{
    public class PackageServiceTests
    {
        private readonly Mock<IRawSqlService> _rawSqlServiceMock;
        private readonly Mock<IObjectRelationshipService> _objectRelationshipServiceMock;
        private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
        private readonly Mock<IDbConnection> _connectionMock;
        private readonly Mock<IDbCommand> _commandMock;
        private readonly Mock<IDataReader> _readerMock;
        private readonly Mock<IDbDataParameter> _parameterMock;
        private readonly IPackageService _IPackageToolService;

        public PackageServiceTests()
        {
            _rawSqlServiceMock = new Mock<IRawSqlService>();
            _objectRelationshipServiceMock = new Mock<IObjectRelationshipService>();
            _connectionFactoryMock = new Mock<IDbConnectionFactory>();
            _connectionMock = new Mock<IDbConnection>();
            _commandMock = new Mock<IDbCommand>();
            _readerMock = new Mock<IDataReader>();
            _parameterMock = new Mock<IDbDataParameter>();
            
            // Setup parameter collection mock
            var parameterCollectionMock = new Mock<IDataParameterCollection>();
            
            // Setup the connection factory and related mocks
            _ = _connectionFactoryMock.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(_connectionMock.Object);
            _ = _connectionMock.Setup(c => c.CreateCommand()).Returns(_commandMock.Object);
            _ = _commandMock.Setup(c => c.CreateParameter()).Returns(_parameterMock.Object);
            _ = _commandMock.Setup(c => c.Parameters).Returns(parameterCollectionMock.Object);
            _ = _commandMock.Setup(c => c.ExecuteReader()).Returns(_readerMock.Object);
            
            _IPackageToolService = new PackageService(_rawSqlServiceMock.Object, _objectRelationshipServiceMock.Object, _connectionFactoryMock.Object);
        }

        [Fact]
        public async Task GetPackageDefinitionAsync_ReturnsDefinition()
        {
            string packageName = "TEST_PACKAGE";
            string expected = "PACKAGE DEFINITION TEXT";
            
            // Setup the reader to return the expected text
            _ = _readerMock.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            _ = _readerMock.Setup(r => r["TEXT"]).Returns(expected);

            string result = await _IPackageToolService.GetPackageDefinitionAsync(packageName);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetPackageBodyAsync_ReturnsBody()
        {
            string packageName = "TEST_PACKAGE";
            string expected = "PACKAGE BODY TEXT";
            
            // Setup the reader to return the expected text
            _ = _readerMock.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            _ = _readerMock.Setup(r => r["TEXT"]).Returns(expected);

            string result = await _IPackageToolService.GetPackageBodyAsync(packageName);

            Assert.Equal(expected, result);
        }
    }
}
