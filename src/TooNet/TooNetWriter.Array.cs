namespace TooNet;

public partial struct TooNetWriter
{
    #region Array Writers

    public void WriteStartArray(int count, ArrayFormatMode format = ArrayFormatMode.Inline, string[]? fieldNames = null)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Array count cannot be negative");

        WriteIndentIfNeeded();

        // Write array header: [N]: or [N<delimiter>]: or [N]{fields}:
        WriteByte(Utf8Constants.OpenBracket);

        // Write count
        Span<byte> buffer = stackalloc byte[20];
        bool success = Utf8Formatter.TryFormat(count, buffer, out int written);
        if (!success)
            throw new TooNetException("Failed to format array count");

        var span = output.GetSpan(written);
        buffer[..written].CopyTo(span);
        output.Advance(written);

        // Write delimiter marker if not comma
        if (delimiter != Delimiter.Comma)
        {
            WriteByte((byte)delimiter);
        }

        WriteByte(Utf8Constants.CloseBracket);

        // Write field names for tabular format
        if (format == ArrayFormatMode.Tabular && fieldNames != null && fieldNames.Length > 0)
        {
            WriteByte(Utf8Constants.OpenBrace);
            for (int i = 0; i < fieldNames.Length; i++)
            {
                if (i > 0)
                {
                    WriteByte((byte)delimiter);
                }
                WriteUnquotedString(fieldNames[i]);
            }
            WriteByte(Utf8Constants.CloseBrace);
        }

        WriteByte(Utf8Constants.Colon);

        // For inline arrays with content, add space after colon
        if (format == ArrayFormatMode.Inline && count > 0)
        {
            WriteByte(Utf8Constants.Space);
        }

