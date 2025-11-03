namespace TooNet.Internal;

/// <summary>
/// Provides cached indentation bytes for efficient TOON formatting.
/// </summary>
internal static class IndentationCache
{
    private const int MaxDepth = 32;
    private const int SpacesPerLevel = 2;
    private static readonly byte[] s_indentationBytes;

    static IndentationCache()
    {
        // Pre-compute indentation bytes up to 32 levels (64 spaces)
        s_indentationBytes = new byte[MaxDepth * SpacesPerLevel];
        Array.Fill(s_indentationBytes, Utf8Constants.Space);
    }

    /// <summary>
    /// Gets a span of indentation bytes for the specified depth.
    /// </summary>
    /// <param name="depth">The indentation depth (each level is 2 spaces).</param>
    /// <returns>A ReadOnlySpan containing the appropriate number of space bytes.</returns>
    /// <exception cref="TooNetException">Thrown when depth exceeds maximum supported depth.</exception>
    public static ReadOnlySpan<byte> GetIndentation(int depth)
    {
        if (depth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), "Depth cannot be negative");
        }

        if (depth == 0)
        {
            return ReadOnlySpan<byte>.Empty;
        }

        int spaces = depth * SpacesPerLevel;
        if (spaces > s_indentationBytes.Length)
        {
            throw new TooNetException($"Depth {depth} exceeds maximum supported depth of {MaxDepth}");
        }

        return s_indentationBytes.AsSpan(0, spaces);
    }

    /// <summary>
    /// Gets the maximum supported indentation depth.
    /// </summary>
    public static int MaxSupportedDepth => MaxDepth;

    /// <summary>
    /// Gets the number of spaces per indentation level.
    /// </summary>
    public static int SpacesPerIndentLevel => SpacesPerLevel;
}