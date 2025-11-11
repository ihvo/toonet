# TooNet Development Container

This devcontainer provides a sandboxed .NET 9.0 development environment with restricted networking, similar to the Claude Code project architecture.

## Features

- **Isolated Environment**: Complete .NET 9.0 SDK with development tools
- **Restricted Networking**: Firewall rules allow only essential services
- **Persistent Storage**: NuGet cache and command history persist across container rebuilds
- **Development Tools**: Git, GitHub CLI, Delta diff viewer, Zsh with oh-my-zsh

## Quick Start

1. **Open in VS Code**:
   - Install the Dev Containers extension
   - Open this repository in VS Code
   - When prompted, click "Reopen in Container"

2. **Manual Build**:
   ```bash
   # From repository root
   docker build -t toonet-dev .devcontainer
   ```

## Network Access

The firewall restricts outbound connections to essential services only:

### Allowed Domains
- **NuGet Services**: `api.nuget.org`, `www.nuget.org`, `globalcdn.nuget.org`
- **Microsoft/Azure**: .NET CLI downloads, VS Code marketplace
- **GitHub**: Repository access, releases, raw content
- **Local Network**: Host machine and container networking

### Verification

After container startup, the firewall script verifies:
- ✅ Cannot reach `https://example.com` (blocked)
- ✅ Can reach `https://api.nuget.org` (allowed)
- ✅ Can reach `https://api.github.com` (allowed)

## Development Workflow

### Building TooNet
```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run benchmarks
dotnet run -c Release --project benchmarks/TooNet.Benchmarks
```

### Package Management
```bash
# Add package (will use allowed NuGet sources)
dotnet add package Newtonsoft.Json

# List packages
dotnet list package

# Update packages
dotnet restore --force
```

## Container Configuration

### VS Code Extensions
- **C# Dev Kit**: Full .NET development support
- **GitLens**: Enhanced Git integration
- **Prettier**: Code formatting

### Environment Variables
- `DOTNET_CLI_TELEMETRY_OPTOUT=1`: Disable telemetry
- `DOTNET_NOLOGO=1`: Hide .NET logo
- `DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1`: Skip welcome messages
- `NUGET_XMLDOC_MODE=skip`: Faster package operations

### Persistent Volumes
- **NuGet Cache**: `/home/dotnet/.nuget` - Package cache survives rebuilds
- **Command History**: `/commandhistory` - Bash/Zsh history preservation

## Security Features

### Network Restrictions
- **Default Policy**: DROP all traffic
- **Allowlist Approach**: Only pre-approved domains accessible
- **Local Development**: Host network access for debugging

### Container Privileges
- **NET_ADMIN**: Required for firewall management
- **NET_RAW**: Required for iptables operations
- **Non-root User**: Development runs as `dotnet` user

## Troubleshooting

### Network Issues
```bash
# Check firewall status
sudo iptables -L -n

# View allowed domains
sudo ipset list allowed-domains

# Test specific domain
curl -v https://api.nuget.org/v3/index.json
```

### .NET Issues
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Verify .NET installation
dotnet --info

# Check package sources
dotnet nuget list source
```

### Container Issues
```bash
# Rebuild container
Ctrl+Shift+P → "Dev Containers: Rebuild Container"

# View container logs
docker logs <container-id>

# Access container shell
docker exec -it <container-id> /bin/zsh
```

## Customization

### Adding Allowed Domains
Edit `.devcontainer/init-firewall.sh` and add domains to the resolution loop:

```bash
for domain in \
    "your-domain.com" \
    "another-domain.com"; do
    # ... resolution logic
done
```

### Additional Tools
Modify `.devcontainer/Dockerfile` to install additional packages:

```dockerfile
RUN apt-get update && apt-get install -y \
    your-package \
    another-package
```

## Performance

- **Container Size**: ~1.2GB (based on microsoft/dotnet:9.0-sdk)
- **Startup Time**: ~30-60 seconds (includes firewall setup)
- **Build Performance**: Comparable to host .NET performance

## Architecture

```
Container Network Security
├── Docker DNS (127.0.0.11) ✅ Preserved
├── Localhost (lo) ✅ Always allowed
├── Host Network ✅ Development access
├── Allowed Domains ✅ Via ipset whitelist
└── Everything Else ❌ Explicitly rejected
```

This setup ensures a secure, reproducible development environment while maintaining full .NET development capabilities.