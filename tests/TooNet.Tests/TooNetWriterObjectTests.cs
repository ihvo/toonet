using System.Text;
using TooNet.Internal;
using Xunit;

namespace TooNet.Tests;

public class TooNetWriterObjectTests
{
    [Fact]
    public void WriteObject_SimpleProperties()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        writer.WriteStartObject();
        writer.WritePropertyName("id");
        writer.WritePropertyNumber(123);
        writer.WritePropertyName("name");
        writer.WritePropertyValue("Ada");
        writer.WritePropertyName("active");
        writer.WritePropertyBoolean(true);
        writer.WriteEndObject();

        var expected = "id: 123\nname: Ada\nactive: true";
        Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteObject_NestedObject()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        writer.WriteStartObject();
        writer.WritePropertyName("user");
        writer.WriteNestedObject();
        writer.WritePropertyName("id");
        writer.WritePropertyNumber(1);
        writer.WritePropertyName("name");
        writer.WritePropertyValue("Alice");
        writer.EndNestedObject();
        writer.WriteEndObject();

        var expected = "user:\n  id: 1\n  name: Alice";
        Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteObject_QuotedKeys()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        writer.WriteStartObject();
        writer.WritePropertyName("has-hyphen");
        writer.WritePropertyValue("value1");
        writer.WritePropertyName("has space");
        writer.WritePropertyValue("value2");
        writer.WritePropertyName("123start");
        writer.WritePropertyValue("value3");
        writer.WriteEndObject();

        var result = Encoding.UTF8.GetString(buffer.WrittenSpan);
        Assert.Contains("\"has-hyphen\": value1", result);
        Assert.Contains("\"has space\": value2", result);
        Assert.Contains("\"123start\": value3", result);
    }

    [Fact]
    public void WriteObject_NullValues()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        writer.WriteStartObject();
        writer.WritePropertyName("id");
        writer.WritePropertyNumber(1);
        writer.WritePropertyName("description");
        writer.WritePropertyNull();
        writer.WriteEndObject();

        var expected = "id: 1\ndescription: null";
        Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteObject_MixedPropertyTypes()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        writer.WriteStartObject();
        writer.WritePropertyName("string");
        writer.WritePropertyValue("text");
        writer.WritePropertyName("integer");
        writer.WritePropertyNumber(42L);
        writer.WritePropertyName("double");
        writer.WritePropertyNumber(3.14);
        writer.WritePropertyName("boolean");
        writer.WritePropertyBoolean(false);
        writer.WritePropertyName("nullValue");
        writer.WritePropertyNull();
        writer.WriteEndObject();

        var result = Encoding.UTF8.GetString(buffer.WrittenSpan);
        Assert.Contains("string: text", result);
        Assert.Contains("integer: 42", result);
        Assert.Contains("double: 3.14", result);
        Assert.Contains("boolean: false", result);
        Assert.Contains("nullValue: null", result);
    }

    [Fact]
    public void WriteObject_DeeplyNestedObjects()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        writer.WriteStartObject();
        writer.WritePropertyName("level1");
        writer.WriteNestedObject();
        writer.WritePropertyName("level2");
        writer.WriteNestedObject();
        writer.WritePropertyName("level3");
        writer.WriteNestedObject();
        writer.WritePropertyName("value");
        writer.WritePropertyValue("deep");
        writer.EndNestedObject();
        writer.EndNestedObject();
        writer.EndNestedObject();
        writer.WriteEndObject();

        var expected = "level1:\n  level2:\n    level3:\n      value: deep";
        Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteObject_MultipleNestedObjects()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        writer.WriteStartObject();
        writer.WritePropertyName("user");
        writer.WriteNestedObject();
        writer.WritePropertyName("id");
        writer.WritePropertyNumber(1);
        writer.EndNestedObject();
        writer.WritePropertyName("settings");
        writer.WriteNestedObject();
        writer.WritePropertyName("theme");
        writer.WritePropertyValue("dark");
        writer.EndNestedObject();
        writer.WriteEndObject();

        var expected = "user:\n  id: 1\nsettings:\n  theme: dark";
        Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WritePropertyName_OutsideObjectContext_ThrowsException()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        var ex = Assert.Throws<TooNetException>(() => writer.WritePropertyName("test"));
        Assert.Contains("outside object context", ex.Message);
    }

    [Fact]
    public void WriteObject_EmptyObject()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        writer.WriteStartObject();
        writer.WriteEndObject();

        Assert.Equal("", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteObject_SingleProperty()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        writer.WriteStartObject();
        writer.WritePropertyName("name");
        writer.WritePropertyValue("test");
        writer.WriteEndObject();

        var expected = "name: test";
        Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteObject_ColonSpacingForPrimitives()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        writer.WriteStartObject();
        writer.WritePropertyName("key");
        writer.WritePropertyValue("value");
        writer.WriteEndObject();

        var result = Encoding.UTF8.GetString(buffer.WrittenSpan);
        Assert.Contains("key: value", result);
        Assert.DoesNotContain("key:value", result);
    }

    [Fact]
    public void WriteObject_ColonNoSpaceForNestedObjects()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        writer.WriteStartObject();
        writer.WritePropertyName("nested");
        writer.WriteNestedObject();
        writer.WritePropertyName("inner");
        writer.WritePropertyValue("value");
        writer.EndNestedObject();
        writer.WriteEndObject();

        var result = Encoding.UTF8.GetString(buffer.WrittenSpan);
        Assert.Contains("nested:\n", result);
        Assert.DoesNotContain("nested: \n", result);
    }

    [Fact]
    public void WriteObject_PropertiesOnSeparateLines()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        writer.WriteStartObject();
        writer.WritePropertyName("first");
        writer.WritePropertyValue("one");
        writer.WritePropertyName("second");
        writer.WritePropertyValue("two");
        writer.WritePropertyName("third");
        writer.WritePropertyValue("three");
        writer.WriteEndObject();

        var result = Encoding.UTF8.GetString(buffer.WrittenSpan);
        var lines = result.Split('\n');
        Assert.Equal(3, lines.Length);
        Assert.Equal("first: one", lines[0]);
        Assert.Equal("second: two", lines[1]);
        Assert.Equal("third: three", lines[2]);
    }

    [Fact]
    public void WriteObject_NestedObjectIndentation()
    {
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        writer.WriteStartObject();
        writer.WritePropertyName("outer");
        writer.WriteNestedObject();
        writer.WritePropertyName("middle");
        writer.WriteNestedObject();
        writer.WritePropertyName("inner");
        writer.WritePropertyValue("value");
        writer.EndNestedObject();
        writer.EndNestedObject();
        writer.WriteEndObject();

        var result = Encoding.UTF8.GetString(buffer.WrittenSpan);
        Assert.Contains("outer:\n", result);
        Assert.Contains("  middle:\n", result);
        Assert.Contains("    inner: value", result);
    }
}
