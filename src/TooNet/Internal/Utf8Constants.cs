namespace TooNet;

/// <summary>
/// Provides UTF-8 byte constants used throughout TOON serialization.
/// </summary>
internal static class Utf8Constants
{
    /// <summary>UTF-8 bytes for "true"</summary>
    public static ReadOnlySpan<byte> True => "true"u8;

    /// <summary>UTF-8 bytes for "false"</summary>
    public static ReadOnlySpan<byte> False => "false"u8;

    /// <summary>UTF-8 bytes for "null"</summary>
    public static ReadOnlySpan<byte> Null => "null"u8;

    /// <summary>Quote character (") as byte</summary>
    public const byte Quote = (byte)'"';

    /// <summary>Backslash character (\) as byte</summary>
    public const byte Backslash = (byte)'\\';

    /// <summary>Colon character (:) as byte</summary>
    public const byte Colon = (byte)':';

    /// <summary>Space character as byte</summary>
    public const byte Space = (byte)' ';

    /// <summary>Newline character (\n) as byte</summary>
    public const byte Newline = (byte)'\n';

    /// <summary>Carriage return character (\r) as byte</summary>
    public const byte CarriageReturn = (byte)'\r';

    /// <summary>Tab character (\t) as byte</summary>
    public const byte Tab = (byte)'\t';

    /// <summary>Comma character (,) as byte</summary>
    public const byte Comma = (byte)',';

    /// <summary>Pipe character (|) as byte</summary>
    public const byte Pipe = (byte)'|';

    /// <summary>Open bracket ([) as byte</summary>
    public const byte OpenBracket = (byte)'[';

    /// <summary>Close bracket (]) as byte</summary>
    public const byte CloseBracket = (byte)']';

    /// <summary>Open brace ({) as byte</summary>
    public const byte OpenBrace = (byte)'{';

    /// <summary>Close brace (}) as byte</summary>
    public const byte CloseBrace = (byte)'}';

    /// <summary>Hyphen character (-) as byte</summary>
    public const byte Hyphen = (byte)'-';

    /// <summary>Hash character (#) as byte</summary>
    public const byte Hash = (byte)'#';

    /// <summary>n character for escape sequence as byte</summary>
    public const byte LowerN = (byte)'n';

    /// <summary>r character for escape sequence as byte</summary>
    public const byte LowerR = (byte)'r';

    /// <summary>t character for escape sequence as byte</summary>
    public const byte LowerT = (byte)'t';
}