#!/bin/bash
echo "Installing .NET 10 SDK..."

# Download and install the .NET 10 SDK
curl -sSL https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0 --quality preview

# Add to PATH if needed
if [[ ":$PATH:" != *":$HOME/.dotnet:"* ]]; then
  echo 'export PATH="$PATH:$HOME/.dotnet"' >> ~/.zshrc
  echo 'export PATH="$PATH:$HOME/.dotnet"' >> ~/.bash_profile
  export PATH="$PATH:$HOME/.dotnet"
fi

# Verify installation
echo "Verifying installation..."
dotnet --version

echo "Installation complete. Please restart your terminal or VS Code."
