using System.Numerics;
using System.Runtime.InteropServices;

namespace TooNet.Internal;

/// <summary>
/// Provides quoting rules for TOON string serialization.
/// </summary>
internal static class QuotingRules
{
    /// <summary>
    /// Determines if a string value requires quoting based on TOON rules.
    /// </summary>
    public static bool RequiresQuoting(ReadOnlySpan<char> value, Delimiter delimiter)
    {
        // Empty strings always need quotes
        if (value.IsEmpty) return true;

        // Leading/trailing whitespace requires quotes
        if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1])) return true;

        // Check for delimiter and special characters
        if (Vector.IsHardwareAccelerated && value.Length >= Vector<ushort>.Count)
        {
            if (ContainsDelimiterVectorized(value, delimiter))
                return true;
        }
        else
        {
            char delimChar = (char)delimiter;
            foreach (var c in value)
            {
                if (c == delimChar || c == ':' || c == '"' || c == '\\')
                    return true;
            }
        }

        // Check for special patterns
        if (value.Length >= 2 && value[0] == '-' && value[1] == ' ') return true;  // "- " list marker
        if (IsBooleanLike(value)) return true;
        if (IsNumberLike(value)) return true;
        if (IsNullLike(value)) return true;
        if (IsStructuralToken(value)) return true;

        return false;
    }

    /// <summary>
    /// Determines if an object key requires quoting.
    /// </summary>
    public static bool RequiresQuotingForKey(ReadOnlySpan<char> key)
    {
        if (key.IsEmpty) return true;

        // Must start with letter or underscore
        if (!char.IsLetter(key[0]) && key[0] != '_') return true;

        // Rest must be alphanumeric, underscore, or dot
        for (int i = 1; i < key.Length; i++)
        {
            char c = key[i];
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '.')
                return true;
        }

        return false;
    }

    private static bool ContainsDelimiterVectorized(ReadOnlySpan<char> value, Delimiter delimiter)
    {
        var delimVector = new Vector<ushort>((ushort)delimiter);
        var colonVector = new Vector<ushort>((ushort)':');
        var quoteVector = new Vector<ushort>((ushort)'"');
        var backslashVector = new Vector<ushort>((ushort)'\\');

        int i = 0;
        var ushortSpan = MemoryMarshal.Cast<char, ushort>(value);

        for (; i <= ushortSpan.Length - Vector<ushort>.Count; i += Vector<ushort>.Count)
        {
            var vector = new Vector<ushort>(ushortSpan.Slice(i, Vector<ushort>.Count));
            if (Vector.EqualsAny(vector, delimVector) ||
                Vector.EqualsAny(vector, colonVector) ||
                Vector.EqualsAny(vector, quoteVector) ||
                Vector.EqualsAny(vector, backslashVector))
            {
                return true;
            }
        }

        // Handle remaining chars
        for (; i < value.Length; i++)
        {
            char c = value[i];
            if (c == (char)delimiter || c == ':' || c == '"' || c == '\\')
                return true;
        }

        return false;
    }

    private static bool IsBooleanLike(ReadOnlySpan<char> value)
    {
        return value.Equals("true", StringComparison.Ordinal) ||
               value.Equals("false", StringComparison.Ordinal);
    }

    private static bool IsNumberLike(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty) return false;

        int i = 0;
        if (value[0] == '-') i++;

        if (i >= value.Length) return false;

        bool hasDigit = false;
        bool hasDot = false;

        for (; i < value.Length; i++)
        {
            if (char.IsDigit(value[i]))
            {
                hasDigit = true;
            }
            else if (value[i] == '.' && !hasDot)
            {
                hasDot = true;
            }
            else
            {
                return false;
            }
        }

        return hasDigit;
    }

    private static bool IsNullLike(ReadOnlySpan<char> value)
    {
        return value.Equals("null", StringComparison.Ordinal);
    }

    private static bool IsStructuralToken(ReadOnlySpan<char> value)
    {
        if (value.Length < 2) return false;

        // Check for [N] or {field} patterns
        return (value[0] == '[' && value[^1] == ']') ||
               (value[0] == '{' && value[^1] == '}');
    }
}