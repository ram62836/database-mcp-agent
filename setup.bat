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

REM Copy example configuration files
echo ⚙️  Setting up configuration files...

if not exist "appsettings.json" (
    copy "appsettings.example.json" "appsettings.json" >nul
    echo ✅ Created appsettings.json
) else (
    echo ⚠️  appsettings.json already exists, skipping...
)

if not exist "DatabaseMcp.Server\appsettings.json" (
    copy "DatabaseMcp.Server\appsettings.example.json" "DatabaseMcp.Server\appsettings.json" >nul
    echo ✅ Created DatabaseMcp.Server\appsettings.json
) else (
    echo ⚠️  DatabaseMcp.Server\appsettings.json already exists, skipping...
)

if not exist "DatabaseMcp.Client\appsettings.json" (
    copy "DatabaseMcp.Client\appsettings.example.json" "DatabaseMcp.Client\appsettings.json" >nul
    echo ✅ Created DatabaseMcp.Client\appsettings.json
) else (
    echo ⚠️  DatabaseMcp.Client\appsettings.json already exists, skipping...
)

if not exist ".vscode\mcp.json" (
    copy ".vscode\mcp.example.json" ".vscode\mcp.json" >nul
    echo ✅ Created .vscode\mcp.json
) else (
    echo ⚠️  .vscode\mcp.json already exists, skipping...
)

REM Run tests
echo 🧪 Running tests...
dotnet test
if %errorlevel% neq 0 (
    echo ⚠️  Some tests failed, but setup can continue
)

echo.
echo 🎉 Setup completed successfully!
echo.
echo Next steps:
echo 1. Update the database connection strings in the appsettings.json files
echo 2. Update .vscode\mcp.json with the correct paths for your system
echo 3. Run the MCP server: dotnet run --project DatabaseMcp.Server
echo.
echo For more information, see the README.md file.

pause
