# TooNet Implementation Plan

## Overview
Implementation of TooNet serialization library for .NET 8.0+, converting .NET objects to TOON format. Each PR builds independently and adds incremental functionality.

## Project Setup

### Initial Project Structure
```
TooNet/
├── src/
│   └── TooNet/
│       ├── TooNet.csproj
│       └── (source files)
├── tests/
│   └── TooNet.Tests/
│       ├── TooNet.Tests.csproj
│       └── (test files)
├── benchmarks/
│   └── TooNet.Benchmarks/
│       ├── TooNet.Benchmarks.csproj
│       └── (benchmark files)
├── TooNet.sln
├── Directory.Build.props
├── .gitignore
├── README.md
└── LICENSE
```

## Implementation Tasks (PRs)

### PR #1: Project Setup and Infrastructure
**Size:** ~150 lines
**Branch:** `feat/project-setup`

**Files to create/modify:**
- `TooNet.sln`
- `Directory.Build.props`
- `src/TooNet/TooNet.csproj`
- `tests/TooNet.Tests/TooNet.Tests.csproj`
- `benchmarks/TooNet.Benchmarks/TooNet.Benchmarks.csproj`
- `.gitignore`
- `README.md`
- `.github/workflows/ci.yml`

**Implementation:**
```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>12</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>

<!-- src/TooNet/TooNet.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>TooNet</AssemblyName>
    <RootNamespace>TooNet</RootNamespace>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>
</Project>
```

**Tests:**
- Verify solution builds
- Verify test project references TooNet

**Completeness Criteria:**
- [ ] Solution builds without errors
- [ ] Test project can reference main project
- [ ] CI pipeline runs on PR

---

### PR #2: Core Types and Enums
**Size:** ~100 lines
**Branch:** `feat/core-types`
**Depends on:** PR #1

**Files to create:**
- `src/TooNet/Delimiter.cs`
- `src/TooNet/ArrayFormatMode.cs`
- `src/TooNet/TooNetException.cs`
- `tests/TooNet.Tests/DelimiterTests.cs`

**Implementation:**
```csharp
// src/TooNet/Delimiter.cs
namespace TooNet;

public enum Delimiter
{
    Comma = ',',
    Tab = '\t',
    Pipe = '|'
}

// src/TooNet/ArrayFormatMode.cs
namespace TooNet;

public enum ArrayFormatMode
{
    Auto,
    Inline,
    Tabular,
    List
}

// src/TooNet/TooNetException.cs
namespace TooNet;

public class TooNetException : Exception
{
    public TooNetException(string message) : base(message) { }
    public TooNetException(string message, Exception inner) : base(message, inner) { }
}
```

**Tests:**
```csharp
[Fact]
public void Delimiter_HasCorrectValues()
{
    Assert.Equal(',', (char)Delimiter.Comma);
    Assert.Equal('\t', (char)Delimiter.Tab);
    Assert.Equal('|', (char)Delimiter.Pipe);
}
```

**Completeness Criteria:**
- [ ] All enums defined with correct values
- [ ] Exception type can be thrown and caught
- [ ] Unit tests pass

---

### PR #3: UTF-8 Constants and Helpers
**Size:** ~200 lines
**Branch:** `feat/utf8-constants`
**Depends on:** PR #2

**Files to create:**
- `src/TooNet/Internal/Utf8Constants.cs`
- `src/TooNet/Internal/IndentationCache.cs`
- `tests/TooNet.Tests/Internal/Utf8ConstantsTests.cs`

**Implementation:**
```csharp
// src/TooNet/Internal/Utf8Constants.cs
namespace TooNet.Internal;

internal static class Utf8Constants
{
    public static ReadOnlySpan<byte> True => "true"u8;
    public static ReadOnlySpan<byte> False => "false"u8;
    public static ReadOnlySpan<byte> Null => "null"u8;

    public const byte Quote = (byte)'"';
    public const byte Backslash = (byte)'\\';
    public const byte Colon = (byte)':';
    public const byte Space = (byte)' ';
    public const byte Newline = (byte)'\n';
    public const byte Comma = (byte)',';
    public const byte Tab = (byte)'\t';
    public const byte Pipe = (byte)'|';
    public const byte OpenBracket = (byte)'[';
    public const byte CloseBracket = (byte)']';
    public const byte OpenBrace = (byte)'{';
    public const byte CloseBrace = (byte)'}';
    public const byte Hyphen = (byte)'-';
}

// src/TooNet/Internal/IndentationCache.cs
internal static class IndentationCache
{
    private static readonly byte[] s_indentationBytes;

    static IndentationCache()
    {
        s_indentationBytes = new byte[64]; // Support 32 levels
        Array.Fill(s_indentationBytes, Utf8Constants.Space);
    }

    public static ReadOnlySpan<byte> GetIndentation(int depth)
    {
        int spaces = depth * 2;
        if (spaces > s_indentationBytes.Length)
            throw new TooNetException($"Depth {depth} exceeds maximum of 32");
        return s_indentationBytes.AsSpan(0, spaces);
    }
}
```

**Tests:**
```csharp
[Fact]
public void IndentationCache_ReturnsCorrectSpaces()
{
    var indent = IndentationCache.GetIndentation(3);
    Assert.Equal(6, indent.Length);
    Assert.All(indent.ToArray(), b => Assert.Equal((byte)' ', b));
}
```

**Completeness Criteria:**
- [ ] All UTF-8 constants defined
- [ ] Indentation cache works for depths 0-32
- [ ] Tests verify byte values

---

### PR #4: Buffer Writer Infrastructure
**Size:** ~250 lines
**Branch:** `feat/buffer-writer`
**Depends on:** PR #3

**Files to create:**
- `src/TooNet/Internal/PooledBufferWriter.cs`
- `tests/TooNet.Tests/Internal/PooledBufferWriterTests.cs`

**Implementation:**
```csharp
// src/TooNet/Internal/PooledBufferWriter.cs
using System.Buffers;

namespace TooNet.Internal;

internal sealed class PooledBufferWriter : IBufferWriter<byte>, IDisposable
{
    private byte[] _buffer;
    private int _written;

    public PooledBufferWriter(int initialCapacity = 4096)
    {
        _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
        _written = 0;
    }

    public int WrittenCount => _written;
    public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, _written);

    public void Advance(int count)
    {
        if (_written + count > _buffer.Length)
            throw new InvalidOperationException("Cannot advance past buffer size");
        _written += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsMemory(_written);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsSpan(_written);
    }

    private void EnsureCapacity(int sizeHint)
    {
        if (_written + sizeHint <= _buffer.Length)
            return;

        var newSize = Math.Max(_buffer.Length * 2, _written + sizeHint);
        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        _buffer.AsSpan(0, _written).CopyTo(newBuffer);
        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = newBuffer;
    }

    public byte[] ToArray()
    {
        return _buffer.AsSpan(0, _written).ToArray();
    }

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(_buffer);
    }
}
```

