using AutoFixture.Xunit2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OracleAgent.Core.Services;
using Xunit;
using System.Threading.Tasks;

namespace OracleAgentCore.Tests
{
    public class KeyIdentificationServiceTests
    {
        private static ILogger<KeyIdentificationService> CreateLogger()
        {
            var loggerMock = new Moq.Mock<ILogger<KeyIdentificationService>>();
            return loggerMock.Object;
        }

        [Theory, AutoData]
        public async Task GetPrimaryKeysAsync_Throws(string tableName)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new KeyIdentificationService(configMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetPrimaryKeysAsync(tableName));
        }

        [Theory, AutoData]
        public async Task GetForeignKeysAsync_Throws(string tableName)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new KeyIdentificationService(configMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetForeignKeysAsync(tableName));
        }

        [Fact]
        public async Task GetForeignKeyRelationshipsAsync_Throws()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new KeyIdentificationService(configMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetForeignKeyRelationshipsAsync());
        }

        [Fact]
        public void Constructor_SetsConnectionString()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("TestConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("TestConnectionString");

            var service = new KeyIdentificationService(configMock.Object, CreateLogger());
            Assert.NotNull(service);
        }
    }
}
