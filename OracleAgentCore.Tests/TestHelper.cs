using Moq;
using Microsoft.Extensions.Logging;

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
