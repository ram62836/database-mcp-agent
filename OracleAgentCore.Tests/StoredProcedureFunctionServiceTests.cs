using AutoFixture.Xunit2;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OracleAgent.Core.Services;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OracleAgentCore.Tests
{
    public class StoredProcedureFunctionServiceTests
    {
        private static ILogger<StoredProcedureFunctionService> CreateLogger()
        {
            var loggerMock = new Moq.Mock<ILogger<StoredProcedureFunctionService>>();
            return loggerMock.Object;
        }

        [Theory, AutoData]
        public async Task GetAllStoredProceduresAsync_Throws()
        {
            var configMock = new Mock<IConfiguration>();
            var cacheMock = new Mock<IMemoryCache>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new StoredProcedureFunctionService(configMock.Object, cacheMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetAllStoredProceduresAsync());
        }

        [Theory, AutoData]
        public async Task GetAllFunctionsAsync_Throws()
        {
            var configMock = new Mock<IConfiguration>();
            var cacheMock = new Mock<IMemoryCache>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new StoredProcedureFunctionService(configMock.Object, cacheMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetAllFunctionsAsync());
        }

        [Theory, AutoData]
        public async Task GetStoredProceduresMetadataByNameAsync_Throws(List<string> names)
        {
            var configMock = new Mock<IConfiguration>();
            var cacheMock = new Mock<IMemoryCache>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new StoredProcedureFunctionService(configMock.Object, cacheMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetStoredProceduresMetadataByNameAsync(names));
        }

        [Theory, AutoData]
        public async Task GetFunctionsMetadataByNameAsync_Throws(List<string> names)
        {
            var configMock = new Mock<IConfiguration>();
            var cacheMock = new Mock<IMemoryCache>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new StoredProcedureFunctionService(configMock.Object, cacheMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetFunctionsMetadataByNameAsync(names));
        }

        [Theory, AutoData]
        public async Task GetStoredProcedureParametersAsync_Throws(string name)
        {
            var configMock = new Mock<IConfiguration>();
            var cacheMock = new Mock<IMemoryCache>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new StoredProcedureFunctionService(configMock.Object, cacheMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetStoredProcedureParametersAsync(name));
        }

        [Theory, AutoData]
        public async Task GetFunctionParametersAsync_Throws(string name)
        {
            var configMock = new Mock<IConfiguration>();
            var cacheMock = new Mock<IMemoryCache>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new StoredProcedureFunctionService(configMock.Object, cacheMock.Object, CreateLogger());
            await Assert.ThrowsAnyAsync<System.InvalidOperationException>(() => service.GetFunctionParametersAsync(name));
        }

        [Fact]
        public void Constructor_SetsConnectionString()
        {
            var configMock = new Mock<IConfiguration>();
            var cacheMock = new Mock<IMemoryCache>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("TestConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("TestConnectionString");

            var service = new StoredProcedureFunctionService(configMock.Object, cacheMock.Object, CreateLogger());
            Assert.NotNull(service);
        }
    }
}
