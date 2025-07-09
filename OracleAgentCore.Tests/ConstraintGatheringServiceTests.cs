using AutoFixture.Xunit2;
using Microsoft.Extensions.Configuration;
using Moq;
using OracleAgent.Core.Services;
using Xunit;
using System.Threading.Tasks;

namespace OracleAgentCore.Tests
{
    public class ConstraintGatheringServiceTests
    {
        [Theory, AutoData]
        public async Task GetUniqueConstraintsAsync_Throws(string tableName)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new ConstraintGatheringService(configMock.Object);
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetUniqueConstraintsAsync(tableName));
        }

        [Theory, AutoData]
        public async Task GetCheckConstraintsAsync_Throws(string tableName)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new ConstraintGatheringService(configMock.Object);
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetCheckConstraintsAsync(tableName));
        }

        [Fact]
        public void Constructor_SetsConnectionString()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("TestConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("TestConnectionString");

            var service = new ConstraintGatheringService(configMock.Object);
            Assert.NotNull(service);
        }
    }
}
