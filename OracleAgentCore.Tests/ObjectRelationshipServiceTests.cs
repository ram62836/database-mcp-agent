using AutoFixture.Xunit2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OracleAgent.Core.Services;
using Xunit;
using System.Threading.Tasks;

namespace OracleAgentCore.Tests
{
    public class ObjectRelationshipServiceTests
    {
        private static ILogger<ObjectRelationshipService> CreateLogger()
        {
            var loggerMock = new Moq.Mock<ILogger<ObjectRelationshipService>>();
            return loggerMock.Object;
        }

        [Theory, AutoData]
        public async Task GetReferenceObjects_Throws(string objectName, string objectType)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new ObjectRelationshipService(configMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetReferenceObjects(objectName, objectType));
        }

        [Fact]
        public void Constructor_SetsConnectionString()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("TestConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("TestConnectionString");

            var service = new ObjectRelationshipService(configMock.Object, CreateLogger());
            Assert.NotNull(service);
        }
    }
}
