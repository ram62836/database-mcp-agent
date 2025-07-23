namespace DatabaseMcp.Core.Tests
{
    [CollectionDefinition("Database Tests")]
    public class DatabaseTestsCollection : ICollectionFixture<TestCollectionFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public class TestCollectionFixture : IDisposable
    {
        private readonly string? _originalMetadataCacheJsonPath;
        private readonly string _testDirectory;

        public TestCollectionFixture()
        {
            // Store the original environment variable value
            _originalMetadataCacheJsonPath = Environment.GetEnvironmentVariable("MetadataCacheJsonPath");

            // Create a unique temporary directory for this test run
            _testDirectory = Path.Combine(Path.GetTempPath(), "DatabaseMcpAgent_Tests", Guid.NewGuid().ToString());
            _ = Directory.CreateDirectory(_testDirectory);

            // Set the environment variable to point to our test directory
            Environment.SetEnvironmentVariable("MetadataCacheJsonPath", _testDirectory);
        }

        public void Dispose()
        {
            try
            {
                // Restore the original environment variable value
                Environment.SetEnvironmentVariable("MetadataCacheJsonPath", _originalMetadataCacheJsonPath);

                // Clean up the test directory
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch (Exception)
            {
                // Ignore cleanup errors - they shouldn't fail the tests
            }
        }
    }
}
