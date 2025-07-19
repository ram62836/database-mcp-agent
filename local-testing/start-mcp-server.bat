@echo off
REM Manual MCP Server Test Script
REM This will start the MCP server manually to test if it's working

echo Starting Hala.DatabaseMcpAgent MCP Server...
echo.

echo Setting environment variables...
set "ConnectionStrings__OracleConnection=Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.180.75)(PORT=1522))(CONNECT_DATA=(SERVICE_NAME=eneadv19)));Persist Security Info=True;User Id=engspl4;Password=engspl4;"
set "DatabaseMcp__CacheExpirationMinutes=30"
set "DatabaseMcp__MaxConnectionRetries=3"
set "Logging__LogLevel__Default=Information"

echo.
echo Starting MCP server in stdio mode...
echo Press Ctrl+C to stop the server
echo.

database-mcp-agent stdio
