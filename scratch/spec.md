# TooNet Library Design Specification

## Overview
TooNet is a high-performance .NET 8.0+ serialization library for converting .NET objects to TOON (Token-Oriented Object Notation) format. TOON is optimized for Large Language Model input, achieving 30-60% token reduction compared to JSON while maintaining human readability.

## Requirements

### Functional Requirements
1. **Serialization**: Convert .NET objects to TOON format string or UTF-8 bytes
2. **Format Compliance**: Full compliance with TOON specification v1.0
3. **Delimiter Support**: Handle comma (default), tab, and pipe delimiters
4. **Array Format Selection**: Hybrid approach - auto-detect optimal format with attribute/option overrides
5. **Type Support**: Handle all .NET primitive types, objects, collections, and custom types
6. **Validation**: Ensure correct array counts in output

### Non-Functional Requirements
1. **Performance**: Match or exceed System.Text.Json serialization performance
2. **Memory Efficiency**: Minimize allocations using modern .NET 8.0 patterns
3. **AOT Compatible**: Support ahead-of-time compilation (source generation in Phase 2)
4. **Streaming**: Support large data with streaming writer APIs
5. **Thread Safety**: Immutable configuration, thread-safe operations
6. **Single Assembly**: All functionality in one TooNet.dll

## Architecture

### Core Components

#### 1. TooNetSerializer (Entry Point)
```csharp
public static class TooNetSerializer
{
    // High-level API matching System.Text.Json pattern
    public static string Serialize<T>(T value, TooNetSerializerOptions? options = null);
    public static byte[] SerializeToUtf8Bytes<T>(T value, TooNetSerializerOptions? options = null);
    public static void Serialize<T>(Stream stream, T value, TooNetSerializerOptions? options = null);
    public static Task SerializeAsync<T>(Stream stream, T value, TooNetSerializerOptions? options = null);
}
```

#### 2. TooNetWriter (Low-Level Writer)
```csharp
public ref struct TooNetWriter
{
    // Stack-allocated writer for performance
    public TooNetWriter(IBufferWriter<byte> output, TooNetSerializerOptions options);

    // Primitive writers
    public void WriteNumber(long value);
    public void WriteNumber(double value);
    public void WriteString(ReadOnlySpan<char> value);
    public void WriteBoolean(bool value);
    public void WriteNull();

    // Object writers
    public void WriteStartObject();
    public void WriteEndObject();
    public void WritePropertyName(ReadOnlySpan<char> name);

    // Array writers
    public void WriteStartArray(int count, ArrayFormat format = ArrayFormat.Auto);
    public void WriteArrayHeader(int count, Delimiter delimiter, ReadOnlySpan<string> fieldNames);
    public void WriteEndArray();

    // Raw output
    public void WriteRaw(ReadOnlySpan<byte> utf8Value);
}
```

### Performance Optimizations

#### Memory Management
- **ArrayPool<byte>**: Rent buffers for temporary operations
- **Span<T>/Memory<T>**: Zero-copy slicing and processing
- **Stack Allocation**: Ref structs for reader/writer state
- **String Interning**: Optional string pooling for repeated values

#### UTF-8 Processing
- Work directly with UTF-8 bytes to avoid encoding overhead
- Pre-computed byte patterns for delimiters and structural tokens
- Vectorized scanning for quote detection using SIMD (when available)
- Direct UTF-8 number formatting using Utf8Formatter

#### Indentation Handling
- Pre-computed indentation buffers up to 32 levels
- Direct byte writing without string concatenation
- Cached indentation state during traversal

#### Field Name Caching
- Cache UTF-8 encoded field names for repeated use
- Dictionary-based lookup for common property names
- Avoids repeated UTF-8 encoding of same strings

#### Escape Sequence Optimization
- Efficient escape sequence handling with minimal allocations
- Batch writing of unescaped segments
- Pre-computed escape sequences for common characters

### Serialization Pipeline

```
.NET Object
    ↓
[Type Converter] → (optional source-generated fast path in Phase 2)
    ↓
[Array Analyzer] → determines optimal format (inline/tabular/list)
    ↓
[TooNetWriter] → writes UTF-8 bytes to buffer
    ↓
UTF-8 Bytes/String
```

## API Design

### Configuration Options

```csharp
public class TooNetSerializerOptions
{
    // Format options
    public Delimiter DefaultDelimiter { get; set; } = Delimiter.Comma;
    public bool IncludeLengthMarkers { get; set; } = false;  // Use [#N] format

    // Array format control
    public ArrayFormatMode ArrayMode { get; set; } = ArrayFormatMode.Auto;
    public int TabularThreshold { get; set; } = 3;  // Min items for tabular
    public int InlineMaxLength { get; set; } = 100;  // Max chars for inline

    // Performance options
    public int MaxDepth { get; set; } = 64;
    public int InitialBufferSize { get; set; } = 4096;

    // Type handling
    public IList<ITooNetConverter> Converters { get; }
    public bool IgnoreNullValues { get; set; } = false;
    public bool WriteEnumsAsStrings { get; set; } = true;
}

public enum ArrayFormatMode
{
    Auto,      // Analyze and choose best format
    Inline,    // Force inline format when possible
    Tabular,   // Force tabular format when possible
    List       // Always use list format
}
```

### Custom Converters

```csharp
public abstract class TooNetConverter<T> : ITooNetConverter
{
    public abstract void Write(TooNetWriter writer, T value, TooNetSerializerOptions options);
}

// Attribute for array format hints
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class TooNetArrayFormatAttribute : Attribute
{
    public ArrayFormatMode Format { get; set; }
    public TooNetArrayFormatAttribute(ArrayFormatMode format) => Format = format;
}

// Attribute for delimiter preference
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class TooNetDelimiterAttribute : Attribute
{
    public Delimiter Delimiter { get; set; }
    public TooNetDelimiterAttribute(Delimiter delimiter) => Delimiter = delimiter;
}
```

