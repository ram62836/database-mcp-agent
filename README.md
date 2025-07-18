# Oracle Database MCP Agent

Oracle Database MCP Agent is a .NET solution designed to analyze, manage, and interact with Oracle database metadata through the Model Context Protocol (MCP). It provides tools and services for discovering, caching, and analyzing database objects such as tables, views, stored procedures, functions, triggers, indexes, and more.

## Features

- **Metadata Discovery**: Enumerate tables, views, indexes, triggers, stored procedures, and functions
- **Dependency Analysis**: Analyze object dependencies (e.g., which procedures/functions/triggers reference a table)
- **Metadata Caching**: Caches metadata in JSON files for performance with tools to refresh the cache
- **Raw SQL Execution**: Execute raw SQL queries against the Oracle database
- **MCP Integration**: Expose database tools via Model Context Protocol for use with AI agents
- **Extensible Architecture**: Modular design for easy extension and customization

## Projects

- **DatabaseMcp.Core**: Contains interfaces, models, and services for database metadata operations
- **DatabaseMcp.Server**: The main MCP server application that hosts tools for metadata analysis
- **DatabaseMcp.Client**: A simple .NET console client for testing and standalone usage
- **DatabaseMcp.Core.Tests**: Unit tests for the core functionality

## Requirements

- .NET 8.0 SDK or later
- Oracle Database (tested with Oracle 11g and later)
- Oracle.ManagedDataAccess.Core NuGet package

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/oracle-ai-agent.git
cd oracle-ai-agent
```

### 2. Configure Database Connection

Copy the example configuration file and update it with your database details:

```bash
# For the main project
cp appsettings.example.json appsettings.json

# For the server project
cp DatabaseMcp.Server/appsettings.example.json DatabaseMcp.Server/appsettings.json

# For the client project  
cp DatabaseMcp.Client/appsettings.example.json DatabaseMcp.Client/appsettings.json
```

Edit the `appsettings.json` files and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=your-oracle-host)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=your-service-name)));Persist Security Info=True;User Id=your-username;Password=your-password;"
  }
}
```

### 3. Build and Run

```bash
# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run the MCP server
dotnet run --project DatabaseMcp.Server
```

## Claude Desktop Integration

To use this MCP server with Claude Desktop, you'll need to configure Claude to connect to the server.

### Option 1: Development Mode (using dotnet run)

Add this to your Claude Desktop configuration file:

**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "oracle-database-agent": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\path\\to\\your\\oracle-ai-agent\\DatabaseMcp.Server"
      ]
    }
  }
}
```

### Option 2: Using Built Executable

First, publish the application:

```bash
dotnet publish DatabaseMcp.Server -c Release -o ./publish
```

Then configure Claude Desktop:

```json
{
  "mcpServers": {
    "oracle-database-agent": {
      "command": "C:\\path\\to\\your\\oracle-ai-agent\\publish\\DatabaseMcp.Server.exe",
      "args": ["--console"]
    }
  }
}
```

For a complete setup guide, see the example configuration in `.vscode/mcp.example.json`.

## Configuration

The main configuration file is `DatabaseMcp.Server/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-oracle-connection-string-here"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

## Available MCP Tools

The server exposes the following tools for use with AI agents:

- **GetTablesByName**: Retrieve metadata for specific tables
- **GetColumnMetadata**: Get detailed column information for tables
- **GetForeignKeyRelationships**: Analyze foreign key relationships
- **GetPrimaryKeys**: Retrieve primary key information
- **DependentObjectsAnalysis**: Analyze object dependencies
- **ExecuteRawSelect**: Execute SELECT statements
- **RefreshFullDBMetadata**: Refresh cached metadata
- And many more...

## Testing

Run the unit tests to verify everything is working:

```bash
dotnet test
```

## Getting Started

1. **Prerequisites**: Ensure you have .NET 8 SDK installed
2. **Clone**: Clone this repository to your local machine
3. **Configure**: Copy example config files and update with your database details
4. **Build**: Run `dotnet build` to build the solution
5. **Test**: Run `dotnet test` to verify everything works
6. **Run**: Start the MCP server with `dotnet run --project DatabaseMcp.Server`

## Architecture

The solution follows a clean architecture pattern:

- **Core Layer** (`DatabaseMcp.Core`): Contains business logic, interfaces, and models
- **Server Layer** (`DatabaseMcp.Server`): MCP server implementation and tool definitions
- **Client Layer** (`DatabaseMcp.Client`): Console application for testing and standalone usage
- **Tests Layer** (`DatabaseMcp.Core.Tests`): Unit tests for core functionality

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions, please open an issue on the GitHub repository.

## Roadmap

- [ ] Add support for SQL Server databases
- [ ] Add support for PostgreSQL databases
- [ ] Implement more advanced dependency analysis
- [ ] Add database schema comparison tools
- [ ] Improve caching mechanisms
- [ ] Add REST API endpoints

---

**Note**: This project is designed to work with Oracle databases. Ensure you have the appropriate database access and credentials configured before using.