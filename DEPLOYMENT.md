# 🚀 Deployment Guide

## ⚡ **Getting Started (Users)**

**👉 Most users should download pre-built releases instead of building from source.**

### **Download Pre-Built Releases**
1. **Go to releases**: [GitHub Releases](https://github.com/ram62836/database-mcp-agent/releases/latest)
2. **Download for your platform**:
   - Windows: `database-mcp-agent-win-x64.zip`
   - macOS: `database-mcp-agent-osx-x64.tar.gz`
3. **Extract and run**: Use the included setup scripts
4. **Configure**: Edit `appsettings.json` with your database connection
5. **Run**: Execute `DatabaseMcp.Server.exe` (Windows) or `./DatabaseMcp.Server` (macOS)

**✅ No compilation, no dependencies, no .NET installation required!**

---

## 🏗️ **For Developers: Build Pipeline**

The Oracle Database MCP Agent uses GitHub Actions to automatically build and release cross-platform executables.

### **Automated Build Features**
- ✅ **Triggered on**: Push to main, tags, pull requests, manual workflow dispatch
- ✅ **Platforms**: Windows and macOS
- ✅ **Single-file executables** with all dependencies included
- ✅ **Configuration files** included with each release
- ✅ **Documentation** bundled with executables

### **Release Packages**

Each release includes two platform-specific packages:

| Platform | Package | Executable |
|----------|---------|------------|
| **Windows** | `database-mcp-agent-win-x64.zip` | `DatabaseMcp.Server.exe` |
| **macOS** | `database-mcp-agent-osx-x64.tar.gz` | `DatabaseMcp.Server` |

### **Package Contents**

Each package contains:
```
DatabaseMcp.Server[.exe]         # Main executable (self-contained)
appsettings.json                 # Example configuration
setup.[bat|sh]                   # Platform setup script
README.md                        # Complete documentation
QUICK_REFERENCE.md              # Quick command reference
MCP_TOOLS_GUIDE.md              # Comprehensive tools guide
ORACLE_COMPATIBILITY.md         # Database compatibility
SECURITY.md                     # Security best practices
LICENSE                         # MIT License
```

## 🔄 **Creating a Release**

### **Automatic Release (Recommended)**
1. Create a new tag following semantic versioning:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. GitHub Actions will automatically:
   - Build for Windows and macOS platforms
   - Run tests
   - Create release packages
   - Publish GitHub release with binaries and comprehensive release notes

### **Manual Build (Development Only)**
```bash
# Build single-file executable for current platform
dotnet publish DatabaseMcp.Server \
  --configuration Release \
  --self-contained true \
  --runtime win-x64 \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true
```

## 📋 **Build Requirements**

### **For Users: None!**
- ✅ **Pre-built releases** available on GitHub
- ✅ **Self-contained executables** - no .NET installation needed
- ✅ **No compilation** required

### **For Developers: Local Build**
- ✅ .NET 8.0 SDK or later
- ✅ Git (for version tagging)
- ✅ Oracle database for testing

### **GitHub Actions Environment**
- ✅ .NET 8.0 SDK
- ✅ Cross-platform runners (Windows, macOS)
- ✅ GitHub token for releases (automatic)

## 🎯 **Deployment Scenarios**

### **1. End-User Deployment (Recommended)**
**Download ready-to-use releases:**
1. **Download**: Go to [GitHub Releases](https://github.com/ram62836/database-mcp-agent/releases/latest)
2. **Extract**: Unzip the platform-specific package
3. **Setup**: Run `setup.bat` (Windows) or `./setup.sh` (macOS)
4. **Configure**: Edit `appsettings.json` with your database connection
5. **Run**: Launch `DatabaseMcp.Server.exe` or `./DatabaseMcp.Server`

**✅ Zero compilation, zero dependencies!**

### **2. Development Deployment**
**For developers who want to build from source:**
```bash
git clone https://github.com/ram62836/database-mcp-agent.git
cd database-mcp-agent
dotnet restore
dotnet build
dotnet run --project DatabaseMcp.Server
```

### **3. Enterprise Deployment**
**Organizations can:**
- Download and redistribute pre-built packages internally
- Build from source with custom configurations
- Deploy via corporate software distribution systems
- Use in Docker containers or server environments

## 🔧 **Build Configuration Details**

### **Single-File Publishing Settings**
```xml
<PublishSingleFile>true</PublishSingleFile>
<SelfContained>true</SelfContained>
<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
<EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
```

### **Platform-Specific Runtime Identifiers**
- **Windows**: `win-x64` (64-bit Intel/AMD)
- **macOS**: `osx-x64` (64-bit Intel/AMD and Apple Silicon via Rosetta)

### **Supported Platforms**
| Platform | Architecture | Executable Format | Status |
|----------|-------------|-------------------|--------|
| Windows 10+ | x64 | `.exe` | ✅ Fully Supported |
| macOS 10.15+ | x64/ARM64 | Binary | ✅ Fully Supported |

## 📊 **Build Artifacts & Storage**

### **GitHub Actions Artifacts**
- Build artifacts stored for 30 days
- Available for download from workflow runs
- Include logs and debug information

### **Release Assets**
- **Permanently stored** with GitHub releases
- **Automatically generated** for tagged releases
- **Comprehensive release notes** with setup instructions

### **File Sizes**
- Windows executable: ~15-20 MB (self-contained)
- macOS executable: ~15-20 MB (self-contained)
- Documentation: ~200 KB total

## 🛠️ **Troubleshooting**

### **For Users: Download Issues**
| Issue | Solution |
|-------|----------|
| File won't download | ✅ Check internet connection, try different browser |
| Executable won't run | ✅ Check antivirus settings, ensure file permissions |
| Configuration errors | ✅ Follow setup scripts, verify database credentials |

### **For Developers: Build Issues**
| Issue | Solution |
|-------|----------|
| Build fails | ✅ Check .NET 8.0 SDK installation |
| Tests fail | ✅ Verify Oracle database connectivity |
| Publishing fails | ✅ Check runtime identifier and platform |

### **Getting Help**
1. **Check documentation** in this repository
2. **Review logs** in GitHub Actions for build failures
3. **Open an issue** with detailed error information
4. **Provide context**: OS version, .NET version, error messages

---

**🚀 Users: Download releases • Developers: Build from source • Everyone: Get Oracle database intelligence with AI!**
- Test artifacts available for debugging

## 📞 Support

For build and deployment issues:
1. Check GitHub Actions workflow logs
2. Review this deployment guide
3. Open an issue with build details
4. Include platform and .NET version information

---

**Next Steps**: After setting up the build pipeline, create your first release by pushing a tag like `v1.0.0`!
