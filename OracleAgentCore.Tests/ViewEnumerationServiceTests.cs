using AutoFixture.Xunit2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OracleAgent.Core.Services;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OracleAgentCore.Tests
{
    public class ViewEnumerationServiceTests
    {
        private static ILogger<ViewEnumerationService> CreateLogger()
        {
            var loggerMock = new Moq.Mock<ILogger<ViewEnumerationService>>();
            return loggerMock.Object;
        }

        [Fact]
        public async Task GetAllViewsAsync_Throws()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new ViewEnumerationService(configMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetAllViewsAsync());
        }

        [Theory, AutoData]
        public async Task GetViewsDefinitionAsync_Throws(List<string> viewNames)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new ViewEnumerationService(configMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetViewsDefinitionAsync(viewNames));
        }

        [Fact]
        public void Constructor_SetsConnectionString()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("TestConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("TestConnectionString");

            var service = new ViewEnumerationService(configMock.Object, CreateLogger());
            Assert.NotNull(service);
        }
    }
}
