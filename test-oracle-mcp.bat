@echo off
REM Script to start the Oracle Database Agent MCP Server for testing

echo Starting Oracle Database Agent MCP Server...
echo.
echo This will start the server on port 5123
echo Press Ctrl+C to stop the server
echo.

dotnet run --project src/MCP/DatabaseAgent.Oracle.MCP/DatabaseAgent.Oracle.MCP.csproj

echo.
echo Server stopped.
