using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using DatabaseMcp.Core.Services;
using Moq;

namespace DatabaseMcp.Core.Tests
{
    public class PackageServiceTests
    {
        private readonly Mock<IRawSqlService> _rawSqlServiceMock;
        private readonly Mock<IObjectRelationshipService> _objectRelationshipServiceMock;
        private readonly IPackageService _IPackageToolService;

        public PackageServiceTests()
        {
            _rawSqlServiceMock = new Mock<IRawSqlService>();
            _objectRelationshipServiceMock = new Mock<IObjectRelationshipService>();
            _IPackageToolService = new PackageService(_rawSqlServiceMock.Object, _objectRelationshipServiceMock.Object);
        }

        [Fact]
        public async Task GetPackageDefinitionAsync_ReturnsDefinition()
        {
            var packageName = "TEST_PACKAGE";
            var expected = "PACKAGE DEFINITION TEXT";
            _rawSqlServiceMock.Setup(s => s.ExecuteRawSelectAsync(It.IsAny<string>())).ReturnsAsync(expected);

            var result = await _IPackageToolService.GetPackageDefinitionAsync(packageName);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetPackageBodyAsync_ReturnsBody()
        {
            var packageName = "TEST_PACKAGE";
            var expected = "PACKAGE BODY TEXT";
            _rawSqlServiceMock.Setup(s => s.ExecuteRawSelectAsync(It.IsAny<string>())).ReturnsAsync(expected);

            var result = await _IPackageToolService.GetPackageBodyAsync(packageName);

            Assert.Equal(expected, result);
        }
    }
}