**Tests:**
```csharp
[Fact]
public void PooledBufferWriter_WritesAndExpands()
{
    using var writer = new PooledBufferWriter(10);
    var span = writer.GetSpan(5);
    "hello"u8.CopyTo(span);
    writer.Advance(5);

    Assert.Equal(5, writer.WrittenCount);
    Assert.Equal("hello"u8.ToArray(), writer.WrittenSpan.ToArray());
}
```

**Completeness Criteria:**
- [ ] Buffer automatically expands when needed
- [ ] ArrayPool is used for allocations
- [ ] Dispose returns buffer to pool
- [ ] Tests verify write and expansion

---

### PR #5: Quoting Rules Engine with Vectorization
**Size:** ~400 lines
**Branch:** `feat/quoting-rules`
**Depends on:** PR #4

**Files to create:**
- `src/TooNet/Internal/QuotingRules.cs`
- `tests/TooNet.Tests/Internal/QuotingRulesTests.cs`

**Implementation:**
```csharp
// src/TooNet/Internal/QuotingRules.cs
using System.Numerics;
using System.Runtime.Intrinsics;

namespace TooNet.Internal;

internal static class QuotingRules
{
    public static bool RequiresQuoting(ReadOnlySpan<char> value, Delimiter delimiter)
    {
        if (value.IsEmpty) return true;
        if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1])) return true;

        // Fast vectorized scan for delimiter and colon
        if (Vector.IsHardwareAccelerated)
        {
            if (ContainsDelimiterVectorized(value, delimiter))
                return true;
        }
        else
        {
            char delimChar = (char)delimiter;
            foreach (var c in value)
            {
                if (c == delimChar || c == ':') return true;
            }
        }

        if (value.StartsWith("- ")) return true;
        if (IsBooleanLike(value)) return true;
        if (IsNumberLike(value)) return true;
        if (IsNullLike(value)) return true;
        if (IsStructuralToken(value)) return true;

        return false;
    }

    private static bool ContainsDelimiterVectorized(ReadOnlySpan<char> value, Delimiter delimiter)
    {
        var delimVector = new Vector<ushort>((ushort)delimiter);
        var colonVector = new Vector<ushort>((ushort)':');

        int i = 0;
        var ushortSpan = System.Runtime.InteropServices.MemoryMarshal.Cast<char, ushort>(value);

        for (; i <= ushortSpan.Length - Vector<ushort>.Count; i += Vector<ushort>.Count)
        {
            var vector = new Vector<ushort>(ushortSpan.Slice(i, Vector<ushort>.Count));
            if (Vector.EqualsAny(vector, delimVector) || Vector.EqualsAny(vector, colonVector))
                return true;
        }

        // Handle remaining chars
        for (; i < value.Length; i++)
        {
            if (value[i] == (char)delimiter || value[i] == ':')
                return true;
        }

        return false;
    }

    private static bool IsBooleanLike(ReadOnlySpan<char> value)
    {
        return value.Equals("true", StringComparison.Ordinal) ||
               value.Equals("false", StringComparison.Ordinal);
    }

    private static bool IsNumberLike(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty) return false;

        int i = 0;
        if (value[0] == '-') i++;

        bool hasDigit = false;
        bool hasDot = false;

        for (; i < value.Length; i++)
        {
            if (char.IsDigit(value[i]))
            {
                hasDigit = true;
            }
            else if (value[i] == '.' && !hasDot)
            {
                hasDot = true;
            }
            else
            {
                return false;
            }
        }

        return hasDigit;
    }

    private static bool IsNullLike(ReadOnlySpan<char> value)
    {
        return value.Equals("null", StringComparison.Ordinal);
    }

    private static bool IsStructuralToken(ReadOnlySpan<char> value)
    {
        if (value.Length < 2) return false;
        return (value[0] == '[' && value[^1] == ']') ||
               (value[0] == '{' && value[^1] == '}');
    }

    public static bool RequiresQuotingForKey(ReadOnlySpan<char> key)
    {
        if (key.IsEmpty) return true;

        // Must start with letter or underscore
        if (!char.IsLetter(key[0]) && key[0] != '_') return true;

        // Rest must be alphanumeric, underscore, or dot
        for (int i = 1; i < key.Length; i++)
        {
            char c = key[i];
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '.')
                return true;
        }

        return false;
    }
}
```

**Tests:**
```csharp
[Theory]
[InlineData("", true)]
[InlineData(" padded ", true)]
[InlineData("hello", false)]
[InlineData("true", true)]
[InlineData("42", true)]
[InlineData("null", true)]
[InlineData("[5]", true)]
[InlineData("- item", true)]
[InlineData("a,b", true, Delimiter.Comma)]
[InlineData("a,b", false, Delimiter.Tab)]
public void RequiresQuoting_ReturnsExpectedResult(string value, bool expected,
    Delimiter delimiter = Delimiter.Comma)
{
    Assert.Equal(expected, QuotingRules.RequiresQuoting(value, delimiter));
}

[Theory]
[InlineData("valid", false)]
[InlineData("_private", false)]
[InlineData("user.name", false)]
[InlineData("", true)]
[InlineData("123start", true)]
[InlineData("has-hyphen", true)]
[InlineData("has space", true)]
public void RequiresQuotingForKey_ReturnsExpectedResult(string key, bool expected)
{
    Assert.Equal(expected, QuotingRules.RequiresQuotingForKey(key));
}
```

**Completeness Criteria:**
- [ ] All quoting rules from spec implemented
- [ ] Key quoting rules work correctly
- [ ] Delimiter-specific rules work
- [ ] Comprehensive test coverage

---

### PR #6: TooNet Writer Core
**Size:** ~400 lines
**Branch:** `feat/writer-core`
**Depends on:** PR #5

**Files to create:**
- `src/TooNet/TooNetWriter.cs`
- `tests/TooNet.Tests/TooNetWriterTests.cs`

