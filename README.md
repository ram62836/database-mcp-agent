# MCP SDK Solution

## Overview
The MCP SDK Solution is a .NET-based library designed to facilitate database metadata retrieval and management. It provides a set of services that allow users to discover tables, retrieve column metadata, identify keys, list indexes, gather constraints, enumerate views, list synonyms, and retrieve stored procedures and functions.

## Project Structure
The solution consists of the following projects:

- **McpSdkLibrary**: This project contains the core library with services and models for database metadata operations.
- **McpSdkApp**: This project serves as the application entry point, utilizing the services provided by the McpSdkLibrary.

## Features
- **Table Discovery**: Retrieve all user-defined tables in the database.
- **Column Metadata Retrieval**: Fetch details about columns, including names, data types, nullability, and default values.
- **Primary and Foreign Key Identification**: Identify primary keys and foreign key relationships within the database.
- **Index Listing**: List all indexes and their associated columns.
- **Constraint Gathering**: Gather information on unique constraints and check constraints.
- **View Enumeration**: Enumerate views and their definitions.
- **Synonym Listing**: List synonyms and their base objects.
- **Stored Procedure and Function Retrieval**: Retrieve stored procedures and functions along with their parameters.

## Setup Instructions
1. Clone the repository to your local machine.
2. Open the solution file `McpSdkSolution.sln` in your preferred .NET development environment.
3. Restore the NuGet packages by running the following command in the Package Manager Console:
   ```
   dotnet restore
   ```
4. Build the solution to ensure all components are correctly set up:
   ```
   dotnet build
   ```

## Usage
To use the services provided by the MCP SDK, instantiate the desired service in your application and call the appropriate methods. For example:

```csharp
var tableDiscoveryService = new TableDiscoveryService();
var tables = tableDiscoveryService.GetAllUserDefinedTables();
```

Refer to the individual service classes for detailed method descriptions and usage examples.

## Contributing
Contributions are welcome! Please submit a pull request or open an issue for any enhancements or bug fixes.

## License
This project is licensed under the MIT License. See the LICENSE file for more details.