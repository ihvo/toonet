namespace TooNet;

/// <summary>
/// A high-performance writer for serializing data in TOON format.
/// </summary>
public partial struct TooNetWriter(IBufferWriter<byte> output, Delimiter delimiter = Delimiter.Comma)
{
    private readonly IBufferWriter<byte> output = output ?? throw new ArgumentNullException(nameof(output));
    private readonly Delimiter delimiter = delimiter;
    private int depth = 0;
    private bool needsIndent = false;
    private bool isFirstProperty = false;
    private bool inObject = false;
    private bool inArray;
    private int arrayItemCount;
    private int arrayItemsWritten;
    private ArrayFormatMode currentArrayFormat;

    /// <summary>
    /// Gets the current indentation depth.
    /// </summary>
    public readonly int CurrentDepth => depth;

    /// <summary>
    /// Gets the delimiter being used.
    /// </summary>
    public readonly Delimiter Delimiter => delimiter;

    #region Primitive Writers

    public void WriteNull()
    {
        WriteIndentIfNeeded();
        var span = output.GetSpan(4);
        Utf8Constants.Null.CopyTo(span);
        output.Advance(4);
    }

    public void WriteBoolean(bool value)
    {
        WriteIndentIfNeeded();
        var bytes = value ? Utf8Constants.True : Utf8Constants.False;
        var span = output.GetSpan(bytes.Length);
        bytes.CopyTo(span);
        output.Advance(bytes.Length);
    }

    public void WriteNumber(long value)
    {
        WriteIndentIfNeeded();
        Span<byte> buffer = stackalloc byte[20]; // Max long digits
        bool success = Utf8Formatter.TryFormat(value, buffer, out int written);
        if (!success)
            throw new TooNetException("Failed to format number");

        var span = output.GetSpan(written);
        buffer[..written].CopyTo(span);
        output.Advance(written);
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

        var span = output.GetSpan(written);
        buffer[..written].CopyTo(span);
        output.Advance(written);
    }

    public void WriteString(ReadOnlySpan<char> value)
    {
        WriteIndentIfNeeded();

        bool needsQuoting = QuotingRules.RequiresQuoting(value, delimiter);

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
        var span = output.GetSpan(value.Length);
        value.CopyTo(span);
        output.Advance(value.Length);
    }

    public void WriteNewLine()
    {
        WriteByte(Utf8Constants.Newline);
        needsIndent = true;
    }

    public void IncreaseDepth()
    {
        depth++;
        if (depth > IndentationCache.MaxSupportedDepth)
        {
            throw new TooNetException($"Maximum depth {IndentationCache.MaxSupportedDepth} exceeded");
        }
    }

    public void DecreaseDepth()
    {
        if (depth > 0)
            depth--;
    }

    #endregion

    #region Private Helpers

    private void WriteIndentIfNeeded()
    {
        if (needsIndent && depth > 0)
        {
            var indent = IndentationCache.GetIndentation(depth);
            WriteRaw(indent);
            needsIndent = false;
        }
        else if (needsIndent)
        {
            needsIndent = false;
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
        var span = output.GetSpan(byteCount);
        Encoding.UTF8.GetBytes(value, span);
        output.Advance(byteCount);
    }

    private void WriteChar(char c)
    {
        Span<byte> buffer = stackalloc byte[4]; // Max UTF-8 bytes per char
        int written = Encoding.UTF8.GetBytes(stackalloc char[] { c }, buffer);
        var span = output.GetSpan(written);
        buffer[..written].CopyTo(span);
        output.Advance(written);
    }

    private void WriteByte(byte value)
    {
        var span = output.GetSpan(1);
        span[0] = value;
        output.Advance(1);
    }

    #endregion
}