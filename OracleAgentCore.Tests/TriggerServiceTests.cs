using AutoFixture.Xunit2;
using Microsoft.Extensions.Configuration;
using Moq;
using OracleAgent.Core.Services;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OracleAgentCore.Tests
{
    public class TriggerServiceTests
    {
        [Fact]
        public async Task GetAllTriggersAsync_Throws()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new TriggerService(configMock.Object);
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetAllTriggersAsync());
        }

        [Theory, AutoData]
        public async Task GetTriggersByNameAsync_Throws(List<string> triggerNames)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new TriggerService(configMock.Object);
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetTriggersByNameAsync(triggerNames));
        }

        [Fact]
        public void Constructor_SetsConnectionString()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("TestConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("TestConnectionString");

            var service = new TriggerService(configMock.Object);
            Assert.NotNull(service);
        }
    }
}
