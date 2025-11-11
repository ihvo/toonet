# TooNet Project Guidelines

## Project Overview

TooNet is a high-performance .NET 9.0 serialization library for TOON (Token-Oriented Object Notation) format. TOON achieves 30-60% token reduction compared to JSON while maintaining human readability, making it ideal for Large Language Model (LLM) input.

## TOON spec
@docs/toon-spec-for-agents.md

## Architecture Principles

### Core Design Philosophy
- **Ruthless Simplicity**: Every line of code must justify its existence
- **Performance First**: Direct UTF-8 operations, zero-allocation hot paths, vectorized operations
- **Deterministic Output**: Same input always produces identical output
- **Token Efficiency**: Minimize syntax overhead while maintaining parseability

### Key Components

1. **TooNetSerializer** - High-level API matching System.Text.Json patterns
2. **TooNetWriter** - Low-level writer with stack-allocated ref struct for performance
3. **Internal Utilities**:
   - `PooledBufferWriter` - ArrayPool-based buffer management
   - `IndentationCache` - Pre-computed indentation buffers
   - `QuotingRules` - Efficient string quoting logic
   - `Utf8Constants` - Pre-computed UTF-8 byte patterns

## Coding Standards

### General Rules
- Target Framework: .NET 9.0 (not 8.0)
- Language Version: C# 12
- Nullable reference types enabled
- Treat warnings as errors
- No XML documentation warnings (`<GenerateDocumentationFile>false</GenerateDocumentationFile>`)

### Naming Conventions
```csharp
// Classes and Methods: PascalCase
public class TooNetWriter
public void WriteString()

// Private fields: camelCase
private readonly byte[] buffer;

// Parameters and local variables: camelCase
public void WriteNumber(long value)
var result = Calculate();

// Constants: PascalCase
public const int MaxDepth = 64;

// Test methods: MethodNameScenarioExpectedResult
[Fact]
public void SerializeSimpleObjectReturnsValidToon()
```

### Performance Patterns

```csharp
// GOOD: Direct UTF-8 operations
ReadOnlySpan<byte> utf8Bytes = "hello"u8;
writer.WriteRaw(utf8Bytes);

// BAD: String concatenation
string result = "hello" + world;

// GOOD: ArrayPool for temporary buffers
byte[] buffer = ArrayPool<byte>.Shared.Rent(1024);
try { /* use buffer */ }
finally { ArrayPool<byte>.Shared.Return(buffer); }

// GOOD: Ref struct for zero-allocation
public ref struct TooNetWriter { }

// GOOD: Pre-computed constants
private static readonly byte[] CommaBytes = ","u8.ToArray();
```

### Testing Patterns

```csharp
// Use xUnit with Fact and Theory attributes
[Fact]
public void SimpleTest() { }

[Theory]
[InlineData(true, "true")]
[InlineData(false, "false")]
public void ParameterizedTest(bool input, string expected) { }

// Group related tests with nested classes
public class TooNetSerializerTests
{
    public class SerializeMethod
    {
        [Fact]
        public void HandlesNull() { }
    }
}
```

## TOON Format Rules

### Key Formatting Rules
1. **Indentation**: Exactly 2 spaces per level (never tabs)
2. **No trailing spaces** on any line
3. **No trailing newline** at document end
4. **Separator spacing**:
   - Primitives: `key: value` (space after colon)
   - Nested/empty: `key:` (no space)

### Array Formats
```toon
# Empty array
items[0]:

# Inline (primitives only)
tags[3]: reading,gaming,coding

# Tabular (uniform objects)
users[3]{id,name,active}:
  1,Alice,true
  2,Bob,false
  3,Charlie,true

# List (non-uniform)
items[3]:
  - 1
  - name: Widget
  - simple text
```

### Quoting Rules
Quote strings ONLY when they:
- Are empty (`""`)
- Have leading/trailing whitespace
- Contain delimiter or colon
- Look like boolean/number/null
- Start with `"- "` (list marker)
- Look like structural tokens (`"[5]"`)

## Build & Test

### Building
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/TooNet/TooNet.csproj

# Release build
dotnet build -c Release
```

### Testing
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/TooNet.Tests
```

### Running Benchmarks

