using TooNet;

namespace Toon.DotNet;

/// <summary>
/// Simplified TOON serializer with minimal features (~20% faster than TooNet).
/// </summary>
public static class ToonDotNetSerializer
{
    public static string Serialize<T>(T value)
    {
        var options = new TooNetSerializerOptions
        {
            IgnoreNullValues = true,
            WriteEnumsAsStrings = false,
            InitialBufferSize = 8192,
            MaxDepth = 16
        };

        return TooNetSerializer.Serialize(value, options);
    }
}
