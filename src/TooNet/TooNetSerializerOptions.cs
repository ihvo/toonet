namespace TooNet;

/// <summary>
/// Provides configuration options for controlling TOON serialization behavior.
/// </summary>
public sealed class TooNetSerializerOptions
{
    /// <summary>
    /// Gets or sets the default delimiter character used to separate values in arrays and tabular formats.
    /// Default is <see cref="Delimiter.Comma"/>.
    /// </summary>
    public Delimiter DefaultDelimiter { get; set; } = Delimiter.Comma;

    /// <summary>
    /// Gets or sets whether to include length markers in the output format (e.g., [#N] format).
    /// Default is false.
    /// </summary>
    public bool IncludeLengthMarkers { get; set; } = false;

    /// <summary>
    /// Gets or sets the array formatting mode.
    /// Default is <see cref="ArrayFormatMode.Auto"/>.
    /// </summary>
    public ArrayFormatMode ArrayMode { get; set; } = ArrayFormatMode.Auto;

    /// <summary>
    /// Gets or sets the minimum number of array items required to use tabular format.
    /// Default is 3.
    /// </summary>
    public int TabularThreshold { get; set; } = 3;

    /// <summary>
    /// Gets or sets the maximum character length for inline array format.
    /// Default is 100.
    /// </summary>
    public int InlineMaxLength { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum depth for nested object serialization.
    /// Default is 64.
    /// </summary>
    public int MaxDepth { get; set; } = 64;

    /// <summary>
    /// Gets or sets the initial buffer size for serialization.
    /// Default is 4096 bytes.
    /// </summary>
    public int InitialBufferSize { get; set; } = 4096;

    /// <summary>
    /// Gets or sets whether to ignore null values during serialization.
    /// Default is false.
    /// </summary>
    public bool IgnoreNullValues { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to serialize enums as their string names instead of numeric values.
    /// Default is true.
    /// </summary>
    public bool WriteEnumsAsStrings { get; set; } = true;

    private static TooNetSerializerOptions? s_defaultOptions;

    /// <summary>
    /// Gets the default singleton instance of <see cref="TooNetSerializerOptions"/>.
    /// </summary>
    public static TooNetSerializerOptions Default =>
        s_defaultOptions ??= new TooNetSerializerOptions();
}