        inArray = true;
        arrayItemCount = count;
        arrayItemsWritten = 0;
        currentArrayFormat = format;
    }

    public void WriteEndArray()
    {
        if (!inArray)
            throw new TooNetException("WriteEndArray called outside array context");

        if (arrayItemsWritten != arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {arrayItemCount} items, but wrote {arrayItemsWritten}");

        inArray = false;
        arrayItemCount = 0;
        arrayItemsWritten = 0;
    }

    public void WriteArrayItem(ReadOnlySpan<char> value)
    {
        if (!inArray)
            throw new TooNetException("WriteArrayItem called outside array context");

        if (arrayItemsWritten >= arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {arrayItemCount} items");

        // Write delimiter before item (except for first item)
        if (arrayItemsWritten > 0)
        {
            WriteByte((byte)delimiter);
        }

        // Write the value
        WriteString(value);

        arrayItemsWritten++;
    }

    public void WriteArrayNumber(long value)
    {
        if (!inArray)
            throw new TooNetException("WriteArrayNumber called outside array context");

        if (arrayItemsWritten >= arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {arrayItemCount} items");

        // Write delimiter before item (except for first item)
        if (arrayItemsWritten > 0)
        {
            WriteByte((byte)delimiter);
        }

        // Write the number directly (no quoting)
        Span<byte> buffer = stackalloc byte[20];
        bool success = Utf8Formatter.TryFormat(value, buffer, out int written);
        if (!success)
            throw new TooNetException("Failed to format number");

        var span = output.GetSpan(written);
        buffer[..written].CopyTo(span);
        output.Advance(written);

        arrayItemsWritten++;
    }

    public void WriteArrayNumber(double value)
    {
        if (!inArray)
            throw new TooNetException("WriteArrayNumber called outside array context");

        if (arrayItemsWritten >= arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {arrayItemCount} items");

        // Write delimiter before item (except for first item)
        if (arrayItemsWritten > 0)
        {
            WriteByte((byte)delimiter);
        }

        // NaN and Infinity become null
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            var span = output.GetSpan(4);
            Utf8Constants.Null.CopyTo(span);
            output.Advance(4);
            arrayItemsWritten++;
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

        var span2 = output.GetSpan(written);
        buffer[..written].CopyTo(span2);
        output.Advance(written);

        arrayItemsWritten++;
    }

    public void WriteArrayBoolean(bool value)
    {
        if (!inArray)
            throw new TooNetException("WriteArrayBoolean called outside array context");

        if (arrayItemsWritten >= arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {arrayItemCount} items");

        // Write delimiter before item (except for first item)
        if (arrayItemsWritten > 0)
        {
            WriteByte((byte)delimiter);
        }

        // Write the boolean directly (no quoting)
        var bytes = value ? Utf8Constants.True : Utf8Constants.False;
        var span = output.GetSpan(bytes.Length);
        bytes.CopyTo(span);
        output.Advance(bytes.Length);

        arrayItemsWritten++;
    }

    public void WriteArrayNull()
    {
        if (!inArray)
            throw new TooNetException("WriteArrayNull called outside array context");

        if (arrayItemsWritten >= arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {arrayItemCount} items");

        // Write delimiter before item (except for first item)
        if (arrayItemsWritten > 0)
        {
            WriteByte((byte)delimiter);
        }

        // Write null directly
        var span = output.GetSpan(4);
        Utf8Constants.Null.CopyTo(span);
        output.Advance(4);

        arrayItemsWritten++;
    }

    #endregion

    #region List Format Writers

    public void WriteListItem(ReadOnlySpan<char> value)
    {
        if (!inArray)
            throw new TooNetException("WriteListItem called outside array context");

        if (arrayItemsWritten >= arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {arrayItemCount} items");

        WriteNewLine();
        WriteIndentIfNeeded();
        WriteRaw("- "u8);
        WriteString(value);

        arrayItemsWritten++;
    }

    public void WriteListItemNumber(long value)
    {
        if (!inArray)
            throw new TooNetException("WriteListItemNumber called outside array context");

        if (arrayItemsWritten >= arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {arrayItemCount} items");

        WriteNewLine();
        WriteIndentIfNeeded();
        WriteRaw("- "u8);

        Span<byte> buffer = stackalloc byte[20];
        bool success = Utf8Formatter.TryFormat(value, buffer, out int written);
        if (!success)
            throw new TooNetException("Failed to format number");

        var span = output.GetSpan(written);
        buffer[..written].CopyTo(span);
        output.Advance(written);

        arrayItemsWritten++;
    }

    public void WriteListItemNumber(double value)
    {
        if (!inArray)
            throw new TooNetException("WriteListItemNumber called outside array context");

        if (arrayItemsWritten >= arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {arrayItemCount} items");

        WriteNewLine();
        WriteIndentIfNeeded();
        WriteRaw("- "u8);

        // NaN and Infinity become null
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            var span = output.GetSpan(4);
            Utf8Constants.Null.CopyTo(span);
            output.Advance(4);
            arrayItemsWritten++;
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

        var span2 = output.GetSpan(written);
        buffer[..written].CopyTo(span2);
        output.Advance(written);

        arrayItemsWritten++;
    }

    public void WriteListItemBoolean(bool value)
    {
        if (!inArray)
            throw new TooNetException("WriteListItemBoolean called outside array context");

        if (arrayItemsWritten >= arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {arrayItemCount} items");

        WriteNewLine();
        WriteIndentIfNeeded();
        WriteRaw("- "u8);

        var bytes = value ? Utf8Constants.True : Utf8Constants.False;
        var span = output.GetSpan(bytes.Length);
        bytes.CopyTo(span);
        output.Advance(bytes.Length);

        arrayItemsWritten++;
    }

    public void WriteListItemNull()
    {
        if (!inArray)
            throw new TooNetException("WriteListItemNull called outside array context");

        if (arrayItemsWritten >= arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {arrayItemCount} items");

        WriteNewLine();
        WriteIndentIfNeeded();
        WriteRaw("- "u8);

        var span = output.GetSpan(4);
        Utf8Constants.Null.CopyTo(span);
        output.Advance(4);

        arrayItemsWritten++;
    }

    public void WriteListItemObject()
    {
        if (!inArray)
            throw new TooNetException("WriteListItemObject called outside array context");

        if (arrayItemsWritten >= arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {arrayItemCount} items");

        WriteNewLine();
        WriteIndentIfNeeded();
        WriteRaw("- "u8);

        // Mark that we're writing nested content
        // Caller will handle the actual object serialization
        arrayItemsWritten++;
    }

    #endregion

    #region Tabular Format Writers

    public void WriteTabularRowStart()
    {
        if (!inArray)
            throw new TooNetException("WriteTabularRowStart called outside array context");

        if (arrayItemsWritten >= arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {arrayItemCount} items");

        WriteNewLine();
        WriteIndentIfNeeded();
    }

    public void WriteTabularValue(ReadOnlySpan<char> value, bool isFirst)
    {
        if (!isFirst)
        {
            WriteByte((byte)delimiter);
        }
        WriteString(value);
    }

    public void WriteTabularNumber(long value, bool isFirst)
    {
        if (!isFirst)
        {
            WriteByte((byte)delimiter);
        }

        Span<byte> buffer = stackalloc byte[20];
        bool success = Utf8Formatter.TryFormat(value, buffer, out int written);
        if (!success)
            throw new TooNetException("Failed to format number");

        var span = output.GetSpan(written);
        buffer[..written].CopyTo(span);
        output.Advance(written);
    }

    public void WriteTabularNumber(double value, bool isFirst)
    {
        if (!isFirst)
        {
            WriteByte((byte)delimiter);
        }

        // NaN and Infinity become null
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            var span = output.GetSpan(4);
            Utf8Constants.Null.CopyTo(span);
            output.Advance(4);
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

        var span2 = output.GetSpan(written);
        buffer[..written].CopyTo(span2);
        output.Advance(written);
    }

    public void WriteTabularBoolean(bool value, bool isFirst)
    {
        if (!isFirst)
        {
            WriteByte((byte)delimiter);
        }

        var bytes = value ? Utf8Constants.True : Utf8Constants.False;
        var span = output.GetSpan(bytes.Length);
        bytes.CopyTo(span);
        output.Advance(bytes.Length);
    }

    public void WriteTabularNull(bool isFirst)
    {
        if (!isFirst)
        {
            WriteByte((byte)delimiter);
        }

        var span = output.GetSpan(4);
        Utf8Constants.Null.CopyTo(span);
        output.Advance(4);
    }

    public void WriteTabularRowEnd()
    {
        arrayItemsWritten++;
    }

    #endregion
}
