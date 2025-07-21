# Contributing to Oracle Database MCP Agent

Thank you for your interest in contributing to the Oracle Database MCP Agent! This document provides guidelines and instructions for contributing to this project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Issue Reporting](#issue-reporting)

## Code of Conduct

This project adheres to a Code of Conduct that we expect all contributors to follow. Please be respectful and constructive in all interactions.

## Getting Started

1. Fork the repository on GitHub
2. Clone your fork locally
3. Set up the development environment
4. Create a branch for your contribution
5. Make your changes
6. Test your changes
7. Submit a pull request

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- Oracle Database (for testing)
- Git
- Your favorite IDE (Visual Studio, VS Code, Rider, etc.)

### Initial Setup

```bash
# Clone your fork
git clone https://github.com/your-username/oracle-ai-agent.git
cd oracle-ai-agent

# Add the original repository as upstream
git remote add upstream https://github.com/original-owner/oracle-ai-agent.git

# Install dependencies
dotnet restore

# Build the solution
dotnet build

# Copy example configuration files
cp appsettings.example.json appsettings.json
cp DatabaseMcp.Server/appsettings.example.json DatabaseMcp.Server/appsettings.json
cp DatabaseMcp.Client/appsettings.example.json DatabaseMcp.Client/appsettings.json

# Update appsettings.json files with your database connection details
```

## How to Contribute

### Types of Contributions

- **Bug Reports**: Help us identify and fix issues
- **Feature Requests**: Suggest new features or improvements
- **Code Contributions**: Implement bug fixes or new features
- **Documentation**: Improve or add documentation
- **Testing**: Add or improve test coverage

### Before You Start

1. Check existing issues and pull requests to avoid duplicates
2. For major changes, please open an issue first to discuss the proposed changes
3. Make sure your development environment is set up correctly

## Pull Request Process

1. **Create a Branch**: Create a feature branch from `main`
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make Changes**: Implement your changes following our coding standards

3. **Test**: Ensure all tests pass and add new tests for your changes
   ```bash
   dotnet test
   ```

4. **Commit**: Use clear, descriptive commit messages
   ```bash
   git commit -m "Add feature: description of what you added"
   ```

5. **Push**: Push your branch to your fork
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Submit PR**: Create a pull request with:
   - Clear title and description
   - Reference to any related issues
   - Screenshots or examples if applicable

## Coding Standards

### C# Code Style

- Follow standard C# naming conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Use async/await patterns appropriately
- Handle exceptions gracefully

### Project Structure

- Keep business logic in `DatabaseMcp.Core`
- MCP-specific tools go in `DatabaseMcp.Server/Tools`
- Add interfaces to `DatabaseMcp.Core/Interfaces`
- Place models in `DatabaseMcp.Core/Models`
- Service implementations belong in `DatabaseMcp.Core/Services`

### Example Code Style

```csharp
/// <summary>
/// Retrieves metadata for the specified table.
/// </summary>
/// <param name="tableName">The name of the table.</param>
/// <returns>Table metadata or null if not found.</returns>
public async Task<TableMetadata?> GetTableMetadataAsync(string tableName)
{
    if (string.IsNullOrWhiteSpace(tableName))
        throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));

    try
    {
        // Implementation here
        return await SomeAsyncOperation(tableName);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to retrieve metadata for table {TableName}", tableName);
        throw;
    }
}
```

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test DatabaseMcp.Core.Tests
```

### Writing Tests

- Write unit tests for all new functionality
- Use meaningful test names that describe what is being tested
- Follow AAA pattern (Arrange, Act, Assert)
- Mock external dependencies
- Test both success and failure scenarios

### Test Example

```csharp
[Test]
public async Task GetTableMetadataAsync_WithValidTableName_ReturnsMetadata()
{
    // Arrange
    var service = new TableDiscoveryService(_mockConnectionFactory.Object, _mockLogger.Object);
    var tableName = "EMPLOYEES";

    // Act
    var result = await service.GetTableMetadataAsync(tableName);

    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result.TableName, Is.EqualTo(tableName));
}
```

## Issue Reporting

When reporting issues, please include:

1. **Clear Description**: What happened vs. what you expected
2. **Steps to Reproduce**: Detailed steps to reproduce the issue
3. **Environment**: OS, .NET version, Oracle version
4. **Error Messages**: Full error messages and stack traces
5. **Configuration**: Relevant configuration (remove sensitive data)

### Issue Template

```
**Description**
A clear description of the issue.

**Steps to Reproduce**
1. Step one
2. Step two
3. Step three

**Expected Behavior**
What should happen.

**Actual Behavior**
What actually happened.

**Environment**
- OS: [e.g., Windows 11]
- .NET: [e.g., 8.0]
- Oracle: [e.g., 19c]

**Additional Context**
Any other relevant information.
```

## Documentation

- Update README.md if your changes affect setup or usage
- Add XML documentation for public APIs
- Update relevant documentation files
- Include code examples where helpful

## Questions?

If you have questions about contributing, please:

1. Check existing documentation
2. Search existing issues
3. Open a new issue with the `question` label

Thank you for contributing to Oracle Database MCP Agent!
