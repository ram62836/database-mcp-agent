using DatabaseMcp.Core.Services;
using Moq;

namespace DatabaseMcp.Core.Tests
{
    [Collection("Database Tests")]
    public class DependencyInjectionIntegrationTests
    {
        [Fact]
        public void AllServices_CanBeInstantiatedWithCorrectDependencies()
        {
            // Arrange
            Mock<IDbConnectionFactory> connectionFactoryMock = new();
            _ = TestHelper.CreateLoggerMock<object>();

            // Act & Assert - Test that all services can be instantiated with their dependencies
            Assert.NotNull(new TableDiscoveryService(connectionFactoryMock.Object, TestHelper.CreateLoggerMock<TableDiscoveryService>().Object));
            Assert.NotNull(new RawSqlService(connectionFactoryMock.Object, TestHelper.CreateLoggerMock<RawSqlService>().Object));
            Assert.NotNull(new ViewEnumerationService(connectionFactoryMock.Object, TestHelper.CreateLoggerMock<ViewEnumerationService>().Object));
            Assert.NotNull(new StoredProcedureFunctionService(connectionFactoryMock.Object, TestHelper.CreateLoggerMock<StoredProcedureFunctionService>().Object));
            Assert.NotNull(new SynonymListingService(connectionFactoryMock.Object, TestHelper.CreateLoggerMock<SynonymListingService>().Object));
            Assert.NotNull(new TriggerService(connectionFactoryMock.Object, TestHelper.CreateLoggerMock<TriggerService>().Object));
            Assert.NotNull(new KeyIdentificationService(connectionFactoryMock.Object, TestHelper.CreateLoggerMock<KeyIdentificationService>().Object));
            Assert.NotNull(new IndexListingService(connectionFactoryMock.Object, TestHelper.CreateLoggerMock<IndexListingService>().Object));
            Assert.NotNull(new ConstraintGatheringService(connectionFactoryMock.Object, TestHelper.CreateLoggerMock<ConstraintGatheringService>().Object));
            Assert.NotNull(new ObjectRelationshipService(connectionFactoryMock.Object, TestHelper.CreateLoggerMock<ObjectRelationshipService>().Object));
        }

        [Fact]
        public void Services_ThrowArgumentNullException_WhenConnectionFactoryIsNull()
        {
            // Act & Assert
            _ = Assert.Throws<ArgumentNullException>(() => new TableDiscoveryService(null!, TestHelper.CreateLoggerMock<TableDiscoveryService>().Object));
            _ = Assert.Throws<ArgumentNullException>(() => new RawSqlService(null!, TestHelper.CreateLoggerMock<RawSqlService>().Object));
            _ = Assert.Throws<ArgumentNullException>(() => new ViewEnumerationService(null!, TestHelper.CreateLoggerMock<ViewEnumerationService>().Object));
            _ = Assert.Throws<ArgumentNullException>(() => new SynonymListingService(null!, TestHelper.CreateLoggerMock<SynonymListingService>().Object));
            _ = Assert.Throws<ArgumentNullException>(() => new TriggerService(null!, TestHelper.CreateLoggerMock<TriggerService>().Object));
            _ = Assert.Throws<ArgumentNullException>(() => new KeyIdentificationService(null!, TestHelper.CreateLoggerMock<KeyIdentificationService>().Object));
            _ = Assert.Throws<ArgumentNullException>(() => new IndexListingService(null!, TestHelper.CreateLoggerMock<IndexListingService>().Object));
            _ = Assert.Throws<ArgumentNullException>(() => new ConstraintGatheringService(null!, TestHelper.CreateLoggerMock<ConstraintGatheringService>().Object));
            _ = Assert.Throws<ArgumentNullException>(() => new ObjectRelationshipService(null!, TestHelper.CreateLoggerMock<ObjectRelationshipService>().Object));
        }

        [Fact]
        public void DatabaseConnectionService_CanBeInstantiatedWithConfiguration()
        {
            // Arrange
            Mock<Microsoft.Extensions.Configuration.IConfiguration> configurationMock = new();

            // Act & Assert
            Assert.NotNull(new DatabaseConnectionService(configurationMock.Object));
        }

        [Fact]
        public void DatabaseConnectionService_ThrowsArgumentNullException_WhenConfigurationIsNull()
        {
            // Act & Assert
            _ = Assert.Throws<ArgumentNullException>(() => new DatabaseConnectionService(null!));
        }
    }
}