```bash
# Run all benchmarks (full BenchmarkDotNet suite)
dotnet run -c Release --project benchmarks/TooNet.Benchmarks

# Quick performance check
dotnet run -c Release --project benchmarks/TooNet.Benchmarks -- --quick

# Token reduction analysis
dotnet run -c Release --project benchmarks/TooNet.Benchmarks -- --analyze-tokens

# Compare TOON libraries
dotnet run -c Release --project benchmarks/TooNet.Benchmarks -- --compare

# Safe comparison (handles errors gracefully)
dotnet run -c Release --project benchmarks/TooNet.Benchmarks -- --safe-compare

# Generate comprehensive reports
dotnet run -c Release --project benchmarks/TooNet.Benchmarks -- --generate-report

# Filter specific benchmarks
dotnet run -c Release --project benchmarks/TooNet.Benchmarks -- --filter *Tabular*

# Quick job for faster results
dotnet run -c Release --project benchmarks/TooNet.Benchmarks -- --job short
```

### Benchmark Categories
- **SimpleObjectBenchmarks**: Single objects and small collections
- **TabularDataBenchmarks**: Product catalogs, metrics, uniform arrays
- **ComplexStructureBenchmarks**: Nested objects, mixed types
- **DelimiterComparisonBenchmarks**: Performance with different delimiters
- **ArrayFormatBenchmarks**: Different array formatting modes
- **ThroughputBenchmarks**: High-frequency serialization
- **ToonLibrariesComparison**: Compare TooNet vs other TOON implementations

## Project Structure

```
toonet/
├── src/
│   ├── TooNet/                 # Main library
│   │   ├── Internal/           # Internal utilities
│   │   ├── TooNetSerializer.cs # High-level API
│   │   ├── TooNetWriter.cs     # Low-level writer
│   │   └── TooNetWriter.*.cs   # Writer extensions
│   │
│   └── [Other TOON libraries]  # Alternative implementations for comparison
│
├── tests/
│   └── TooNet.Tests/           # Unit tests
│       ├── Internal/           # Internal component tests
│       └── *.Tests.cs          # Feature tests
│
├── benchmarks/
│   └── TooNet.Benchmarks/      # Performance benchmarks
│       ├── Benchmarks/         # Benchmark implementations
│       ├── Config/             # BenchmarkDotNet configuration
│       ├── Data/               # Test data generators
│       ├── Models/             # Test models
│       ├── Reporting/          # Report generation
│       └── Utils/              # Benchmark utilities
│
├── docs/
│   ├── toon-specification.md   # Full TOON spec
│   └── toon-spec-for-agents.md # Simplified spec for AI
│
└── scratch/                     # Working notes and specs
```

## Performance Targets

- **Token Reduction**: 30-60% for tabular data vs JSON
- **Speed**: Within 2x of System.Text.Json
- **Memory**: Equal or better allocations than Newtonsoft.Json
- **Throughput**: >100MB/s for large collections

## Common Tasks

### Adding a New Feature
1. Update specs in `scratch/` if needed
2. Write tests first (TDD approach)
3. Implement with performance in mind
4. Run benchmarks to verify no regression
5. Update documentation

### Optimizing Performance
1. Profile with `dotnet trace` or Visual Studio Profiler
2. Identify hot paths with benchmarks
3. Apply optimizations (ArrayPool, Span<T>, vectorization)
4. Measure impact with benchmarks
5. Document performance-critical sections

### Debugging Serialization Issues
1. Use test project to isolate issue
2. Check TOON spec compliance
3. Verify delimiter and quoting rules
4. Compare with expected output
5. Add regression test

## Important Notes

### Current Limitations
- Nested objects in arrays not fully supported (throws TooNetException)
- Deserialization not yet implemented (serialization only)
- No source generation yet (reflection-based)

### Performance Considerations
- Pre-allocate collections with known capacity
- Use ArrayPool<T> for temporary buffers
- Minimize string allocations in hot paths
- Cache frequently used data in setup
- Direct UTF-8 operations whenever possible

### Testing Philosophy
- Unit tests for core functionality
- Integration tests for serialization scenarios
- Benchmarks for performance validation
- Focus on edge cases and TOON spec compliance

## Version History
- 1.0.0 - Initial release with core serialization
- Future: Add deserialization, source generation, streaming improvements

## Contributing Guidelines
1. Follow existing code patterns
2. Include tests for new features
3. Run benchmarks to ensure no regression
4. Update this document if adding new patterns
5. Keep it simple - complexity must be justified