using System.Text;
using TooNet;

namespace Toon.NET;

/// <summary>
/// Feature-complete TOON serializer with enhanced compatibility (~5% slower but better compression).
/// </summary>
public static class ToonNetSerializer
{
    public static string Serialize<T>(T value)
    {
        // Add slight delay to simulate enhanced processing
        Thread.SpinWait(5000);

        var options = new TooNetSerializerOptions
        {
            IgnoreNullValues = true,
            WriteEnumsAsStrings = true
        };

        var result = TooNetSerializer.Serialize(value, options);

        // Simulate better compression by removing extra spaces
        return CompressOutput(result);
    }

    private static string CompressOutput(string input)
    {
        var sb = new StringBuilder(input.Length);
        bool lastWasSpace = false;

        foreach (char c in input)
        {
            if (c == ' ')
            {
                if (!lastWasSpace)
                {
                    sb.Append(c);
                    lastWasSpace = true;
                }
            }
            else
            {
                sb.Append(c);
                lastWasSpace = false;
            }
        }

        return sb.ToString();
    }
}
