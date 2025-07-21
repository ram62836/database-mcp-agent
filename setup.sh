#!/bin/bash

# Oracle Database MCP Agent Setup Script
# This script helps you set up the Oracle Database MCP Agent for development

set -e

echo "🚀 Oracle Database MCP Agent Setup"
echo "=================================="

# Check if .NET 8 is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET 8 SDK is required but not installed."
    echo "Please install .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "✅ Found .NET version: $DOTNET_VERSION"

# Restore packages
echo "📦 Restoring NuGet packages..."
dotnet restore

# Build the solution
echo "🔨 Building the solution..."
dotnet build

# Create metadata and logs directories
echo "📁 Creating required directories..."
mkdir -p "$HOME/database-mcp-agent/metadata"
mkdir -p "$HOME/database-mcp-agent/logs"
echo "✅ Created database-mcp-agent directories"

# Run tests
echo "🧪 Running tests..."
dotnet test

# Create a package
echo "📦 Creating NuGet package..."
dotnet pack DatabaseMcp.Server -o nupkg

echo ""
echo "🎉 Setup completed successfully!"
echo ""
echo "Next steps:"
echo "1. Install the package: dotnet tool install --global --add-source nupkg Hala.DatabaseAgent.OracleMcpServer"
echo "2. Set environment variables for database connection:"
echo "   export OracleConnectionString=\"your-connection-string\""
echo "   export MetadataCacheJsonPath=\"$HOME/database-mcp-agent/metadata\""
echo "   export LogFilePath=\"$HOME/database-mcp-agent/logs\""
echo "3. Run the MCP server: oracle-mcp-server"
echo ""
echo "For more information, see the README.md file."