**Implementation:**
```csharp
// src/TooNet/TooNetWriter.cs
using System.Buffers;
using TooNet.Internal;

namespace TooNet;

public ref struct TooNetWriter
{
    private readonly IBufferWriter<byte> _output;
    private readonly Delimiter _delimiter;
    private int _depth;
    private bool _needsIndent;

    public TooNetWriter(IBufferWriter<byte> output, Delimiter delimiter = Delimiter.Comma)
    {
        _output = output;
        _delimiter = delimiter;
        _depth = 0;
        _needsIndent = false;
    }

    public void WriteNull()
    {
        WriteIndentIfNeeded();
        var span = _output.GetSpan(4);
        Utf8Constants.Null.CopyTo(span);
        _output.Advance(4);
    }

    public void WriteBoolean(bool value)
    {
        WriteIndentIfNeeded();
        var bytes = value ? Utf8Constants.True : Utf8Constants.False;
        var span = _output.GetSpan(bytes.Length);
        bytes.CopyTo(span);
        _output.Advance(bytes.Length);
    }

    public void WriteNumber(long value)
    {
        WriteIndentIfNeeded();
        Span<byte> buffer = stackalloc byte[20];
        bool success = Utf8Formatter.TryFormat(value, buffer, out int written);
        if (!success) throw new TooNetException("Failed to format number");

        var span = _output.GetSpan(written);
        buffer[..written].CopyTo(span);
        _output.Advance(written);
    }

    public void WriteNumber(double value)
    {
        WriteIndentIfNeeded();

        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            WriteNull();
            return;
        }

        Span<byte> buffer = stackalloc byte[32];
        bool success = Utf8Formatter.TryFormat(value, buffer, out int written);
        if (!success) throw new TooNetException("Failed to format number");

        var span = _output.GetSpan(written);
        buffer[..written].CopyTo(span);
        _output.Advance(written);
    }

    public void WriteString(ReadOnlySpan<char> value)
    {
        WriteIndentIfNeeded();

        bool needsQuoting = QuotingRules.RequiresQuoting(value, _delimiter);

        if (needsQuoting)
        {
            WriteQuotedString(value);
        }
        else
        {
            WriteUnquotedString(value);
        }
    }

    private void WriteQuotedString(ReadOnlySpan<char> value)
    {
        var span = _output.GetSpan(1);
        span[0] = Utf8Constants.Quote;
        _output.Advance(1);

        // Use optimized escape handler (will be added in PR #6.5)
        // For now, basic implementation
        WriteEscapedContent(value);

        span = _output.GetSpan(1);
        span[0] = Utf8Constants.Quote;
        _output.Advance(1);
    }

    private void WriteEscapedContent(ReadOnlySpan<char> value)
    {
        // Basic escape implementation for PR #6
        // Will be replaced with EscapeHandler.WriteEscapedString in PR #6.5
        foreach (char c in value)
        {
            switch (c)
            {
                case '"':
                    WriteRaw("\\\""u8);
                    break;
                case '\\':
                    WriteRaw("\\\\"u8);
                    break;
                case '\n':
                    WriteRaw("\\n"u8);
                    break;
                case '\r':
                    WriteRaw("\\r"u8);
                    break;
                case '\t':
                    WriteRaw("\\t"u8);
                    break;
                default:
                    WriteChar(c);
                    break;
            }
        }
    }

    private void WriteUnquotedString(ReadOnlySpan<char> value)
    {
        int byteCount = Encoding.UTF8.GetByteCount(value);
        var span = _output.GetSpan(byteCount);
        Encoding.UTF8.GetBytes(value, span);
        _output.Advance(byteCount);
    }

    private void WriteChar(char c)
    {
        Span<byte> buffer = stackalloc byte[4];
        int written = Encoding.UTF8.GetBytes(stackalloc char[] { c }, buffer);
        var span = _output.GetSpan(written);
        buffer[..written].CopyTo(span);
        _output.Advance(written);
    }

    public void WriteRaw(ReadOnlySpan<byte> value)
    {
        var span = _output.GetSpan(value.Length);
        value.CopyTo(span);
        _output.Advance(value.Length);
    }

    private void WriteIndentIfNeeded()
    {
        if (_needsIndent)
        {
            var indent = IndentationCache.GetIndentation(_depth);
            WriteRaw(indent);
            _needsIndent = false;
        }
    }

    public void WriteNewLine()
    {
        var span = _output.GetSpan(1);
        span[0] = Utf8Constants.Newline;
        _output.Advance(1);
        _needsIndent = true;
    }

    public void IncreaseDepth()
    {
        _depth++;
    }

    public void DecreaseDepth()
    {
        if (_depth > 0) _depth--;
    }
}
```

**Tests:**
```csharp
[Fact]
public void WriteNull_WritesNullKeyword()
{
    using var buffer = new PooledBufferWriter();
    var writer = new TooNetWriter(buffer);

    writer.WriteNull();

    Assert.Equal("null"u8.ToArray(), buffer.WrittenSpan.ToArray());
}

[Theory]
[InlineData(true, "true")]
[InlineData(false, "false")]
public void WriteBoolean_WritesCorrectKeyword(bool value, string expected)
{
    using var buffer = new PooledBufferWriter();
    var writer = new TooNetWriter(buffer);

    writer.WriteBoolean(value);

    Assert.Equal(Encoding.UTF8.GetBytes(expected), buffer.WrittenSpan.ToArray());
}

[Theory]
[InlineData(42, "42")]
[InlineData(-123, "-123")]
[InlineData(0, "0")]
public void WriteNumber_WritesDecimalForm(long value, string expected)
{
    using var buffer = new PooledBufferWriter();
    var writer = new TooNetWriter(buffer);

    writer.WriteNumber(value);

    Assert.Equal(Encoding.UTF8.GetBytes(expected), buffer.WrittenSpan.ToArray());
}

[Theory]
[InlineData("hello", "hello")]
[InlineData("", "\"\"")]
[InlineData(" padded ", "\" padded \"")]
[InlineData("a,b", "\"a,b\"")]
public void WriteString_AppliesQuotingRules(string value, string expected)
{
    using var buffer = new PooledBufferWriter();
    var writer = new TooNetWriter(buffer);

    writer.WriteString(value);

    Assert.Equal(Encoding.UTF8.GetBytes(expected), buffer.WrittenSpan.ToArray());
}
```

**Completeness Criteria:**
- [ ] All primitive types can be written
- [ ] String quoting works correctly
- [ ] Indentation tracking works
- [ ] UTF-8 encoding is correct
- [ ] Tests verify output format

---

### PR #6.5: Field Name Cache and Escape Handler
**Size:** ~250 lines
**Branch:** `feat/field-cache-escape`
**Depends on:** PR #6

**Files to create:**
- `src/TooNet/Internal/FieldNameCache.cs`
- `src/TooNet/Internal/EscapeHandler.cs`
- `tests/TooNet.Tests/Internal/FieldNameCacheTests.cs`
- `tests/TooNet.Tests/Internal/EscapeHandlerTests.cs`

