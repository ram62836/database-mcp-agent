using DatabaseMcp.Core.Interfaces;
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
            string packageName = "TEST_PACKAGE";
            string expected = "PACKAGE DEFINITION TEXT";
            _ = _rawSqlServiceMock.Setup(s => s.ExecuteRawSelectAsync(It.IsAny<string>())).ReturnsAsync(expected);

            string result = await _IPackageToolService.GetPackageDefinitionAsync(packageName);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetPackageBodyAsync_ReturnsBody()
        {
            string packageName = "TEST_PACKAGE";
            string expected = "PACKAGE BODY TEXT";
            _ = _rawSqlServiceMock.Setup(s => s.ExecuteRawSelectAsync(It.IsAny<string>())).ReturnsAsync(expected);

            string result = await _IPackageToolService.GetPackageBodyAsync(packageName);

            Assert.Equal(expected, result);
        }
    }
}
