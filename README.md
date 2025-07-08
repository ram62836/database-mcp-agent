# Oracle AI Agent

## Overview

Oracle AI Agent is a .NET solution designed to analyze, manage, and interact with Oracle database metadata. It provides tools and services for discovering, caching, and analyzing database objects such as tables, views, stored procedures, functions, triggers, indexes, and more. The solution is modular, with a core library, an application server, and a client.

## Projects

- **OracleAgent.Core**: Contains interfaces, models, and services for Oracle database metadata operations (table discovery, index listing, relationship analysis, etc.).
- **OracleAgent.App**: The main application server. Hosts tools for metadata analysis, dependency analysis, and cache refresh. Uses dependency injection and configuration via `appsettings.json`.
- **OracleAgent.Client**: A simple .NET console client - WIP.

## Features

- **Metadata Discovery**: Enumerate tables, views, indexes, triggers, stored procedures, and functions.
- **Dependency Analysis**: Analyze object dependencies (e.g., which procedures/functions/triggers reference a table).
- **Metadata Caching**: Caches metadata in JSON files for performance. Tools are provided to refresh the cache for all or specific object types.
- **Raw SQL Execution**: Execute raw SQL queries against the Oracle database.
- **Extensible Tooling**: Tools are exposed via the application server for integration with LLM's in agent mode.

## Configuration

The main configuration file is `OracleAgentApp/appsettings.json`:
{
  "ConnectionStrings": {
    "DefaultConnection": "<your-oracle-connection-string>"
  },
  ...
}
Update the `DefaultConnection` string to point to your Oracle database.

## Getting Started

1. **.NET 8 SDK** is required.
2. Restore NuGet packages:dotnet restore3. Build the solution:dotnet build4. Update `appsettings.json` with your Oracle connection details.
5. Run the application server:dotnet run --project OracleAgentApp/OracleAgent.App.csproj
## Main Components

- **Services**: Implementations for metadata discovery, relationship analysis, and more (see `OracleAgentCore/Services`).
- **Tools**: Application-level tools for metadata refresh, dependency analysis, and raw SQL (see `OracleAgentApp/Tools`).
- **Models**: Data models for Oracle metadata (see `OracleAgentCore/Models`).

## Extending

- Add new tools in `OracleAgentApp/Tools`.
- Implement new services in `OracleAgentCore/Services` and register them in `OracleAgentApp/Program.cs`.

## License

This project is intended for internal or demonstration use. Please ensure you have the appropriate Oracle database access and credentials.