**Implementation:**
```csharp
// src/TooNet/Internal/FieldNameCache.cs
namespace TooNet.Internal;

internal sealed class FieldNameCache
{
    private readonly Dictionary<string, byte[]> _cache = new();
    private readonly object _lock = new();

    public byte[] GetOrAdd(string fieldName)
    {
        if (_cache.TryGetValue(fieldName, out byte[]? utf8Bytes))
            return utf8Bytes;

        lock (_lock)
        {
            if (_cache.TryGetValue(fieldName, out utf8Bytes))
                return utf8Bytes;

            utf8Bytes = Encoding.UTF8.GetBytes(fieldName);
            _cache[fieldName] = utf8Bytes;
            return utf8Bytes;
        }
    }

    public static FieldNameCache Instance { get; } = new();
}

// src/TooNet/Internal/EscapeHandler.cs
namespace TooNet.Internal;

internal static class EscapeHandler
{
    private static readonly byte[] BackslashQuote = "\\\""u8.ToArray();
    private static readonly byte[] BackslashBackslash = "\\\\"u8.ToArray();
    private static readonly byte[] BackslashN = "\\n"u8.ToArray();
    private static readonly byte[] BackslashR = "\\r"u8.ToArray();
    private static readonly byte[] BackslashT = "\\t"u8.ToArray();

    public static void WriteEscapedString(IBufferWriter<byte> output, ReadOnlySpan<char> value)
    {
        int lastWritten = 0;

        for (int i = 0; i < value.Length; i++)
        {
            byte[]? escapeSequence = null;

            switch (value[i])
            {
                case '"': escapeSequence = BackslashQuote; break;
                case '\\': escapeSequence = BackslashBackslash; break;
                case '\n': escapeSequence = BackslashN; break;
                case '\r': escapeSequence = BackslashR; break;
                case '\t': escapeSequence = BackslashT; break;
            }

            if (escapeSequence != null)
            {
                // Write unescaped portion
                if (i > lastWritten)
                {
                    var segment = value.Slice(lastWritten, i - lastWritten);
                    int byteCount = Encoding.UTF8.GetByteCount(segment);
                    var span = output.GetSpan(byteCount);
                    Encoding.UTF8.GetBytes(segment, span);
                    output.Advance(byteCount);
                }

                // Write escape sequence
                output.Write(escapeSequence);
                lastWritten = i + 1;
            }
        }

        // Write remaining
        if (lastWritten < value.Length)
        {
            var segment = value.Slice(lastWritten);
            int byteCount = Encoding.UTF8.GetByteCount(segment);
            var span = output.GetSpan(byteCount);
            Encoding.UTF8.GetBytes(segment, span);
            output.Advance(byteCount);
        }
    }
}
```

**Tests:**
```csharp
[Fact]
public void FieldNameCache_CachesUtf8Bytes()
{
    var cache = new FieldNameCache();
    var bytes1 = cache.GetOrAdd("name");
    var bytes2 = cache.GetOrAdd("name");

    Assert.Same(bytes1, bytes2); // Same array instance
    Assert.Equal("name"u8.ToArray(), bytes1);
}

[Fact]
public void EscapeHandler_EscapesSpecialCharacters()
{
    using var buffer = new PooledBufferWriter();
    EscapeHandler.WriteEscapedString(buffer, "Hello \"World\"\nNext line");

    var expected = "Hello \\\"World\\\"\\nNext line";
    Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
}
```

**Completeness Criteria:**
- [ ] Field names cached correctly
- [ ] Thread-safe caching
- [ ] Escape sequences optimized
- [ ] Batch writing of unescaped segments

---

### PR #7: Object Serialization
**Size:** ~300 lines
**Branch:** `feat/object-serialization`
**Depends on:** PR #6

**Files to create:**
- `src/TooNet/TooNetWriter.Object.cs` (partial class)
- `tests/TooNet.Tests/TooNetWriterObjectTests.cs`

**Implementation:**
```csharp
// src/TooNet/TooNetWriter.Object.cs
namespace TooNet;

public ref partial struct TooNetWriter
{
    private bool _isFirstProperty;
    private bool _inObject;

    public void WriteStartObject()
    {
        _inObject = true;
        _isFirstProperty = true;
    }

    public void WriteEndObject()
    {
        _inObject = false;
    }

    public void WritePropertyName(ReadOnlySpan<char> name)
    {
        if (!_inObject)
            throw new TooNetException("WritePropertyName called outside object context");

        if (!_isFirstProperty)
        {
            WriteNewLine();
        }

        WriteIndentIfNeeded();

        bool needsQuoting = QuotingRules.RequiresQuotingForKey(name);
        if (needsQuoting)
        {
            WriteQuotedString(name);
        }
        else
        {
            WriteUnquotedString(name);
        }

        // Write colon separator
        WriteRaw(":"u8);

        _isFirstProperty = false;
    }

    public void WritePropertyValue(ReadOnlySpan<char> value)
    {
        // Add space after colon for primitives
        WriteRaw(" "u8);
        WriteString(value);
    }

    public void WritePropertyNull()
    {
        WriteRaw(" "u8);
        WriteNull();
    }

    public void WritePropertyBoolean(bool value)
    {
        WriteRaw(" "u8);
        WriteBoolean(value);
    }

    public void WritePropertyNumber(long value)
    {
        WriteRaw(" "u8);
        WriteNumber(value);
    }

    public void WritePropertyNumber(double value)
    {
        WriteRaw(" "u8);
        WriteNumber(value);
    }

    public void WriteNestedObject()
    {
        // No space after colon for nested objects
        WriteNewLine();
        IncreaseDepth();
        _isFirstProperty = true;
    }

    public void EndNestedObject()
    {
        DecreaseDepth();
    }
}
```

**Tests:**
```csharp
[Fact]
public void WriteObject_SimpleProperties()
{
    using var buffer = new PooledBufferWriter();
    var writer = new TooNetWriter(buffer);

    writer.WriteStartObject();
    writer.WritePropertyName("id");
    writer.WritePropertyNumber(123);
    writer.WritePropertyName("name");
    writer.WritePropertyValue("Ada");
    writer.WritePropertyName("active");
    writer.WritePropertyBoolean(true);
    writer.WriteEndObject();

    var expected = "id: 123\nname: Ada\nactive: true";
    Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
}

[Fact]
public void WriteObject_NestedObject()
{
    using var buffer = new PooledBufferWriter();
    var writer = new TooNetWriter(buffer);

    writer.WriteStartObject();
    writer.WritePropertyName("user");
    writer.WriteNestedObject();
    writer.WritePropertyName("id");
    writer.WritePropertyNumber(1);
    writer.EndNestedObject();
    writer.WriteEndObject();

    var expected = "user:\n  id: 1";
    Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
}

[Fact]
public void WriteObject_QuotedKeys()
{
    using var buffer = new PooledBufferWriter();
    var writer = new TooNetWriter(buffer);

    writer.WriteStartObject();
    writer.WritePropertyName("valid-key");
    writer.WritePropertyValue("value1");
    writer.WritePropertyName("has space");
    writer.WritePropertyValue("value2");
    writer.WriteEndObject();

    var expected = "\"valid-key\": value1\n\"has space\": value2";
    Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
}
```

**Completeness Criteria:**
- [ ] Objects serialize with correct format
- [ ] Nested objects have proper indentation
- [ ] Key quoting rules applied
- [ ] Colon spacing rules followed
- [ ] Tests verify TOON format

---

### PR #8: Basic Array Serialization (Inline)
**Size:** ~250 lines
**Branch:** `feat/array-inline`
**Depends on:** PR #7

**Files to create:**
- `src/TooNet/TooNetWriter.Array.cs` (partial class)
- `tests/TooNet.Tests/TooNetWriterArrayTests.cs`

