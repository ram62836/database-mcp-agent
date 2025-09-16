@echo off
REM Oracle Database MCP Agent Setup Script for Windows
REM This script helps you set up the Oracle Database MCP Agent for development

echo 🚀 Oracle Database MCP Agent Setup
echo ==================================

REM Check if .NET 8 is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET 8 SDK is required but not installed.
    echo Please install .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0
    exit /b 1
)

REM Check .NET version
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo ✅ Found .NET version: %DOTNET_VERSION%

REM Restore packages
echo 📦 Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ❌ Failed to restore packages
    exit /b 1
)

REM Build the solution
echo 🔨 Building the solution...
dotnet build
if %errorlevel% neq 0 (
    echo ❌ Failed to build solution
    exit /b 1
)

REM Create metadata and logs directories
echo 📁 Creating required directories...
mkdir "%USERPROFILE%\database-mcp-agent\metadata" 2>nul
mkdir "%USERPROFILE%\database-mcp-agent\logs" 2>nul
echo ✅ Created database-mcp-agent directories

REM Run tests
echo 🧪 Running tests...
dotnet test
if %errorlevel% neq 0 (
    echo ⚠️  Some tests failed, but setup can continue
)

REM Create a package
echo 📦 Creating NuGet package...
dotnet pack DatabaseMcp.Server -o nupkg
if %errorlevel% neq 0 (
    echo ⚠️  Package creation failed, but setup can continue
)

echo.
echo 🎉 Setup completed successfully!
echo.
echo Next steps:
echo 1. Install the package: dotnet tool install --global --add-source nupkg Hala.DatabaseAgent.OracleMcpServer
echo 2. Set environment variables for database connection:
echo    $env:OracleConnectionString="your-connection-string"
echo    $env:LogFilePath="%USERPROFILE%\database-mcp-agent\logs"
echo    $env:SchemaOwner="your-schema-name"  REM Required: Schema owner for database operations
echo 3. Run the MCP server: oracle-mcp-server
echo.
echo For more information, see the README.md file.

pause
