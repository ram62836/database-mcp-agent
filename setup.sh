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

# Copy example configuration files
echo "⚙️  Setting up configuration files..."

if [ ! -f "appsettings.json" ]; then
    cp "appsettings.example.json" "appsettings.json"
    echo "✅ Created appsettings.json"
else
    echo "⚠️  appsettings.json already exists, skipping..."
fi

if [ ! -f "DatabaseMcp.Server/appsettings.json" ]; then
    cp "DatabaseMcp.Server/appsettings.example.json" "DatabaseMcp.Server/appsettings.json"
    echo "✅ Created DatabaseMcp.Server/appsettings.json"
else
    echo "⚠️  DatabaseMcp.Server/appsettings.json already exists, skipping..."
fi

if [ ! -f "DatabaseMcp.Client/appsettings.json" ]; then
    cp "DatabaseMcp.Client/appsettings.example.json" "DatabaseMcp.Client/appsettings.json"
    echo "✅ Created DatabaseMcp.Client/appsettings.json"
else
    echo "⚠️  DatabaseMcp.Client/appsettings.json already exists, skipping..."
fi

if [ ! -f ".vscode/mcp.json" ]; then
    cp ".vscode/mcp.example.json" ".vscode/mcp.json"
    echo "✅ Created .vscode/mcp.json"
else
    echo "⚠️  .vscode/mcp.json already exists, skipping..."
fi

# Run tests
echo "🧪 Running tests..."
dotnet test

echo ""
echo "🎉 Setup completed successfully!"
echo ""
echo "Next steps:"
echo "1. Update the database connection strings in the appsettings.json files"
echo "2. Update .vscode/mcp.json with the correct paths for your system"
echo "3. Run the MCP server: dotnet run --project DatabaseMcp.Server"
echo ""
echo "For more information, see the README.md file."
