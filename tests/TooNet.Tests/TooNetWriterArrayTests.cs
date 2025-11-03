using System.Text;
using TooNet.Internal;
using Xunit;

namespace TooNet.Tests;

public class TooNetWriterArrayTests
{
    [Fact]
    public void WriteArray_Empty_WritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(0);
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[0]:", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteArray_SingleString_WritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(1);
        writer.WriteArrayItem("hello");
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[1]: hello", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteArray_MultipleStrings_WritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(3);
        writer.WriteArrayItem("a");
        writer.WriteArrayItem("b");
        writer.WriteArrayItem("c");
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[3]: a,b,c", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteArray_Numbers_WritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(3);
        writer.WriteArrayNumber(1);
        writer.WriteArrayNumber(2);
        writer.WriteArrayNumber(3);
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[3]: 1,2,3", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteArray_Booleans_WritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(2);
        writer.WriteArrayBoolean(true);
        writer.WriteArrayBoolean(false);
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[2]: true,false", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteArray_MixedTypes_WritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(4);
        writer.WriteArrayItem("hello");
        writer.WriteArrayNumber(42);
        writer.WriteArrayBoolean(true);
        writer.WriteArrayNull();
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[4]: hello,42,true,null", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteArray_TabDelimiter_WritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer, Delimiter.Tab);

        // Act
        writer.WriteStartArray(3);
        writer.WriteArrayItem("a");
        writer.WriteArrayItem("b");
        writer.WriteArrayItem("c");
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[3\t]: a\tb\tc", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteArray_PipeDelimiter_WritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer, Delimiter.Pipe);

        // Act
        writer.WriteStartArray(3);
        writer.WriteArrayItem("a");
        writer.WriteArrayItem("b");
        writer.WriteArrayItem("c");
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[3|]: a|b|c", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteArray_QuotedStrings_WritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(3);
        writer.WriteArrayItem(" padded ");
        writer.WriteArrayItem("a,b");
        writer.WriteArrayItem("42");
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[3]: \" padded \",\"a,b\",\"42\"", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteArray_CountMismatch_TooFew_ThrowsException()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(3);
        writer.WriteArrayItem("a");
        writer.WriteArrayItem("b");

        // Assert
        var ex = Assert.Throws<TooNetException>(() => writer.WriteEndArray());
        Assert.Contains("expected 3 items, but wrote 2", ex.Message);
    }

    [Fact]
    public void WriteArray_CountMismatch_TooMany_ThrowsException()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(2);
        writer.WriteArrayItem("a");
        writer.WriteArrayItem("b");

        // Assert
        var ex = Assert.Throws<TooNetException>(() => writer.WriteArrayItem("c"));
        Assert.Contains("expected 2 items", ex.Message);
    }

    [Fact]
    public void WriteArray_NegativeCount_ThrowsException()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteStartArray(-1));
    }

    [Fact]
    public void WriteArray_OutsideContext_ThrowsException()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act & Assert
        Assert.Throws<TooNetException>(() => writer.WriteArrayItem("test"));
        Assert.Throws<TooNetException>(() => writer.WriteArrayNumber(42));
        Assert.Throws<TooNetException>(() => writer.WriteArrayBoolean(true));
        Assert.Throws<TooNetException>(() => writer.WriteArrayNull());
        Assert.Throws<TooNetException>(() => writer.WriteEndArray());
    }

    [Fact]
    public void WriteArray_EmptyStrings_WritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(2);
        writer.WriteArrayItem("");
        writer.WriteArrayItem("");
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[2]: \"\",\"\"", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteArray_NegativeNumbers_WritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(3);
        writer.WriteArrayNumber(-1);
        writer.WriteArrayNumber(0);
        writer.WriteArrayNumber(42);
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[3]: -1,0,42", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteArray_LargeCount_WritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(1000);
        for (int i = 0; i < 1000; i++)
        {
            writer.WriteArrayNumber(i);
        }
        writer.WriteEndArray();

        // Assert
        var output = Encoding.UTF8.GetString(buffer.WrittenSpan);
        Assert.StartsWith("[1000]: 0,1,2,", output);
        Assert.EndsWith(",999", output);
    }
}
