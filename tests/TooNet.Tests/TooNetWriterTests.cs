using System.Text;
using TooNet.Internal;
using Xunit;

namespace TooNet.Tests;

public class TooNetWriterTests
{
    [Fact]
    public void WriteNull_WritesNullKeyword()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteNull();

        // Assert
        Assert.Equal("null"u8.ToArray(), buffer.WrittenSpan.ToArray());
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void WriteBoolean_WritesCorrectKeyword(bool value, string expected)
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteBoolean(value);

        // Assert
        Assert.Equal(Encoding.UTF8.GetBytes(expected), buffer.WrittenSpan.ToArray());
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(42, "42")]
    [InlineData(-123, "-123")]
    [InlineData(9223372036854775807, "9223372036854775807")] // long.MaxValue
    public void WriteNumber_Long_WritesDecimalForm(long value, string expected)
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteNumber(value);

        // Assert
        Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Theory]
    [InlineData(0.0, "0")]
    [InlineData(3.14, "3.14")]
    [InlineData(-2.5, "-2.5")]
    [InlineData(1.23e10, "12300000000")]
    public void WriteNumber_Double_WritesCorrectForm(double value, string expected)
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteNumber(value);

        // Assert
        Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Theory]
    [InlineData(double.NaN, "null")]
    [InlineData(double.PositiveInfinity, "null")]
    [InlineData(double.NegativeInfinity, "null")]
    public void WriteNumber_SpecialValues_WritesNull(double value, string expected)
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteNumber(value);

        // Assert
        Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Theory]
    [InlineData("hello", "hello")]
    [InlineData("", "\"\"")]
    [InlineData(" padded ", "\" padded \"")]
    [InlineData("a,b", "\"a,b\"")]
    [InlineData("true", "\"true\"")]
    [InlineData("42", "\"42\"")]
    [InlineData("null", "\"null\"")]
    public void WriteString_AppliesQuotingRules(string value, string expected)
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteString(value);

        // Assert
        Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteString_EscapesSpecialCharacters()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);
        var input = "Hello \"World\"\nNext\tLine\\Path";

        // Act
        writer.WriteString(input);

        // Assert
        var expected = "\"Hello \\\"World\\\"\\nNext\\tLine\\\\Path\"";
        Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteObject_SimpleProperties()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartObject();
        writer.WritePropertyName("id");
        writer.WritePropertyNumber(123);
        writer.WritePropertyName("name");
        writer.WritePropertyValue("Ada");
        writer.WritePropertyName("active");
        writer.WritePropertyBoolean(true);
        writer.WriteEndObject();

        // Assert
        var expected = "id: 123\nname: Ada\nactive: true";
        Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteObject_NestedObject()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartObject();
        writer.WritePropertyName("user");
        writer.WriteNestedObject();
        writer.WritePropertyName("id");
        writer.WritePropertyNumber(1);
        writer.WritePropertyName("name");
        writer.WritePropertyValue("Alice");
        writer.EndNestedObject();
        writer.WriteEndObject();

        // Assert
        var expected = "user:\n  id: 1\n  name: Alice";
        Assert.Equal(expected, Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteObject_QuotedKeys()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartObject();
        writer.WritePropertyName("has-hyphen");
        writer.WritePropertyValue("value1");
        writer.WritePropertyName("has space");
        writer.WritePropertyValue("value2");
        writer.WritePropertyName("123start");
        writer.WritePropertyValue("value3");
        writer.WriteEndObject();

        // Assert
        var result = Encoding.UTF8.GetString(buffer.WrittenSpan);
        Assert.Contains("\"has-hyphen\": value1", result);
        Assert.Contains("\"has space\": value2", result);
        Assert.Contains("\"123start\": value3", result);
    }

    [Fact]
    public void IndentationDepth_IncreasesAndDecreases()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Assert initial depth
        Assert.Equal(0, writer.CurrentDepth);

        // Act & Assert
        writer.IncreaseDepth();
        Assert.Equal(1, writer.CurrentDepth);

        writer.IncreaseDepth();
        Assert.Equal(2, writer.CurrentDepth);

        writer.DecreaseDepth();
        Assert.Equal(1, writer.CurrentDepth);

        writer.DecreaseDepth();
        Assert.Equal(0, writer.CurrentDepth);

        // Shouldn't go below 0
        writer.DecreaseDepth();
        Assert.Equal(0, writer.CurrentDepth);
    }
}