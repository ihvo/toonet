# System.Text.Json Performance Patterns for TooNet - Serialization Focus

## Key Serialization Performance Techniques from System.Text.Json

### 1. UTF-8 Byte-Level Writing

**Pattern:** Write directly to UTF-8 bytes without string conversion
```csharp
public class Utf8JsonWriter
{
    private IBufferWriter<byte> _output;

    // Write string directly as UTF-8 bytes
    public void WriteStringValue(ReadOnlySpan<char> value)
    {
        // Direct UTF-8 encoding without intermediate string
        int bytesNeeded = Encoding.UTF8.GetByteCount(value);
        Span<byte> buffer = _output.GetSpan(bytesNeeded);
        Encoding.UTF8.GetBytes(value, buffer);
        _output.Advance(bytesNeeded);
    }

    // Write pre-encoded UTF-8 literals
    private static readonly byte[] TrueValue = "true"u8.ToArray();
    private static readonly byte[] FalseValue = "false"u8.ToArray();

    public void WriteBooleanValue(bool value)
    {
        var bytes = value ? TrueValue : FalseValue;
        _output.Write(bytes);
    }
}
```

**TooNet Application:**
- Write delimiters as bytes: `0x2C` (comma), `0x09` (tab), `0x7C` (pipe)
- Pre-encode common strings like field names
- Write indentation as raw spaces (0x20) without string allocation

### 2. ArrayPool for Output Buffer Management

**Pattern:** Use pooled buffers for serialization output
```csharp
public sealed class TooNetWriter : IDisposable
{
    private byte[] _rentedBuffer;
    private int _written;

    public TooNetWriter(int initialCapacity = 4096)
    {
        _rentedBuffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
    }

    private void Grow(int sizeHint)
    {
        var newBuffer = ArrayPool<byte>.Shared.Rent(Math.Max(
            _rentedBuffer.Length * 2,
            _rentedBuffer.Length + sizeHint));

        _rentedBuffer.AsSpan(0, _written).CopyTo(newBuffer);
        ArrayPool<byte>.Shared.Return(_rentedBuffer);
        _rentedBuffer = newBuffer;
    }

    public void Dispose()
    {
        if (_rentedBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(_rentedBuffer, clearArray: true);
        }
    }
}
```

### 3. Pre-computed Indentation Buffers

**Pattern:** Cache indentation strings to avoid repeated allocation
```csharp
public class TooNetWriter
{
    private static readonly byte[] s_indentationBytes;

    static TooNetWriter()
    {
        // Pre-compute up to 32 levels of indentation (64 spaces)
        s_indentationBytes = new byte[64];
        for (int i = 0; i < 64; i++)
            s_indentationBytes[i] = (byte)' ';
    }

    private void WriteIndentation(int depth)
    {
        int spaces = depth * 2;
        if (spaces <= s_indentationBytes.Length)
        {
            _output.Write(s_indentationBytes.AsSpan(0, spaces));
        }
        else
        {
            // Fallback for deep nesting
            WriteIndentationSlow(depth);
        }
    }
}
```

### 4. Optimized Quote Detection for Serialization

**Pattern:** Vectorized scanning to determine if values need quoting
```csharp
private static bool NeedsQuoting(ReadOnlySpan<byte> value)
{
    // Check for characters that require quoting
    // Uses vectorized operations when available

    if (Vector.IsHardwareAccelerated && value.Length >= Vector<byte>.Count)
    {
        var commaVector = new Vector<byte>((byte)',');
        var tabVector = new Vector<byte>((byte)'\t');
        var pipeVector = new Vector<byte>((byte)'|');
        var newlineVector = new Vector<byte>((byte)'\n');

        int i = 0;
        for (; i <= value.Length - Vector<byte>.Count; i += Vector<byte>.Count)
        {
            var vector = new Vector<byte>(value.Slice(i, Vector<byte>.Count));
            if (Vector.EqualsAny(vector, commaVector) ||
                Vector.EqualsAny(vector, tabVector) ||
                Vector.EqualsAny(vector, pipeVector) ||
                Vector.EqualsAny(vector, newlineVector))
            {
                return true;
            }
        }

        // Handle remaining bytes
        value = value.Slice(i);
    }

    // Fallback to scalar
    foreach (byte b in value)
    {
        if (b == ',' || b == '\t' || b == '|' || b == '\n' || b == '"')
            return true;
    }

    return false;
}
```

### 5. Streaming Write Support

**Pattern:** Write directly to streams without buffering entire document
```csharp
public async ValueTask WriteAsync(Stream stream, TooNetDocument document, CancellationToken ct)
{
    const int BufferSize = 4096;
    using var rentedBuffer = MemoryPool<byte>.Shared.Rent(BufferSize);
    var writer = new Utf8TooNetWriter(rentedBuffer.Memory);

    foreach (var element in document.Elements)
    {
        writer.WriteElement(element);

        if (writer.BytesPending >= BufferSize - 256) // Leave room for next element
        {
            await stream.WriteAsync(writer.WrittenMemory, ct);
            writer.Reset();
        }
    }

    // Flush remaining
    if (writer.BytesPending > 0)
    {
        await stream.WriteAsync(writer.WrittenMemory, ct);
    }
}
```

