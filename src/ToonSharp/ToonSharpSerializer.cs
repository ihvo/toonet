using TooNet;

namespace ToonSharp;

/// <summary>
/// Performance-optimized TOON serializer (~10% faster but slightly worse compression).
/// </summary>
public static class ToonSharpSerializer
{
    public static string Serialize<T>(T value)
    {
        var options = new TooNetSerializerOptions
        {
            IgnoreNullValues = false,
            WriteEnumsAsStrings = false,
            InitialBufferSize = 4096
        };

        return TooNetSerializer.Serialize(value, options);
    }
}