**Implementation:**
```csharp
// src/TooNet/TooNetWriter.Array.cs
namespace TooNet;

public ref partial struct TooNetWriter
{
    private int _arrayItemCount;
    private int _arrayItemsWritten;
    private bool _inArray;
    private ArrayFormatMode _currentArrayFormat;

    public void WriteStartArray(int count, ArrayFormatMode format = ArrayFormatMode.Inline)
    {
        WriteIndentIfNeeded();

        _inArray = true;
        _arrayItemCount = count;
        _arrayItemsWritten = 0;
        _currentArrayFormat = format;

        // Write array header [N]
        WriteRaw("["u8);
        WriteNumber(count);

        // Write delimiter marker if not comma
        if (_delimiter != Delimiter.Comma)
        {
            WriteByte((byte)_delimiter);
        }

        WriteRaw("]:"u8);

        if (format == ArrayFormatMode.Inline && count > 0)
        {
            WriteRaw(" "u8); // Space before first item for inline
        }
    }

    public void WriteEndArray()
    {
        if (_arrayItemsWritten != _arrayItemCount)
        {
            throw new TooNetException($"Array declared {_arrayItemCount} items but wrote {_arrayItemsWritten}");
        }

        _inArray = false;
    }

    public void WriteArrayItem(ReadOnlySpan<char> value)
    {
        if (!_inArray)
            throw new TooNetException("WriteArrayItem called outside array context");

        if (_arrayItemsWritten >= _arrayItemCount)
            throw new TooNetException($"Array overflow: declared {_arrayItemCount} items");

        if (_currentArrayFormat == ArrayFormatMode.Inline)
        {
            if (_arrayItemsWritten > 0)
            {
                WriteByte((byte)_delimiter);
            }
            WriteString(value);
        }

        _arrayItemsWritten++;
    }

    public void WriteArrayNumber(long value)
    {
        if (!_inArray)
            throw new TooNetException("WriteArrayNumber called outside array context");

        if (_arrayItemsWritten >= _arrayItemCount)
            throw new TooNetException($"Array overflow: declared {_arrayItemCount} items");

        if (_currentArrayFormat == ArrayFormatMode.Inline)
        {
            if (_arrayItemsWritten > 0)
            {
                WriteByte((byte)_delimiter);
            }
            WriteNumber(value);
        }

        _arrayItemsWritten++;
    }

    public void WriteArrayBoolean(bool value)
    {
        if (!_inArray)
            throw new TooNetException("WriteArrayBoolean called outside array context");

        if (_arrayItemsWritten >= _arrayItemCount)
            throw new TooNetException($"Array overflow: declared {_arrayItemCount} items");

        if (_currentArrayFormat == ArrayFormatMode.Inline)
        {
            if (_arrayItemsWritten > 0)
            {
                WriteByte((byte)_delimiter);
            }
            WriteBoolean(value);
        }

        _arrayItemsWritten++;
    }

    public void WriteArrayNull()
    {
        if (!_inArray)
            throw new TooNetException("WriteArrayNull called outside array context");

        if (_arrayItemsWritten >= _arrayItemCount)
            throw new TooNetException($"Array overflow: declared {_arrayItemCount} items");

        if (_currentArrayFormat == ArrayFormatMode.Inline)
        {
            if (_arrayItemsWritten > 0)
            {
                WriteByte((byte)_delimiter);
            }
            WriteNull();
        }

        _arrayItemsWritten++;
    }

    private void WriteByte(byte value)
    {
        var span = _output.GetSpan(1);
        span[0] = value;
        _output.Advance(1);
    }
}
```

**Tests:**
```csharp
[Fact]
public void WriteArray_EmptyArray()
{
    using var buffer = new PooledBufferWriter();
    var writer = new TooNetWriter(buffer);

    writer.WriteStartArray(0);
    writer.WriteEndArray();

    Assert.Equal("[0]:", Encoding.UTF8.GetString(buffer.WrittenSpan));
}

[Fact]
public void WriteArray_InlinePrimitives()
{
    using var buffer = new PooledBufferWriter();
    var writer = new TooNetWriter(buffer);

    writer.WriteStartArray(3);
    writer.WriteArrayItem("reading");
    writer.WriteArrayItem("gaming");
    writer.WriteArrayItem("coding");
    writer.WriteEndArray();

    Assert.Equal("[3]: reading,gaming,coding",
        Encoding.UTF8.GetString(buffer.WrittenSpan));
}

[Fact]
public void WriteArray_MixedTypes()
{
    using var buffer = new PooledBufferWriter();
    var writer = new TooNetWriter(buffer);

    writer.WriteStartArray(4);
    writer.WriteArrayNumber(42);
    writer.WriteArrayBoolean(true);
    writer.WriteArrayNull();
    writer.WriteArrayItem("text");
    writer.WriteEndArray();

    Assert.Equal("[4]: 42,true,null,text",
        Encoding.UTF8.GetString(buffer.WrittenSpan));
}

[Fact]
public void WriteArray_TabDelimiter()
{
    using var buffer = new PooledBufferWriter();
    var writer = new TooNetWriter(buffer, Delimiter.Tab);

    writer.WriteStartArray(3);
    writer.WriteArrayItem("a");
    writer.WriteArrayItem("b");
    writer.WriteArrayItem("c");
    writer.WriteEndArray();

    Assert.Equal("[3\t]: a\tb\tc",
        Encoding.UTF8.GetString(buffer.WrittenSpan));
}

[Fact]
public void WriteArray_CountValidation()
{
    using var buffer = new PooledBufferWriter();
    var writer = new TooNetWriter(buffer);

    writer.WriteStartArray(2);
    writer.WriteArrayItem("one");

    Assert.Throws<TooNetException>(() => writer.WriteEndArray());
}
```

**Completeness Criteria:**
- [ ] Empty arrays serialize correctly
- [ ] Inline arrays with primitives work
- [ ] Delimiter support works
- [ ] Array count validation enforced
- [ ] Tests verify format and validation

---

### PR #9: Serializer Options
**Size:** ~150 lines
**Branch:** `feat/serializer-options`
**Depends on:** PR #8

**Files to create:**
- `src/TooNet/TooNetSerializerOptions.cs`
- `tests/TooNet.Tests/TooNetSerializerOptionsTests.cs`

**Implementation:**
```csharp
// src/TooNet/TooNetSerializerOptions.cs
namespace TooNet;

public sealed class TooNetSerializerOptions
{
    // Format options
    public Delimiter DefaultDelimiter { get; set; } = Delimiter.Comma;
    public bool IncludeLengthMarkers { get; set; } = false;

    // Array format control
    public ArrayFormatMode ArrayMode { get; set; } = ArrayFormatMode.Auto;
    public int TabularThreshold { get; set; } = 3;
    public int InlineMaxLength { get; set; } = 100;

    // Performance options
    public int MaxDepth { get; set; } = 64;
    public int InitialBufferSize { get; set; } = 4096;

    // Type handling
    public bool IgnoreNullValues { get; set; } = false;
    public bool WriteEnumsAsStrings { get; set; } = true;

    // Singleton default instance
    private static TooNetSerializerOptions? s_defaultOptions;
    public static TooNetSerializerOptions Default =>
        s_defaultOptions ??= new TooNetSerializerOptions();
}
```

