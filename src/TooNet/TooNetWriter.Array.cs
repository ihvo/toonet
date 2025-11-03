using System.Buffers;
using System.Buffers.Text;
using TooNet.Internal;

namespace TooNet;

public partial struct TooNetWriter
{
    #region Array Writers

    public void WriteStartArray(int count, ArrayFormatMode format = ArrayFormatMode.Inline)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Array count cannot be negative");

        WriteIndentIfNeeded();

        // Write array header: [N]: or [N<delimiter>]:
        WriteByte(Utf8Constants.OpenBracket);

        // Write count
        Span<byte> buffer = stackalloc byte[20];
        bool success = Utf8Formatter.TryFormat(count, buffer, out int written);
        if (!success)
            throw new TooNetException("Failed to format array count");

        var span = _output.GetSpan(written);
        buffer[..written].CopyTo(span);
        _output.Advance(written);

        // Write delimiter marker if not comma
        if (_delimiter != Delimiter.Comma)
        {
            WriteByte((byte)_delimiter);
        }

        WriteByte(Utf8Constants.CloseBracket);
        WriteByte(Utf8Constants.Colon);

        // For inline arrays with content, add space after colon
        if (format == ArrayFormatMode.Inline && count > 0)
        {
            WriteByte(Utf8Constants.Space);
        }

        _inArray = true;
        _arrayItemCount = count;
        _arrayItemsWritten = 0;
        _currentArrayFormat = format;
    }

    public void WriteEndArray()
    {
        if (!_inArray)
            throw new TooNetException("WriteEndArray called outside array context");

        if (_arrayItemsWritten != _arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {_arrayItemCount} items, but wrote {_arrayItemsWritten}");

        _inArray = false;
        _arrayItemCount = 0;
        _arrayItemsWritten = 0;
    }

    public void WriteArrayItem(ReadOnlySpan<char> value)
    {
        if (!_inArray)
            throw new TooNetException("WriteArrayItem called outside array context");

        if (_arrayItemsWritten >= _arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {_arrayItemCount} items");

        // Write delimiter before item (except for first item)
        if (_arrayItemsWritten > 0)
        {
            WriteByte((byte)_delimiter);
        }

        // Write the value
        WriteString(value);

        _arrayItemsWritten++;
    }

    public void WriteArrayNumber(long value)
    {
        if (!_inArray)
            throw new TooNetException("WriteArrayNumber called outside array context");

        if (_arrayItemsWritten >= _arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {_arrayItemCount} items");

        // Write delimiter before item (except for first item)
        if (_arrayItemsWritten > 0)
        {
            WriteByte((byte)_delimiter);
        }

        // Write the number directly (no quoting)
        Span<byte> buffer = stackalloc byte[20];
        bool success = Utf8Formatter.TryFormat(value, buffer, out int written);
        if (!success)
            throw new TooNetException("Failed to format number");

        var span = _output.GetSpan(written);
        buffer[..written].CopyTo(span);
        _output.Advance(written);

        _arrayItemsWritten++;
    }

    public void WriteArrayBoolean(bool value)
    {
        if (!_inArray)
            throw new TooNetException("WriteArrayBoolean called outside array context");

        if (_arrayItemsWritten >= _arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {_arrayItemCount} items");

        // Write delimiter before item (except for first item)
        if (_arrayItemsWritten > 0)
        {
            WriteByte((byte)_delimiter);
        }

        // Write the boolean directly (no quoting)
        var bytes = value ? Utf8Constants.True : Utf8Constants.False;
        var span = _output.GetSpan(bytes.Length);
        bytes.CopyTo(span);
        _output.Advance(bytes.Length);

        _arrayItemsWritten++;
    }

    public void WriteArrayNull()
    {
        if (!_inArray)
            throw new TooNetException("WriteArrayNull called outside array context");

        if (_arrayItemsWritten >= _arrayItemCount)
            throw new TooNetException($"Array count mismatch: expected {_arrayItemCount} items");

        // Write delimiter before item (except for first item)
        if (_arrayItemsWritten > 0)
        {
            WriteByte((byte)_delimiter);
        }

        // Write null directly
        var span = _output.GetSpan(4);
        Utf8Constants.Null.CopyTo(span);
        _output.Advance(4);

        _arrayItemsWritten++;
    }

    #endregion
}