### Source Generation (Phase 2)

```csharp
// Future: Source generation for AOT and performance
[TooNetSerializable]
public partial class MyModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    [TooNetArrayFormat(ArrayFormatMode.Tabular)]
    public List<Item> Items { get; set; }
}
```

## Implementation Phases

### Phase 1: Core Functionality
- TooNetWriter implementation with UTF-8 support
- Support for primitives (numbers, strings, booleans, null)
- Simple object serialization
- Basic array serialization (inline format)
- Comma delimiter only
- String quoting rules implementation

### Phase 2: Advanced Features
- Array format auto-detection (inline/tabular/list)
- Support for all delimiter types (tab, pipe)
- Complex nested structures
- Full quoting rules compliance
- Array count validation (strict)
- Attribute-based format hints

### Phase 3: Performance Optimization
- ArrayPool<byte> integration
- Pre-computed indentation buffers
- Span-based operations throughout
- Zero-allocation hot paths
- Vectorized delimiter scanning (if beneficial)

### Phase 4: Extended Features (Future)
- Custom converters API
- Async/streaming support
- Source generation for known types
- AOT compilation support

## Testing Strategy

### Unit Tests
- TooNetWriter component tests
- Edge cases for quoting rules
- Delimiter handling variations
- Array format selection logic
- Indentation correctness

### Integration Tests
- Complex object graph serialization
- All TOON spec example outputs
- Performance benchmarks vs System.Text.Json

### Conformance Tests
- Full TOON specification v1.0 compliance
- Validate against all spec examples
- Edge case coverage

## Example Usage

```csharp
// Simple serialization
var user = new User { Id = 123, Name = "Ada" };
string toon = TooNetSerializer.Serialize(user);
// Output:
// id: 123
// name: Ada

// With options
var options = new TooNetSerializerOptions
{
    DefaultDelimiter = Delimiter.Tab,
    IncludeLengthMarkers = true,
    ArrayMode = ArrayFormatMode.Tabular
};
byte[] utf8 = TooNetSerializer.SerializeToUtf8Bytes(data, options);

// Complex object with arrays
var order = new Order
{
    Id = "ORD-001",
    Items = new List<OrderItem>
    {
        new() { Sku = "A1", Qty = 2, Price = 9.99m },
        new() { Sku = "B2", Qty = 1, Price = 14.50m }
    }
};
string toon = TooNetSerializer.Serialize(order);
// Output:
// id: ORD-001
// items[2]{sku,qty,price}:
//   A1,2,9.99
//   B2,1,14.50

// With attributes
public class Product
{
    public string Name { get; set; }

    [TooNetArrayFormat(ArrayFormatMode.Inline)]
    public List<string> Tags { get; set; }

    [TooNetDelimiter(Delimiter.Tab)]
    public List<Review> Reviews { get; set; }
}
```

## Success Criteria
1. Full TOON v1.0 specification compliance for serialization
2. Performance matching or exceeding System.Text.Json serialization
3. Zero allocations in hot paths (using Span<T>, ArrayPool, etc.)
4. Support for all major .NET types and collections
5. Clean, intuitive API matching System.Text.Json patterns

## Key Implementation Details

### Array Format Detection Algorithm
```csharp
// Pseudo-code for array format selection
ArrayFormat DetermineFormat(IEnumerable items)
{
    // Check attribute override first
    if (HasFormatAttribute) return AttributeFormat;

    // Check options override
    if (Options.ArrayMode != Auto) return Options.ArrayMode;

    // Auto-detection logic
    if (AllPrimitives && Count > 0 && TotalLength < InlineMaxLength)
        return ArrayFormat.Inline;

    if (AllObjectsWithSameKeys && AllPrimitiveValues && Count >= TabularThreshold)
        return ArrayFormat.Tabular;

    return ArrayFormat.List;
}
```

### String Quoting Decision Tree
1. Empty string → Quote
2. Has leading/trailing whitespace → Quote
3. Contains active delimiter → Quote
4. Contains colon → Quote
5. Looks like boolean/number/null → Quote
6. Starts with "- " → Quote
7. Looks like structural token ([N], {}) → Quote
8. Otherwise → No quote

### Indentation Management
- Maintain current depth as integer
- Pre-compute indentation bytes: `byte[] indent = new byte[64];` filled with spaces
- Write indentation: `writer.Write(indent.AsSpan(0, depth * 2))`

## Project Structure
```
TooNet/
├── TooNet.csproj              # .NET 8.0 project
├── TooNetSerializer.cs        # Main entry point
├── TooNetWriter.cs            # Low-level writer
├── TooNetSerializerOptions.cs # Configuration
├── Converters/
│   ├── ITooNetConverter.cs
│   ├── ObjectConverter.cs
│   ├── ArrayConverter.cs
│   └── PrimitiveConverters.cs
├── Internal/
│   ├── ArrayAnalyzer.cs       # Array format detection
│   ├── QuotingRules.cs        # String quoting logic with vectorization
│   ├── Utf8Constants.cs       # Pre-computed UTF-8 bytes
│   ├── FieldNameCache.cs      # UTF-8 field name caching
│   ├── EscapeHandler.cs       # Optimized escape sequence handling
│   └── PooledBufferWriter.cs  # ArrayPool-based buffer management
└── Attributes/
    ├── TooNetArrayFormatAttribute.cs
    └── TooNetDelimiterAttribute.cs
```