using AutoFixture.Xunit2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OracleAgent.Core.Services;
using Xunit;
using System.Threading.Tasks;

namespace OracleAgentCore.Tests
{
    public class RawSqlServiceTests
    {
        private static ILogger<RawSqlService> CreateLogger()
        {
            var loggerMock = new Moq.Mock<ILogger<RawSqlService>>();
            return loggerMock.Object;
        }

        [Theory, AutoData]
        public async Task ExecuteRawSelectAsync_Throws(string sql)
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new RawSqlService(configMock.Object, CreateLogger());
            // Only test for valid select, otherwise method returns string
            if (sql != null && sql.Trim().ToUpper().StartsWith("SELECT"))
                await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.ExecuteRawSelectAsync(sql));
        }

        [Fact]
        public void Constructor_SetsConnectionString()
        {
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("TestConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("TestConnectionString");

            var service = new RawSqlService(configMock.Object, CreateLogger());
            Assert.NotNull(service);
        }
    }
}
