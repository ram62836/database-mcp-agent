# install-dotnet10.ps1
Write-Host "Installing .NET 10 SDK..." -ForegroundColor Cyan

# Download the .NET 10 SDK installer
$downloadUrl = "https://dotnetcli.azureedge.net/dotnet/Sdk/10.0.100-preview.3.24174.11/dotnet-sdk-10.0.100-preview.3.24174.11-win-x64.exe"
$installerPath = "$env:TEMP\dotnet-installer.exe"

Write-Host "Downloading .NET 10 SDK installer..."
Invoke-WebRequest -Uri $downloadUrl -OutFile $installerPath

# Run the installer
Write-Host "Running installer..."
Start-Process -FilePath $installerPath -ArgumentList "/quiet" -Wait

# Verify installation
Write-Host "Verifying installation..."
dotnet --version

Write-Host "Installation complete. Please restart your terminal or VS Code." -ForegroundColor Green
