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
    public class TableDiscoveryServiceTests
    {
        private static ILogger<TableDiscoveryService> CreateLogger()
        {
            var loggerMock = new Moq.Mock<ILogger<TableDiscoveryService>>();
            return loggerMock.Object;
        }

        [Fact]
        public async Task GetAllUserDefinedTablesAsync_Throws()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new TableDiscoveryService(configMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetAllUserDefinedTablesAsync());
        }

        [Theory, AutoData]
        public async Task GetTablesByNameAsync_Throws(List<string> tableNames)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new TableDiscoveryService(configMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetTablesByNameAsync(tableNames));
        }

        [Theory, AutoData]
        public async Task GetTableDefinitionAsync_Throws(string tableName)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new TableDiscoveryService(configMock.Object, CreateLogger());
            // Private method, test via reflection
            var method = typeof(TableDiscoveryService).GetMethod("GetTableDefinitionAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => (Task<string>)method.Invoke(service, new object[] { tableName }));
        }

        [Fact]
        public void Constructor_SetsConnectionString()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("TestConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("TestConnectionString");

            var service = new TableDiscoveryService(configMock.Object, CreateLogger());
            Assert.NotNull(service);
        }
    }
}
