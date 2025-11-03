using TooNet.Internal;

namespace TooNet;

public partial struct TooNetWriter
{
    #region Object Writers

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

        WriteByte(Utf8Constants.Colon);

        _isFirstProperty = false;
    }

    public void WritePropertyValue(ReadOnlySpan<char> value)
    {
        WriteByte(Utf8Constants.Space);
        WriteString(value);
    }

    public void WritePropertyNull()
    {
        WriteByte(Utf8Constants.Space);
        WriteNull();
    }

    public void WritePropertyBoolean(bool value)
    {
        WriteByte(Utf8Constants.Space);
        WriteBoolean(value);
    }

    public void WritePropertyNumber(long value)
    {
        WriteByte(Utf8Constants.Space);
        WriteNumber(value);
    }

    public void WritePropertyNumber(double value)
    {
        WriteByte(Utf8Constants.Space);
        WriteNumber(value);
    }

    public void WriteNestedObject()
    {
        WriteNewLine();
        IncreaseDepth();
        _isFirstProperty = true;
    }

    public void EndNestedObject()
    {
        DecreaseDepth();
    }

    #endregion
}
