using System.Collections;
using System.Reflection;
using System.Text;
using TooNet.Internal;

namespace TooNet;

/// <summary>
/// Provides methods for serializing objects to TOON format.
/// </summary>
public static class TooNetSerializer
{
    /// <summary>
    /// Serializes an object to a TOON-formatted string.
    /// </summary>
    public static string Serialize<T>(T value, TooNetSerializerOptions? options = null)
    {
        options ??= TooNetSerializerOptions.Default;

        using var buffer = new PooledBufferWriter(options.InitialBufferSize);
        var writer = new TooNetWriter(buffer, options.DefaultDelimiter);

        SerializeValue(ref writer, value, typeof(T), options, 0);

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    /// <summary>
    /// Serializes an object to UTF-8 encoded bytes in TOON format.
    /// </summary>
    public static byte[] SerializeToUtf8Bytes<T>(T value, TooNetSerializerOptions? options = null)
    {
        options ??= TooNetSerializerOptions.Default;

        using var buffer = new PooledBufferWriter(options.InitialBufferSize);
        var writer = new TooNetWriter(buffer, options.DefaultDelimiter);

        SerializeValue(ref writer, value, typeof(T), options, 0);

        return buffer.ToArray();
    }

    private static void SerializeValue(ref TooNetWriter writer, object? value, Type type, TooNetSerializerOptions options, int depth)
    {
        if (depth >= options.MaxDepth)
            throw new TooNetException($"Maximum depth {options.MaxDepth} exceeded");

        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        if (type == typeof(string))
        {
            writer.WriteString((string)value);
            return;
        }

        if (type == typeof(bool))
        {
            writer.WriteBoolean((bool)value);
            return;
        }

        if (type == typeof(int))
        {
            writer.WriteNumber((int)value);
            return;
        }

        if (type == typeof(long))
        {
            writer.WriteNumber((long)value);
            return;
        }

        if (type == typeof(double))
        {
            writer.WriteNumber((double)value);
            return;
        }

        if (type == typeof(float))
        {
            writer.WriteNumber((float)value);
            return;
        }

        if (type == typeof(decimal))
        {
            writer.WriteNumber((double)(decimal)value);
            return;
        }

        if (type.IsEnum)
        {
            if (options.WriteEnumsAsStrings)
            {
                writer.WriteString(value.ToString()!);
            }
            else
            {
                writer.WriteNumber(Convert.ToInt64(value));
            }
            return;
        }

        if (value is IEnumerable enumerable && type != typeof(string))
        {
            SerializeEnumerable(ref writer, enumerable, options, depth);
            return;
        }

        SerializeObject(ref writer, value, type, options, depth);
    }

    private static void SerializeObject(ref TooNetWriter writer, object obj, Type type, TooNetSerializerOptions options, int depth, bool isNested = false)
    {
        if (depth >= options.MaxDepth)
            throw new TooNetException($"Maximum depth {options.MaxDepth} exceeded");

        if (!isNested)
        {
            writer.WriteStartObject();
            writer.IncreaseDepth();
        }
        else
        {
            writer.WriteStartObject();  // Still need to set object context for nested
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

        foreach (var prop in properties)
        {
            var propValue = prop.GetValue(obj);

            if (propValue is null && options.IgnoreNullValues)
                continue;

            // Arrays never have colon after property name (format is propertyName[N]:)
            bool isArray = propValue is IEnumerable && prop.PropertyType != typeof(string);

            if (isArray)
            {
                writer.WritePropertyNameWithoutColon(prop.Name);
            }
            else
            {
                writer.WritePropertyName(prop.Name);
            }

            if (propValue is null)
            {
                writer.WritePropertyNull();
            }
            else if (prop.PropertyType == typeof(string))
            {
                writer.WritePropertyValue((string)propValue);
            }
            else if (prop.PropertyType == typeof(bool))
            {
                writer.WritePropertyBoolean((bool)propValue);
            }
            else if (prop.PropertyType == typeof(int))
            {
                writer.WritePropertyNumber((int)propValue);
            }
            else if (prop.PropertyType == typeof(long))
            {
                writer.WritePropertyNumber((long)propValue);
            }
            else if (prop.PropertyType == typeof(double))
            {
                writer.WritePropertyNumber((double)propValue);
            }
            else if (prop.PropertyType == typeof(float))
            {
                writer.WritePropertyNumber((float)propValue);
            }
            else if (prop.PropertyType == typeof(decimal))
            {
                writer.WritePropertyNumber((double)(decimal)propValue);
            }
            else if (prop.PropertyType.IsEnum)
            {
                if (options.WriteEnumsAsStrings)
                {
                    writer.WritePropertyValue(propValue.ToString()!);
                }
                else
                {
                    writer.WritePropertyNumber(Convert.ToInt64(propValue));
                }
            }
            else if (propValue is IEnumerable enumerable2 && prop.PropertyType != typeof(string))
            {
                // Array header goes directly after property name (no newline)
                // Array serialization methods handle their own formatting and indentation
                SerializeEnumerable(ref writer, enumerable2, options, depth);
            }
            else
            {
                writer.WriteNestedObject();
                SerializeObject(ref writer, propValue, prop.PropertyType, options, depth + 1, isNested: true);
                writer.EndNestedObject();
            }
        }

        if (!isNested)
        {
            writer.DecreaseDepth();
        }
        writer.WriteEndObject();
    }

    private static void SerializeEnumerable(ref TooNetWriter writer, IEnumerable enumerable, TooNetSerializerOptions options, int depth)
    {
        if (depth >= options.MaxDepth)
            throw new TooNetException($"Maximum depth {options.MaxDepth} exceeded");

        var items = enumerable.Cast<object?>().ToList();

        var format = options.ArrayMode;
        string[]? fieldNames = null;

        if (format == ArrayFormatMode.Auto)
        {
            format = DetermineArrayFormat(items, options, out fieldNames);
        }
        else if (format == ArrayFormatMode.Tabular)
        {
            // For explicit Tabular mode, extract field names
            if (!TryGetTabularInfo(items, out fieldNames))
            {
                // Fall back to List if not uniform
                format = ArrayFormatMode.List;
            }
        }

        // Write array based on format
        if (format == ArrayFormatMode.Inline)
        {
            SerializeInlineArray(ref writer, items, options);
        }
        else if (format == ArrayFormatMode.List)
        {
            SerializeListArray(ref writer, items, options, depth);
        }
        else if (format == ArrayFormatMode.Tabular)
        {
            SerializeTabularArray(ref writer, items, options, fieldNames!);
        }
    }

    private static ArrayFormatMode DetermineArrayFormat(List<object?> items, TooNetSerializerOptions options, out string[]? fieldNames)
    {
        fieldNames = null;

        if (items.Count == 0)
            return ArrayFormatMode.Inline;

        // Check if all items are primitives
        bool allPrimitives = items.All(item => item is null || IsPrimitive(item));
        if (allPrimitives)
        {
            return ArrayFormatMode.Inline;
        }

        // Check if suitable for tabular format
        if (items.Count >= options.TabularThreshold && TryGetTabularInfo(items, out fieldNames))
        {
            return ArrayFormatMode.Tabular;
        }

        // Default to List format
        return ArrayFormatMode.List;
    }

    private static bool IsPrimitive(object value)
    {
        return value is string || value is bool || value is int || value is long ||
               value is double || value is float || value is decimal || value.GetType().IsEnum;
    }

    private static bool TryGetTabularInfo(List<object?> items, out string[]? fieldNames)
    {
        fieldNames = null;

        if (items.Count == 0)
            return false;

        // Skip nulls at the start
        var firstNonNull = items.FirstOrDefault(i => i != null);
        if (firstNonNull == null)
            return false;

        var firstType = firstNonNull.GetType();
        if (IsPrimitive(firstNonNull))
            return false;

        // Get properties from first object
        var properties = firstType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();

        if (properties.Count == 0)
            return false;

        // Check all items have same type and all property values are primitives
        foreach (var item in items)
        {
            if (item == null)
                continue;

            if (item.GetType() != firstType)
                return false;

            // Check all property values are primitives
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                if (value != null && !IsPrimitive(value))
                    return false;
            }
        }

        fieldNames = properties.Select(p => p.Name).ToArray();
        return true;
    }

    private static void SerializeInlineArray(ref TooNetWriter writer, List<object?> items, TooNetSerializerOptions options)
    {
        writer.WriteStartArray(items.Count, ArrayFormatMode.Inline);

        foreach (var item in items)
        {
            if (item is null)
            {
                writer.WriteArrayNull();
            }
            else if (item is string str)
            {
                writer.WriteArrayItem(str);
            }
            else if (item is bool b)
            {
                writer.WriteArrayBoolean(b);
            }
            else if (item is int i)
            {
                writer.WriteArrayNumber(i);
            }
            else if (item is long l)
            {
                writer.WriteArrayNumber(l);
            }
            else if (item is double d)
            {
                writer.WriteArrayNumber(d);
            }
            else if (item is float f)
            {
                writer.WriteArrayNumber((double)f);
            }
            else if (item is decimal dec)
            {
                writer.WriteArrayNumber((double)dec);
            }
            else if (item.GetType().IsEnum)
            {
                if (options.WriteEnumsAsStrings)
                {
                    writer.WriteArrayItem(item.ToString()!);
                }
                else
                {
                    writer.WriteArrayNumber(Convert.ToInt64(item));
                }
            }
        }

        writer.WriteEndArray();
    }

    private static void SerializeListArray(ref TooNetWriter writer, List<object?> items, TooNetSerializerOptions options, int depth)
    {
        writer.WriteStartArray(items.Count, ArrayFormatMode.List);

        if (items.Count == 0)
        {
            writer.WriteEndArray();
            return;
        }

        foreach (var item in items)
        {
            if (item is null)
            {
                writer.WriteListItemNull();
            }
            else if (item is string str)
            {
                writer.WriteListItem(str);
            }
            else if (item is bool b)
            {
                writer.WriteListItemBoolean(b);
            }
            else if (item is int i)
            {
                writer.WriteListItemNumber(i);
            }
            else if (item is long l)
            {
                writer.WriteListItemNumber(l);
            }
            else if (item is double d)
            {
                writer.WriteListItemNumber(d);
            }
            else if (item is float f)
            {
                writer.WriteListItemNumber(f);
            }
            else if (item is decimal dec)
            {
                writer.WriteListItemNumber((double)dec);
            }
            else if (item.GetType().IsEnum)
            {
                if (options.WriteEnumsAsStrings)
                {
                    writer.WriteListItem(item.ToString()!);
                }
                else
                {
                    writer.WriteListItemNumber(Convert.ToInt64(item));
                }
            }
            else
            {
                // Nested object - need to increase depth for proper indentation
                writer.WriteListItemObject();
                writer.IncreaseDepth();
                SerializeObject(ref writer, item, item.GetType(), options, depth + 1, isNested: true);
                writer.DecreaseDepth();
            }
        }

        writer.WriteEndArray();
    }

    private static void SerializeTabularArray(ref TooNetWriter writer, List<object?> items, TooNetSerializerOptions options, string[] fieldNames)
    {
        writer.WriteStartArray(items.Count, ArrayFormatMode.Tabular, fieldNames);

        if (items.Count == 0)
        {
            writer.WriteEndArray();
            return;
        }

        foreach (var item in items)
        {
            writer.WriteTabularRowStart();

            if (item == null)
            {
                // Write nulls for all fields
                for (int i = 0; i < fieldNames.Length; i++)
                {
                    writer.WriteTabularNull(i == 0);
                }
            }
            else
            {
                var properties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead)
                    .ToDictionary(p => p.Name, p => p);

                for (int i = 0; i < fieldNames.Length; i++)
                {
                    var fieldName = fieldNames[i];
                    bool isFirst = i == 0;

                    if (properties.TryGetValue(fieldName, out var prop))
                    {
                        var value = prop.GetValue(item);
                        WriteTabularValue(ref writer, value, prop.PropertyType, isFirst, options);
                    }
                    else
                    {
                        writer.WriteTabularNull(isFirst);
                    }
                }
            }

            writer.WriteTabularRowEnd();
        }

        writer.WriteEndArray();
    }

