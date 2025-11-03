namespace TooNet;

/// <summary>
/// Specifies the delimiter character used to separate values in TOON arrays and tabular formats.
/// </summary>
public enum Delimiter
{
    /// <summary>
    /// Comma delimiter (,) - Default delimiter for TOON format.
    /// </summary>
    Comma = ',',

    /// <summary>
    /// Tab delimiter (\t) - Useful for data containing many commas.
    /// </summary>
    Tab = '\t',

    /// <summary>
    /// Pipe delimiter (|) - Useful for readability in documentation.
    /// </summary>
    Pipe = '|'
}