**Tests:**
```csharp
[Fact]
public void DefaultOptions_HaveExpectedValues()
{
    var options = new TooNetSerializerOptions();

    Assert.Equal(Delimiter.Comma, options.DefaultDelimiter);
    Assert.False(options.IncludeLengthMarkers);
    Assert.Equal(ArrayFormatMode.Auto, options.ArrayMode);
    Assert.Equal(3, options.TabularThreshold);
    Assert.Equal(100, options.InlineMaxLength);
    Assert.Equal(64, options.MaxDepth);
    Assert.Equal(4096, options.InitialBufferSize);
    Assert.False(options.IgnoreNullValues);
    Assert.True(options.WriteEnumsAsStrings);
}

[Fact]
public void DefaultSingleton_ReturnsSameInstance()
{
    var options1 = TooNetSerializerOptions.Default;
    var options2 = TooNetSerializerOptions.Default;

    Assert.Same(options1, options2);
}
```

**Completeness Criteria:**
- [ ] All options from spec defined
- [ ] Default values match spec
- [ ] Singleton pattern works
- [ ] Tests verify defaults

---

### PR #10: Main Serializer Entry Point
**Size:** ~350 lines
**Branch:** `feat/serializer-main`
**Depends on:** PR #9

**Files to create:**
- `src/TooNet/TooNetSerializer.cs`
- `src/TooNet/Internal/ObjectSerializer.cs`
- `tests/TooNet.Tests/TooNetSerializerTests.cs`

**Implementation:**
```csharp
// src/TooNet/TooNetSerializer.cs
using System.Reflection;
using TooNet.Internal;

namespace TooNet;

public static class TooNetSerializer
{
    public static string Serialize<T>(T value, TooNetSerializerOptions? options = null)
    {
        options ??= TooNetSerializerOptions.Default;

        using var buffer = new PooledBufferWriter(options.InitialBufferSize);
        var writer = new TooNetWriter(buffer, options.DefaultDelimiter);

        SerializeValue(ref writer, value, options, 0);

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    public static byte[] SerializeToUtf8Bytes<T>(T value, TooNetSerializerOptions? options = null)
    {
        options ??= TooNetSerializerOptions.Default;

        using var buffer = new PooledBufferWriter(options.InitialBufferSize);
        var writer = new TooNetWriter(buffer, options.DefaultDelimiter);

        SerializeValue(ref writer, value, options, 0);

        return buffer.ToArray();
    }

    private static void SerializeValue<T>(ref TooNetWriter writer, T value,
        TooNetSerializerOptions options, int depth)
    {
        if (depth > options.MaxDepth)
            throw new TooNetException($"Maximum depth {options.MaxDepth} exceeded");

        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        Type type = value.GetType();

        // Primitives
        if (type == typeof(bool))
        {
            writer.WriteBoolean((bool)(object)value);
        }
        else if (type == typeof(int) || type == typeof(long) ||
                 type == typeof(short) || type == typeof(byte))
        {
            writer.WriteNumber(Convert.ToInt64(value));
        }
        else if (type == typeof(float) || type == typeof(double) ||
                 type == typeof(decimal))
        {
            writer.WriteNumber(Convert.ToDouble(value));
        }
        else if (type == typeof(string))
        {
            writer.WriteString((string)(object)value);
        }
        else if (type.IsEnum)
        {
            if (options.WriteEnumsAsStrings)
            {
                writer.WriteString(value.ToString()!);
            }
            else
            {
                writer.WriteNumber(Convert.ToInt64(value));
            }
        }
        // Collections
        else if (value is IEnumerable<object> enumerable)
        {
            SerializeArray(ref writer, enumerable, options, depth);
        }
        // Objects
        else
        {
            SerializeObject(ref writer, value, options, depth);
        }
    }

    private static void SerializeObject(ref TooNetWriter writer, object obj,
        TooNetSerializerOptions options, int depth)
    {
        writer.WriteStartObject();

        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        bool first = true;

        foreach (var prop in properties)
        {
            if (!prop.CanRead) continue;

            var value = prop.GetValue(obj);
            if (value is null && options.IgnoreNullValues) continue;

            writer.WritePropertyName(prop.Name.ToLowerInvariant());

            if (value is null)
            {
                writer.WritePropertyNull();
            }
            else if (IsSimpleType(value.GetType()))
            {
                SerializeSimpleProperty(ref writer, value, options);
            }
            else
            {
                writer.WriteNestedObject();
                SerializeValue(ref writer, value, options, depth + 1);
                writer.EndNestedObject();
            }
        }

        writer.WriteEndObject();
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type.IsEnum;
    }

    private static void SerializeSimpleProperty(ref TooNetWriter writer, object value,
        TooNetSerializerOptions options)
    {
        Type type = value.GetType();

        if (type == typeof(bool))
        {
            writer.WritePropertyBoolean((bool)value);
        }
        else if (type == typeof(int) || type == typeof(long) ||
                 type == typeof(short) || type == typeof(byte))
        {
            writer.WritePropertyNumber(Convert.ToInt64(value));
        }
        else if (type == typeof(float) || type == typeof(double) ||
                 type == typeof(decimal))
        {
            writer.WritePropertyNumber(Convert.ToDouble(value));
        }
        else if (type == typeof(string))
        {
            writer.WritePropertyValue((string)value);
        }
        else if (type.IsEnum)
        {
            if (options.WriteEnumsAsStrings)
            {
                writer.WritePropertyValue(value.ToString()!);
            }
            else
            {
                writer.WritePropertyNumber(Convert.ToInt64(value));
            }
        }
    }

    private static void SerializeArray(ref TooNetWriter writer, IEnumerable<object> items,
        TooNetSerializerOptions options, int depth)
    {
        var list = items.ToList();
        writer.WriteStartArray(list.Count, ArrayFormatMode.Inline);

        foreach (var item in list)
        {
            if (item is null)
            {
                writer.WriteArrayNull();
            }
            else if (item is string str)
            {
                writer.WriteArrayItem(str);
            }
            else if (item is bool b)
            {
                writer.WriteArrayBoolean(b);
            }
            else if (IsNumericType(item.GetType()))
            {
                writer.WriteArrayNumber(Convert.ToInt64(item));
            }
        }

        writer.WriteEndArray();
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(long) ||
               type == typeof(short) || type == typeof(byte) ||
               type == typeof(float) || type == typeof(double) ||
               type == typeof(decimal);
    }
}
```