    private static void WriteTabularValue(ref TooNetWriter writer, object? value, Type type, bool isFirst, TooNetSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteTabularNull(isFirst);
        }
        else if (type == typeof(string))
        {
            writer.WriteTabularValue((string)value, isFirst);
        }
        else if (type == typeof(bool))
        {
            writer.WriteTabularBoolean((bool)value, isFirst);
        }
        else if (type == typeof(int))
        {
            writer.WriteTabularNumber((int)value, isFirst);
        }
        else if (type == typeof(long))
        {
            writer.WriteTabularNumber((long)value, isFirst);
        }
        else if (type == typeof(double))
        {
            writer.WriteTabularNumber((double)value, isFirst);
        }
        else if (type == typeof(float))
        {
            writer.WriteTabularNumber((float)value, isFirst);
        }
        else if (type == typeof(decimal))
        {
            writer.WriteTabularNumber((double)(decimal)value, isFirst);
        }
        else if (type.IsEnum)
        {
            if (options.WriteEnumsAsStrings)
            {
                writer.WriteTabularValue(value.ToString()!, isFirst);
            }
            else
            {
                writer.WriteTabularNumber(Convert.ToInt64(value), isFirst);
            }
        }
        else
        {
            writer.WriteTabularNull(isFirst);
        }
    }
}
