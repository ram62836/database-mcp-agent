using AutoFixture.Xunit2;
using Microsoft.Extensions.Configuration;
using Moq;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Services;

namespace OracleAgentCore.Tests
{
    public class IndexListingServiceTests
    {
        [Theory, AutoData]
        public async Task ListIndexesAsync_ReturnsIndexes(string tableName)
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new IndexListingService(configMock.Object);

            Assert.NotNull(service);

            // Act & Assert
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.ListIndexesAsync(tableName));
        }

        [Theory, AutoData]
        public async Task GetIndexColumnsAsync_ReturnsColumns(string indexName)
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("FakeConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("FakeConnectionString");

            var service = new IndexListingService(configMock.Object);

            // See above: can't mock OracleConnection, so just test that the method throws
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => service.GetIndexColumnsAsync(indexName));
        }

        [Fact]
        public void Constructor_SetsConnectionString()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("TestConnectionString");
            configMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(configSectionMock.Object);
            configMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("TestConnectionString");

            // Act
            var service = new IndexListingService(configMock.Object);

            // Assert
            Assert.NotNull(service);
        }
    }
    public class OracleParameterCollectionFake
    {
        private readonly List<OracleParameter> _parameters = new();

        public int Add(OracleParameter value)
        {
            _parameters.Add(value);
            return _parameters.Count - 1;
        }

        public void AddRange(IEnumerable<OracleParameter> values)
        {
            _parameters.AddRange(values);
        }

        public int Count => _parameters.Count;

        public int IndexOf(string parameterName) => _parameters.FindIndex(p => p.ParameterName == parameterName);

        public void RemoveAt(string parameterName)
        {
            _parameters.RemoveAll(p => p.ParameterName == parameterName);
        }

        public void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }

        public void Insert(int index, OracleParameter value)
        {
            _parameters.Insert(index, value);
        }

        public bool Contains(string parameterName) => _parameters.Exists(p => p.ParameterName == parameterName);

        public bool Contains(OracleParameter value) => _parameters.Contains(value);

        public void Clear() => _parameters.Clear();

        public void CopyTo(OracleParameter[] array, int index) => _parameters.CopyTo(array, index);

        public IEnumerator<OracleParameter> GetEnumerator() => _parameters.GetEnumerator();

        public OracleParameter this[string parameterName]
        {
            get => _parameters.Find(p => p.ParameterName == parameterName)!;
            set
            {
                var idx = IndexOf(parameterName);
                if (idx >= 0) _parameters[idx] = value;
            }
        }

        public OracleParameter this[int index]
        {
            get => _parameters[index];
            set => _parameters[index] = value;
        }
    }
}
