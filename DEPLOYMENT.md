# 🚀 Deployment Guide

## Automated Builds & Releases

The Oracle Database MCP Agent uses GitHub Actions to automatically build and release cross-platform executables.

## 📦 Build Pipeline Features

### **Automated Builds**
- ✅ **Triggered on**: Push to main, tags, pull requests, manual workflow dispatch
- ✅ **Platforms**: Windows, Linux, macOS
- ✅ **Single-file executables** with all dependencies included
- ✅ **Configuration files** included with each release
- ✅ **Documentation** bundled with executables

### **Release Packages**

Each release includes three platform-specific packages:

| Platform | Package | Executable |
|----------|---------|------------|
| **Windows** | `database-mcp-agent-win-x64.zip` | `DatabaseMcp.Server.exe` |
| **Linux** | `database-mcp-agent-linux-x64.tar.gz` | `DatabaseMcp.Server` |
| **macOS** | `database-mcp-agent-osx-x64.tar.gz` | `DatabaseMcp.Server` |

### **Package Contents**

Each package contains:
```
DatabaseMcp.Server[.exe]         # Main executable
appsettings.json                 # Example configuration
setup.[bat|sh]                   # Platform setup script
README.md                        # Complete documentation
QUICK_REFERENCE.md              # Quick command reference
MCP_TOOLS_GUIDE.md              # Comprehensive tools guide
ORACLE_COMPATIBILITY.md         # Database compatibility
SECURITY.md                     # Security best practices
LICENSE                         # MIT License
```

## 🔄 Creating a Release

### **Automatic Release (Recommended)**
1. Create a new tag following semantic versioning:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. GitHub Actions will automatically:
   - Build for all platforms
   - Run tests
   - Create release packages
   - Publish GitHub release with binaries

### **Manual Build (Development)**
```bash
# Build single-file executable for current platform
dotnet publish DatabaseMcp.Server \
  --configuration Release \
  --self-contained true \
  /p:PublishSingleFile=true \
  /p:IncludeNativeLibrariesForSelfExtract=true \
  /p:EnableCompressionInSingleFile=true
```

## 📋 Build Requirements

### **GitHub Actions Environment**
- ✅ .NET 8.0 SDK
- ✅ Cross-platform runners (Windows, Ubuntu, macOS)
- ✅ GitHub token for releases (automatic)

### **Local Build Requirements**
- ✅ .NET 8.0 SDK or later
- ✅ Git (for version tagging)

## 🎯 Deployment Scenarios

### **1. End-User Deployment**
Users download pre-built releases from GitHub:
1. Download platform-specific package
2. Extract archive
3. Run setup script
4. Configure database connection
5. Launch executable

### **2. Development Deployment**
Developers can build locally:
```bash
git clone https://github.com/ram62836/database-mcp-agent.git
cd database-mcp-agent
./setup.sh  # or setup.bat on Windows
dotnet run --project DatabaseMcp.Server
```

### **3. Enterprise Deployment**
Organizations can:
- Download and redistribute packages internally
- Build from source with custom configurations
- Deploy via corporate software distribution

## 🔧 Build Configuration

### **Single-File Publishing Settings**
```xml
<PublishSingleFile>true</PublishSingleFile>
<SelfContained>true</SelfContained>
<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
<EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
```

### **Platform-Specific Runtime Identifiers**
- **Windows**: `win-x64`
- **Linux**: `linux-x64`
- **macOS**: `osx-x64`

## 📊 Build Artifacts

### **GitHub Actions Artifacts**
- Build artifacts are stored for 30 days
- Available for download from workflow runs
- Include all platform builds

### **Release Assets**
- Permanently stored with GitHub releases
- Automatically generated for tagged releases
- Include comprehensive release notes

## 🛠️ Troubleshooting Builds

### **Common Issues**
1. **Build fails**: Check .NET version and dependencies
2. **Tests fail**: Verify Oracle connectivity in test environment
3. **Publishing fails**: Check runtime identifier and platform compatibility

### **Build Logs**
- View detailed logs in GitHub Actions
- Check individual job outputs for platform-specific issues
- Test artifacts available for debugging

## 📞 Support

For build and deployment issues:
1. Check GitHub Actions workflow logs
2. Review this deployment guide
3. Open an issue with build details
4. Include platform and .NET version information

---

**Next Steps**: After setting up the build pipeline, create your first release by pushing a tag like `v1.0.0`!
