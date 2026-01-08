# FetchGrxml

A high-performance .NET tool for extracting grammar (.grxml) files from production Business Units across multiple FileServer clusters.

## Features

- 🚀 **Multi-cluster support**: Fetch from multiple FileServer clusters in parallel
- ⚡ **Concurrent downloads**: 2 concurrent requests per cluster with intelligent throttling
- 🎯 **Pattern-based exclusions**: Skip unwanted directories with glob patterns
- 📁 **Organized output**: Files organized by cluster and Business Unit (`cluster/BUSxxxx/`)
- 🧪 **Comprehensive testing**: 40+ unit tests with 100% core logic coverage
- 🔒 **Type-safe**: Built with C# 12, nullable reference types enabled
- 🌍 **Cross-platform**: Works on Windows, Linux, and macOS

## Requirements

- .NET 8.0 SDK (for building)
- Access to FileServerCoreSDK v10.0.0 (internal NuGet package)
  - For CI/CD without access: Use the included stub (see below)

## Quick Start

### Build from Source
```bash
dotnet restore
dotnet build
dotnet test
```

### Publish Single-File Executable
```bash
# Windows
dotnet publish -c Release -r win-x64 -o publish

# Linux
dotnet publish -c Release -r linux-x64 -o publish

# macOS
dotnet publish -c Release -r osx-x64 -o publish
```

## Usage

```bash
FetchGrxml <bus-csv> <cluster-config-dir> [exclusions-file] [output-dir]
```

### Arguments

1. **bus-csv** (required): CSV file with BU numbers and clusters
2. **cluster-config-dir** (required): Directory containing `.cluster.properties` files
3. **exclusions-file** (optional): File with directory exclusion patterns
4. **output-dir** (optional): Output directory (default: `./output`)

### Example

```bash
FetchGrxml bus.csv clusters/ exclusions.txt ./grammars
```

## Configuration Files

### Business Units CSV (`bus.csv`)

Maps Business Units to FileServer clusters. Supports single or multiple clusters per BU:

```csv
# Single cluster
12345,cluster3
67890,cluster1

# Multiple clusters (quoted, comma-separated)
11111,"cluster1,cluster2"
22222,"cluster3,cluster1,cluster2"
```

### Cluster Configuration (`.cluster.properties`)

Each cluster requires a properties file: `<cluster-name>.cluster.properties`

Example `clusters/cluster1.cluster.properties`:
```properties
file_server_configuration.FsIpAddress = fileserver-cluster1.example.com
```

### Exclusion Patterns (`exclusions.txt`)

⚠️ **This is NOT Perl regex!** This is a custom pattern matcher I implemented. Patterns are case-sensitive.

**How it works:**
- All file paths start with `/` (e.g., `/temp/file.grxml`, `/logs/debug.grxml`)
- Without `*`: Exact match or prefix match (requires `/` after pattern)
- With `*`: Wildcard matching (converts to regex internally)

**Common pitfalls:**
- `temp` - **USELESS!** Never matches because paths start with `/`, not `temp`
- `/temp` - Matches `/temp` and `/temp/anything`, but NOT `/logs/temp` 
- `*/temp` - Matches any path ending with `/temp` (like `/logs/temp`)
- `*temp*` - Matches any path containing "temp" anywhere

**Examples:**
```
# Skip /temp directory at root (includes /temp/subfolder)
/temp

# Skip any directory named logs at any level
*/logs

# Skip .cache folders anywhere
*/.cache

# Case-sensitive! /Temp won't match /temp
/Temp

# Comments start with #
```

**Why `/temp` vs `temp` matters:**
- All paths are absolute from BU root: `/temp/file.grxml`
- Pattern `temp` expects path to start with `temp` or `temp/` → never matches
- Pattern `/temp` expects `/temp` or `/temp/` → matches correctly
- This caught me by surprise with lsync, so documenting clearly here!

## Output Structure

Files are organized by cluster and Business Unit:

```
output/
├── cluster1/
│   ├── BUS12345/
│   │   ├── grammar1.grxml
│   │   └── grammar2.grxml
│   └── BUS11111/
│       └── grammar3.grxml
├── cluster2/
│   └── BUS12345/
│       └── grammar4.grxml
└── cluster3/
    └── BUS67890/
        └── grammar5.grxml
```

## Architecture

### Concurrency Model
- **Per-cluster**: Up to 2 concurrent FileServer requests (throttled)
- **Cross-cluster**: All clusters process in parallel
- **Example**: 3 clusters = up to 6 concurrent requests (2 per cluster)

### Key Components
- **ConfigHelper**: Parses CSV and properties files
- **WorkflowOrchestrator**: Coordinates BU processing
- **FileScanner**: Recursively scans for .grxml files
- **FileDownloader**: Parallel file downloads with retry
- **RequestThrottler**: Semaphore-based concurrency control
- **ExclusionMatcher**: Pattern-based directory filtering

## CI/CD Without Internal NuGet Access

The `FileServerCoreSDK.Stub` project provides a non-functional stub for builds without access to the internal FileServerCoreSDK package.

### Using the Stub

```bash
# Pack the stub
dotnet pack FileServerCoreSDK.Stub -c Debug -o ./local-packages

# Add as local package source
dotnet nuget add source ./local-packages --name LocalStub

# Build (uses stub instead of real SDK)
dotnet build
dotnet test
```

## Development

### Project Structure
```
FetchGrxml/
├── FetchGrxml/              # Main application
│   ├── Core/                # Business logic
│   ├── Config/              # Configuration parsing
│   └── UI/                  # Console output
├── FetchGrxml.Tests/        # Unit tests (xUnit)
└── FileServerCoreSDK.Stub/  # CI/CD stub package
```

### Running Tests
```bash
dotnet test
```

### Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Contributing

1. Ensure all tests pass: `dotnet test`
2. Maintain cross-platform compatibility
3. Follow existing code style (nullable enabled, implicit usings)
4. Add tests for new features

## License

Internal use only.