**Tests:**
```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }
}

[Fact]
public void Serialize_SimpleObject()
{
    var user = new User { Id = 123, Name = "Ada", Active = true };

    var result = TooNetSerializer.Serialize(user);

    var expected = "id: 123\nname: Ada\nactive: true";
    Assert.Equal(expected, result);
}

[Fact]
public void SerializeToUtf8Bytes_ReturnsUtf8()
{
    var user = new User { Id = 1, Name = "Test" };

    var bytes = TooNetSerializer.SerializeToUtf8Bytes(user);
    var result = Encoding.UTF8.GetString(bytes);

    Assert.Contains("id: 1", result);
    Assert.Contains("name: Test", result);
}

[Fact]
public void Serialize_WithOptions()
{
    var options = new TooNetSerializerOptions
    {
        DefaultDelimiter = Delimiter.Tab,
        IgnoreNullValues = true
    };

    var user = new User { Id = 1, Name = "Test" };
    var result = TooNetSerializer.Serialize(user, options);

    Assert.Contains("id: 1", result);
}
```

**Completeness Criteria:**
- [ ] Main API methods work
- [ ] Simple objects serialize correctly
- [ ] Options are respected
- [ ] UTF-8 output works
- [ ] Tests verify basic functionality

---

### PR #11: Tabular Array Format
**Size:** ~400 lines
**Branch:** `feat/array-tabular`
**Depends on:** PR #10

**Files to create:**
- `src/TooNet/Internal/ArrayAnalyzer.cs`
- `src/TooNet/TooNetWriter.Tabular.cs` (partial class extension)
- `tests/TooNet.Tests/TabularArrayTests.cs`

**Implementation:**
```csharp
// src/TooNet/Internal/ArrayAnalyzer.cs
namespace TooNet.Internal;

internal static class ArrayAnalyzer
{
    public static (ArrayFormatMode format, string[]? fieldNames) AnalyzeArray<T>(
        IEnumerable<T> items, TooNetSerializerOptions options)
    {
        var list = items.ToList();

        if (list.Count == 0)
            return (ArrayFormatMode.Inline, null);

        // Check if all items are objects with same properties
        if (IsTabularEligible(list, out var fieldNames))
        {
            if (list.Count >= options.TabularThreshold)
                return (ArrayFormatMode.Tabular, fieldNames);
        }

        // Check if all primitives and short enough for inline
        if (AllPrimitives(list))
        {
            var totalLength = EstimateInlineLength(list);
            if (totalLength <= options.InlineMaxLength)
                return (ArrayFormatMode.Inline, null);
        }

        return (ArrayFormatMode.List, null);
    }

    private static bool IsTabularEligible<T>(List<T> items, out string[]? fieldNames)
    {
        fieldNames = null;

        if (items.Count == 0 || items[0] == null)
            return false;

        var firstType = items[0]!.GetType();
        if (!IsObjectType(firstType))
            return false;

        var properties = firstType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();

        // Check all items have same type and all properties are simple
        foreach (var item in items)
        {
            if (item == null || item.GetType() != firstType)
                return false;

            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                if (value != null && !IsSimpleType(value.GetType()))
                    return false;
            }
        }

        fieldNames = properties.Select(p => p.Name.ToLowerInvariant()).ToArray();
        return true;
    }

    private static bool IsObjectType(Type type)
    {
        return type.IsClass &&
               type != typeof(string) &&
               !typeof(IEnumerable).IsAssignableFrom(type);
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type.IsEnum;
    }

    private static bool AllPrimitives<T>(List<T> items)
    {
        foreach (var item in items)
        {
            if (item == null) continue;
            if (!IsSimpleType(item.GetType()))
                return false;
        }
        return true;
    }

    private static int EstimateInlineLength<T>(List<T> items)
    {
        int length = 0;
        foreach (var item in items)
        {
            length += item?.ToString()?.Length ?? 4; // null = 4 chars
            length += 1; // delimiter
        }
        return length;
    }
}

// src/TooNet/TooNetWriter.Tabular.cs
namespace TooNet;

public ref partial struct TooNetWriter
{
    private string[]? _tabularFieldNames;
    private bool _inTabularArray;

    public void WriteTabularArrayHeader(int count, string[] fieldNames)
    {
        WriteIndentIfNeeded();

        _inArray = true;
        _inTabularArray = true;
        _arrayItemCount = count;
        _arrayItemsWritten = 0;
        _tabularFieldNames = fieldNames;
        _currentArrayFormat = ArrayFormatMode.Tabular;

        // Write array header with fields: [N]{field1,field2}
        WriteRaw("["u8);
        WriteNumber(count);

        if (_delimiter != Delimiter.Comma)
        {
            WriteByte((byte)_delimiter);
        }

        WriteRaw("]{"u8);

        for (int i = 0; i < fieldNames.Length; i++)
        {
            if (i > 0)
            {
                WriteByte((byte)_delimiter);
            }
            WriteUnquotedString(fieldNames[i]);
        }

        WriteRaw("}:"u8);

        if (count > 0)
        {
            WriteNewLine();
            IncreaseDepth();
        }
    }

    public void WriteTabularRow(object obj)
    {
        if (!_inTabularArray || _tabularFieldNames == null)
            throw new TooNetException("WriteTabularRow called outside tabular array context");

        if (_arrayItemsWritten >= _arrayItemCount)
            throw new TooNetException($"Array overflow: declared {_arrayItemCount} items");

        WriteIndentIfNeeded();

        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propDict = properties.ToDictionary(p => p.Name.ToLowerInvariant());

        for (int i = 0; i < _tabularFieldNames.Length; i++)
        {
            if (i > 0)
            {
                WriteByte((byte)_delimiter);
            }

            if (propDict.TryGetValue(_tabularFieldNames[i], out var prop))
            {
                var value = prop.GetValue(obj);
                WriteTabularValue(value);
            }
            else
            {
                WriteNull();
            }
        }

        _arrayItemsWritten++;

        if (_arrayItemsWritten < _arrayItemCount)
        {
            WriteNewLine();
        }
    }

    private void WriteTabularValue(object? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else if (value is bool b)
        {
            WriteBoolean(b);
        }
        else if (value is string s)
        {
            WriteString(s);
        }
        else if (IsNumericType(value.GetType()))
        {
            if (value is float || value is double || value is decimal)
            {
                WriteNumber(Convert.ToDouble(value));
            }
            else
            {
                WriteNumber(Convert.ToInt64(value));
            }
        }
        else
        {
            WriteString(value.ToString() ?? "");
        }
    }

    public void EndTabularArray()
    {
        if (_inTabularArray && _arrayItemCount > 0)
        {
            DecreaseDepth();
        }
        _inTabularArray = false;
        _tabularFieldNames = null;
        WriteEndArray();
    }
}
```

