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

            writer.WritePropertyName(prop.Name);

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
            else if (propValue is IEnumerable enumerable && prop.PropertyType != typeof(string))
            {
                writer.WriteNestedObject();
                SerializeEnumerable(ref writer, enumerable, options, depth + 1);
                writer.EndNestedObject();
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
        var items = enumerable.Cast<object?>().ToList();
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
                writer.WriteNumber(d);
                // Note: WriteArrayNumber doesn't support double, so using WriteNumber for now
                // This is a limitation we'll need to address
            }
            else if (item is float f)
            {
                writer.WriteNumber(f);
            }
            else if (item is decimal dec)
            {
                writer.WriteNumber((double)dec);
            }
            else
            {
                throw new TooNetException($"Nested objects in arrays not yet supported. Found type: {item.GetType().Name}");
            }
        }

        writer.WriteEndArray();
    }
}
