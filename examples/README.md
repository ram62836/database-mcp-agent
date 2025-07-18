# Configuration Examples

This directory contains example configuration files for integrating the Oracle Database MCP Agent with different AI assistants.

## 📁 Files

### **claude-desktop-config.json**
- **Purpose**: Configuration for Claude Desktop integration
- **Usage**: Copy to your Claude Desktop config directory
- **Locations**:
  - Windows: `%APPDATA%\Claude\claude_desktop_config.json`
  - macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`

### **vscode-settings.json**
- **Purpose**: VS Code settings.json configuration for GitHub Copilot integration
- **Usage**: Add the `mcp.servers` section to your VS Code settings.json
- **Requirements**: 
  - GitHub Copilot extension installed
  - MCP Client for VS Code extension installed

## 🔧 Setup Process

1. **Download the latest release** from [GitHub Releases](https://github.com/ram62836/database-mcp-agent/releases/latest)
2. **Extract** to a folder (e.g., `C:\Tools\database-mcp-agent`)
3. **Choose your integration**:
   - For Claude Desktop: Use `claude-desktop-config.json`
   - For VS Code: Use `vscode-settings.json`
4. **Update paths** in the configuration to match your installation
5. **Restart** the respective application

## 💡 Tips

- Always update the `command` and `cwd` paths to match your actual installation directory
- Ensure the `appsettings.json` file with your Oracle connection string is in the same directory as the executable
- Check the Output panel in VS Code or Claude Desktop logs if connections fail