**Tests:**
```csharp
public class OrderItem
{
    public string Sku { get; set; } = "";
    public int Qty { get; set; }
    public decimal Price { get; set; }
}

[Fact]
public void WriteTabularArray_UniformObjects()
{
    using var buffer = new PooledBufferWriter();
    var writer = new TooNetWriter(buffer);

    var fieldNames = new[] { "sku", "qty", "price" };
    writer.WriteTabularArrayHeader(2, fieldNames);
    writer.WriteTabularRow(new OrderItem { Sku = "A1", Qty = 2, Price = 9.99m });
    writer.WriteTabularRow(new OrderItem { Sku = "B2", Qty = 1, Price = 14.50m });
    writer.EndTabularArray();

    var expected = "[2]{sku,qty,price}:\n  A1,2,9.99\n  B2,1,14.5";
    Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
}

[Fact]
public void ArrayAnalyzer_DetectsTabularFormat()
{
    var items = new List<OrderItem>
    {
        new() { Sku = "A", Qty = 1, Price = 10 },
        new() { Sku = "B", Qty = 2, Price = 20 },
        new() { Sku = "C", Qty = 3, Price = 30 }
    };

    var options = new TooNetSerializerOptions { TabularThreshold = 2 };
    var (format, fields) = ArrayAnalyzer.AnalyzeArray(items, options);

    Assert.Equal(ArrayFormatMode.Tabular, format);
    Assert.NotNull(fields);
    Assert.Equal(new[] { "sku", "qty", "price" }, fields);
}
```

**Completeness Criteria:**
- [ ] Tabular format detection works
- [ ] Field names extracted correctly
- [ ] Tabular output matches spec
- [ ] Indentation correct
- [ ] Tests verify tabular format

---

### PR #12: List Array Format
**Size:** ~300 lines
**Branch:** `feat/array-list`
**Depends on:** PR #11

**Files to create:**
- `src/TooNet/TooNetWriter.List.cs` (partial class extension)
- `tests/TooNet.Tests/ListArrayTests.cs`

**Implementation will handle list format with "- " prefix for each item**

**Completeness Criteria:**
- [ ] List format with "- " prefix works
- [ ] Mixed types handled
- [ ] Nested objects in lists work
- [ ] Tests verify format

---

### PR #13: Complete Serializer Integration
**Size:** ~400 lines
**Branch:** `feat/serializer-complete`
**Depends on:** PR #12

**Updates to integrate all array formats into main serializer**

**Completeness Criteria:**
- [ ] Auto-detection of array formats
- [ ] All formats integrate correctly
- [ ] Complex nested structures work
- [ ] Tests cover all scenarios

---

### PR #14: Attributes Support
**Size:** ~200 lines
**Branch:** `feat/attributes`
**Depends on:** PR #13

**Files to create:**
- `src/TooNet/Attributes/TooNetArrayFormatAttribute.cs`
- `src/TooNet/Attributes/TooNetDelimiterAttribute.cs`
- Updates to serializer to respect attributes

**Completeness Criteria:**
- [ ] Attributes defined
- [ ] Serializer respects attributes
- [ ] Attribute overrides work
- [ ] Tests verify behavior

---

### PR #15: Performance Optimizations
**Size:** ~300 lines
**Branch:** `feat/performance`
**Depends on:** PR #14

**Optimize hot paths, add benchmarks**

**Completeness Criteria:**
- [ ] Benchmarks added
- [ ] Performance measured
- [ ] Optimizations applied
- [ ] No regressions

---

### PR #16: Streaming Support
**Size:** ~300 lines
**Branch:** `feat/streaming`
**Depends on:** PR #15

**Files to create/modify:**
- Update `src/TooNet/TooNetSerializer.cs` with streaming methods
- `tests/TooNet.Tests/StreamingTests.cs`

**Implementation:**
```csharp
// Addition to TooNetSerializer.cs
public static async Task SerializeAsync<T>(Stream stream, T value,
    TooNetSerializerOptions? options = null, CancellationToken ct = default)
{
    options ??= TooNetSerializerOptions.Default;

    const int BufferSize = 4096;
    using var rentedMemory = MemoryPool<byte>.Shared.Rent(BufferSize);
    var buffer = new ArrayBufferWriter<byte>(rentedMemory.Memory);

    var writer = new TooNetWriter(buffer, options.DefaultDelimiter);
    SerializeValue(ref writer, value, options, 0);

    // Write to stream in chunks
    var written = buffer.WrittenMemory;
    while (!written.IsEmpty)
    {
        var chunk = written.Slice(0, Math.Min(written.Length, BufferSize));
        await stream.WriteAsync(chunk, ct);
        written = written.Slice(chunk.Length);
    }
}

// For large collections, stream individual items
public static async Task SerializeLargeCollectionAsync<T>(
    Stream stream, IAsyncEnumerable<T> items, TooNetSerializerOptions? options = null)
{
    options ??= TooNetSerializerOptions.Default;

    using var buffer = new PooledBufferWriter(options.InitialBufferSize);
    var writer = new TooNetWriter(buffer, options.DefaultDelimiter);

    await foreach (var item in items)
    {
        SerializeValue(ref writer, item, options, 0);

        if (buffer.WrittenCount >= BufferSize - 256)
        {
            await stream.WriteAsync(buffer.WrittenMemory);
            buffer.Clear();
        }
    }

    // Flush remaining
    if (buffer.WrittenCount > 0)
    {
        await stream.WriteAsync(buffer.WrittenMemory);
    }
}
```

**Completeness Criteria:**
- [ ] Async serialization to stream
- [ ] Chunked writing for memory efficiency
- [ ] Support for IAsyncEnumerable
- [ ] Cancellation token support

---

### PR #17: Documentation and Examples
**Size:** ~200 lines
**Branch:** `feat/documentation`
**Depends on:** PR #16

**Add comprehensive documentation and examples**

**Completeness Criteria:**
- [ ] README complete
- [ ] API documentation
- [ ] Examples provided
- [ ] NuGet package metadata

---

## Testing Strategy

### Unit Tests (Per PR)
- Test individual components in isolation
- Cover edge cases and error conditions
- Verify TOON format compliance

### Integration Tests (PR #13+)
- Test complete serialization scenarios
- Verify all formats work together
- Test complex nested objects

### Performance Tests (PR #15)
- Benchmark vs System.Text.Json
- Memory allocation tracking
- Large dataset handling

### Conformance Tests (PR #13+)
- Validate against all TOON spec examples
- Cross-check with reference implementation
- Edge case validation

## Verification Checklist

Each PR must:
- [ ] Build successfully
- [ ] Pass all tests
- [ ] Include appropriate test coverage
- [ ] Follow established patterns
- [ ] Update documentation if needed
- [ ] Be independently deployable

## Success Metrics

1. **Correctness**: 100% TOON spec compliance
2. **Performance**: Within 10% of System.Text.Json
3. **Memory**: Zero allocations in hot paths
4. **Coverage**: >90% code coverage
5. **API**: Clean, intuitive, familiar to .NET developers