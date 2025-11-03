using System.Buffers;
using System.Buffers.Text;
using System.Text;
using TooNet.Internal;

namespace TooNet;

/// <summary>
/// A high-performance writer for serializing data in TOON format.
/// </summary>
public partial struct TooNetWriter
{
    private readonly IBufferWriter<byte> _output;
    private readonly Delimiter _delimiter;
    private int _depth;
    private bool _needsIndent;
    private bool _isFirstProperty;
    private bool _inObject;
    private bool _inArray;
    private int _arrayItemCount;
    private int _arrayItemsWritten;
    private ArrayFormatMode _currentArrayFormat;

    public TooNetWriter(IBufferWriter<byte> output, Delimiter delimiter = Delimiter.Comma)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _delimiter = delimiter;
        _depth = 0;
        _needsIndent = false;
        _isFirstProperty = false;
        _inObject = false;
    }

    /// <summary>
    /// Gets the current indentation depth.
    /// </summary>
    public int CurrentDepth => _depth;

    /// <summary>
    /// Gets the delimiter being used.
    /// </summary>
    public Delimiter Delimiter => _delimiter;

    #region Primitive Writers

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
        Span<byte> buffer = stackalloc byte[20]; // Max long digits
        bool success = Utf8Formatter.TryFormat(value, buffer, out int written);
        if (!success)
            throw new TooNetException("Failed to format number");

        var span = _output.GetSpan(written);
        buffer[..written].CopyTo(span);
        _output.Advance(written);
    }

    public void WriteNumber(double value)
    {
        WriteIndentIfNeeded();

        // NaN and Infinity become null
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            WriteNull();
            return;
        }

        // Convert -0 to 0
        if (value == 0.0 && double.IsNegative(value))
        {
            value = 0.0;
        }

        Span<byte> buffer = stackalloc byte[32];
        bool success = Utf8Formatter.TryFormat(value, buffer, out int written, new StandardFormat('G'));
        if (!success)
            throw new TooNetException("Failed to format number");

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

    #endregion

    #region Formatting Helpers

    public void WriteRaw(ReadOnlySpan<byte> value)
    {
        var span = _output.GetSpan(value.Length);
        value.CopyTo(span);
        _output.Advance(value.Length);
    }

    public void WriteNewLine()
    {
        WriteByte(Utf8Constants.Newline);
        _needsIndent = true;
    }

    public void IncreaseDepth()
    {
        _depth++;
        if (_depth > IndentationCache.MaxSupportedDepth)
        {
            throw new TooNetException($"Maximum depth {IndentationCache.MaxSupportedDepth} exceeded");
        }
    }

    public void DecreaseDepth()
    {
        if (_depth > 0)
            _depth--;
    }

    #endregion

    #region Private Helpers

    private void WriteIndentIfNeeded()
    {
        if (_needsIndent && _depth > 0)
        {
            var indent = IndentationCache.GetIndentation(_depth);
            WriteRaw(indent);
            _needsIndent = false;
        }
        else if (_needsIndent)
        {
            _needsIndent = false;
        }
    }

    private void WriteQuotedString(ReadOnlySpan<char> value)
    {
        WriteByte(Utf8Constants.Quote);
        WriteEscapedContent(value);
        WriteByte(Utf8Constants.Quote);
    }

    private void WriteEscapedContent(ReadOnlySpan<char> value)
    {
        // Simple implementation for now - will be optimized with EscapeHandler later
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
        Span<byte> buffer = stackalloc byte[4]; // Max UTF-8 bytes per char
        int written = Encoding.UTF8.GetBytes(stackalloc char[] { c }, buffer);
        var span = _output.GetSpan(written);
        buffer[..written].CopyTo(span);
        _output.Advance(written);
    }

    private void WriteByte(byte value)
    {
        var span = _output.GetSpan(1);
        span[0] = value;
        _output.Advance(1);
    }

    #endregion
}