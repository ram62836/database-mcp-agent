using Microsoft.Extensions.Logging;
using Moq;

namespace OracleAgentCore.Tests
{
    public static class TestHelper
    {
        public static Mock<ILogger<T>> CreateLoggerMock<T>()
        {
            return new Mock<ILogger<T>>();
        }
    }
}