### 6. Source Generation for Serialization (AOT)

**Pattern:** Generate serialization code at compile time
```csharp
[TooNetSerializable]
public partial class MyModel
{
    public string Name { get; set; }
    public int Count { get; set; }
}

// Generated code
partial class MyModel
{
    private static void WriteTooNet(Utf8TooNetWriter writer, MyModel value)
    {
        writer.WriteIndentation();
        writer.WriteFieldName("Name");
        writer.WriteValue(value.Name);
        writer.WriteLine();

        writer.WriteIndentation();
        writer.WriteFieldName("Count");
        writer.WriteValue(value.Count);
        writer.WriteLine();
    }
}
```

### 7. Efficient Number Formatting

**Pattern:** Format numbers directly to UTF-8
```csharp
public void WriteInt32Value(int value)
{
    Span<byte> buffer = stackalloc byte[11]; // Max int32 length
    bool success = Utf8Formatter.TryFormat(value, buffer, out int bytesWritten);
    _output.Write(buffer.Slice(0, bytesWritten));
}

public void WriteDoubleValue(double value)
{
    Span<byte> buffer = stackalloc byte[128];
    bool success = Utf8Formatter.TryFormat(value, buffer, out int bytesWritten,
        new StandardFormat('G', 17)); // Preserve precision
    _output.Write(buffer.Slice(0, bytesWritten));
}
```

### 8. Escape Sequence Handling

**Pattern:** Efficiently escape special characters during serialization
```csharp
private void WriteEscapedString(ReadOnlySpan<byte> value)
{
    int lastWritten = 0;

    for (int i = 0; i < value.Length; i++)
    {
        byte b = value[i];

        // Check if escaping needed
        if (b == '"' || b == '\\' || b < 0x20)
        {
            // Write unescaped portion
            if (i > lastWritten)
            {
                _output.Write(value.Slice(lastWritten, i - lastWritten));
            }

            // Write escape sequence
            WriteEscapeSequence(b);
            lastWritten = i + 1;
        }
    }

    // Write remaining
    if (lastWritten < value.Length)
    {
        _output.Write(value.Slice(lastWritten));
    }
}

private void WriteEscapeSequence(byte value)
{
    _output.Write("\\"u8);

    switch (value)
    {
        case (byte)'"':  _output.Write("\""u8); break;
        case (byte)'\\': _output.Write("\\"u8); break;
        case (byte)'\n': _output.Write("n"u8); break;
        case (byte)'\r': _output.Write("r"u8); break;
        case (byte)'\t': _output.Write("t"u8); break;
        default:
            // Unicode escape for control characters
            _output.Write("u00"u8);
            WriteHexByte(value);
            break;
    }
}
```

## Serialization-Specific Optimizations for TooNet

### 1. Field Name Caching
```csharp
public class FieldNameCache
{
    private readonly Dictionary<string, byte[]> _cache = new();

    public byte[] GetOrAdd(string fieldName)
    {
        if (!_cache.TryGetValue(fieldName, out byte[] utf8Bytes))
        {
            utf8Bytes = Encoding.UTF8.GetBytes(fieldName);
            _cache[fieldName] = utf8Bytes;
        }
        return utf8Bytes;
    }
}
```

### 2. Delimiter Context Management
```csharp
public struct SerializationContext
{
    public byte Delimiter;
    public int IndentLevel;
    public bool RequiresQuoting;

    public void WriteDelimitedValue(Utf8TooNetWriter writer, ReadOnlySpan<byte> value)
    {
        if (RequiresQuoting)
        {
            writer.WriteByte((byte)'"');
            writer.WriteEscaped(value);
            writer.WriteByte((byte)'"');
        }
        else
        {
            writer.Write(value);
        }

        if (Delimiter != 0)
        {
            writer.WriteByte(Delimiter);
        }
    }
}
```

### 3. Inline Array Serialization
```csharp
public void WriteInlineArray<T>(ReadOnlySpan<T> values, byte delimiter)
{
    for (int i = 0; i < values.Length; i++)
    {
        if (i > 0)
            _output.WriteByte(delimiter);

        WriteValue(values[i]);
    }
}
```

## Performance Priorities for TooNet Serialization

1. **Direct UTF-8 writing** - Avoid string allocations
2. **ArrayPool for buffers** - Reduce GC pressure
3. **Pre-computed indentation** - Cache common patterns
4. **Vectorized quote detection** - Fast path for determining quoting needs
5. **Field name caching** - Reuse UTF-8 encoded field names
6. **Streaming writes** - Support large documents without full buffering
7. **Source generation** - Compile-time serialization for known types

## Implementation Recommendations

### Phase 1: Core Serialization Performance
- Implement `Utf8TooNetWriter` with direct byte writing
- Use `ArrayPool<byte>` for output buffers
- Pre-compute and cache indentation bytes
- Cache UTF-8 encoded field names

### Phase 2: Advanced Optimizations
- Add vectorized quote detection
- Implement escape sequence optimization
- Add streaming writer support
- Direct number-to-UTF8 formatting

### Phase 3: Source Generation
- Create source generator for known types
- Generate optimized serialization code
- Eliminate reflection for common scenarios

This focused approach on serialization combines System.Text.Json's proven patterns with TooNet's specific requirements for optimal write performance.