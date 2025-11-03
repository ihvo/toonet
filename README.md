# TooNet

High-performance .NET serialization library for TOON (Token-Oriented Object Notation) format.

## Overview

TooNet is a .NET 9.0+ library that provides efficient serialization to TOON format, a token-optimized alternative to JSON designed for Large Language Model (LLM) input. TOON achieves 30-60% token reduction compared to JSON while maintaining human readability.

## Features

- **Token Efficiency**: 30-60% reduction in token usage compared to JSON
- **High Performance**: Zero-allocation hot paths using modern .NET patterns
- **Flexible Delimiters**: Support for comma, tab, and pipe delimiters
- **Smart Array Formatting**: Automatic selection between inline, tabular, and list formats
- **UTF-8 Native**: Direct UTF-8 byte operations for optimal performance
- **Streaming Support**: Handle large documents with streaming APIs

## Installation

```bash
dotnet add package TooNet
```

## Quick Start

```csharp
using TooNet;

// Simple object serialization
var user = new User { Id = 123, Name = "Ada", Active = true };
string toon = TooNetSerializer.Serialize(user);
// Output:
// id: 123
// name: Ada
// active: true

// Serialization with options
var options = new TooNetSerializerOptions
{
    DefaultDelimiter = Delimiter.Tab,
    ArrayMode = ArrayFormatMode.Tabular
};
byte[] utf8 = TooNetSerializer.SerializeToUtf8Bytes(data, options);

// Stream serialization for large data
await TooNetSerializer.SerializeAsync(stream, largeObject);
```

## TOON Format

TOON uses indentation-based structure with minimal syntax:

```toon
user:
  id: 123
  name: Ada Lovelace
  tags[3]: programming,mathematics,computing
  projects[2]{name,year}:
    Analytical Engine,1843
    Notes on Babbage,1842
```

## Performance

TooNet achieves high performance through:
- Direct UTF-8 byte operations
- ArrayPool buffer management
- Vectorized operations for delimiter detection
- Pre-computed indentation buffers
- Zero allocations in hot paths

## Building from Source

```bash
dotnet build
dotnet test
dotnet pack
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.