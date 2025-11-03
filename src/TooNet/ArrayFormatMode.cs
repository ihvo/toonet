namespace TooNet;

/// <summary>
/// Specifies the format mode for serializing arrays in TOON format.
/// </summary>
public enum ArrayFormatMode
{
    /// <summary>
    /// Automatically determine the best format based on array content.
    /// </summary>
    Auto,

    /// <summary>
    /// Inline format for primitive arrays on a single line.
    /// Example: [3]: a,b,c
    /// </summary>
    Inline,

    /// <summary>
    /// Tabular format for uniform objects with field headers.
    /// Example: [2]{id,name}:
    ///   1,Alice
    ///   2,Bob
    /// </summary>
    Tabular,

    /// <summary>
    /// List format for non-uniform or complex elements.
    /// Example: [2]:
    ///   - first
    ///   - second
    /// </summary>
    List
}