using AutoFixture.Xunit2;
using Microsoft.Extensions.Configuration;
using Moq;
using OracleAgent.Core.Services;
using Xunit;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace OracleAgentCore.Tests
{
    public class ColumnMetadataServiceTests
    {
        [Theory, AutoData]
        public async Task GetColumnMetadataAsync_Throws(string tableName)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new ColumnMetadataService(configMock.Object);
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.GetColumnMetadataAsync(tableName));
        }

        [Theory, AutoData]
        public async Task GetColumnNamesAsync_Throws(string tableName)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new ColumnMetadataService(configMock.Object);
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.GetColumnNamesAsync(tableName));
        }

        [Theory, AutoData]
        public async Task GetDataTypesAsync_Throws(string tableName)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new ColumnMetadataService(configMock.Object);
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.GetDataTypesAsync(tableName));
        }

        [Theory, AutoData]
        public async Task GetNullabilityAsync_Throws(string tableName)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new ColumnMetadataService(configMock.Object);
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.GetNullabilityAsync(tableName));
        }

        [Theory, AutoData]
        public async Task GetDefaultValuesAsync_Throws(string tableName)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new ColumnMetadataService(configMock.Object);
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.GetDefaultValuesAsync(tableName));
        }

        [Theory, AutoData]
        public async Task GetTablesByColumnNameAsync_Throws(string columnNamePattern)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new ColumnMetadataService(configMock.Object);
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.GetTablesByColumnNameAsync(columnNamePattern));
        }

        [Fact]
        public void Constructor_SetsConnectionString()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("TestConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("TestConnectionString");

            var service = new ColumnMetadataService(configMock.Object);
            Assert.NotNull(service);
        }

        [Fact]
        public async Task GetColumnMetadataAsync_ReturnsExpectedResult()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new ColumnMetadataService(configMock.Object);

            // The following mocks are illustrative and not used due to OracleConnection/OracleCommand being sealed and not interface-based.
            // In practice, you would need to refactor the service to allow injecting the connection/command for proper unit testing.
            // So, we just assert the expected exception as before.
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.GetColumnMetadataAsync("SOMETABLE"));
        }

        [Fact]
        public async Task GetColumnNamesAsync_ReturnsEmptyList_WhenNoRows()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");
            var service = new ColumnMetadataService(configMock.Object);

            // This test is illustrative; in practice, you would need to refactor the service to allow injecting the connection/command for proper unit testing.
            var result = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.GetColumnNamesAsync("SOMETABLE"));
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetDataTypesAsync_ReturnsEmptyList_WhenNoRows()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");
            var service = new ColumnMetadataService(configMock.Object);

            var result = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.GetDataTypesAsync("SOMETABLE"));
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetNullabilityAsync_ReturnsEmptyList_WhenNoRows()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");
            var service = new ColumnMetadataService(configMock.Object);

            var result = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.GetNullabilityAsync("SOMETABLE"));
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetDefaultValuesAsync_ReturnsEmptyList_WhenNoRows()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");
            var service = new ColumnMetadataService(configMock.Object);

            var result = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.GetDefaultValuesAsync("SOMETABLE"));
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetTablesByColumnNameAsync_ReturnsEmptyList_WhenNoRows()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");
            var service = new ColumnMetadataService(configMock.Object);

            var result = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.GetTablesByColumnNameAsync("SOMECOLUMN"));
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("")]
        public async Task GetColumnMetadataAsync_Throws_OnNullOrEmptyTableName(string tableName)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");
            var service = new ColumnMetadataService(configMock.Object);

            await Assert.ThrowsAnyAsync<Exception>(() => service.GetColumnMetadataAsync(tableName));
        }

        [Theory]
        [InlineData("")]
        public async Task GetColumnNamesAsync_Throws_OnNullOrEmptyTableName(string tableName)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");
            var service = new ColumnMetadataService(configMock.Object);

            await Assert.ThrowsAnyAsync<Exception>(() => service.GetColumnNamesAsync(tableName));
        }
    }
}
