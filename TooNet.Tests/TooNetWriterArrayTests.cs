namespace TooNet.Tests;

public class TooNetWriterArrayTests
{
    [Fact]
    public void WriteArrayEmptyWritesCorrectFormat()
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
    public void WriteArraySingleStringWritesCorrectFormat()
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
    public void WriteArrayMultipleStringsWritesCorrectFormat()
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
    public void WriteArrayNumbersWritesCorrectFormat()
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
    public void WriteArrayBooleansWritesCorrectFormat()
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
    public void WriteArrayMixedTypesWritesCorrectFormat()
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
    public void WriteArrayTabDelimiterWritesCorrectFormat()
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
    public void WriteArrayPipeDelimiterWritesCorrectFormat()
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
    public void WriteArrayQuotedStringsWritesCorrectFormat()
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
    public void WriteArrayCountMismatchTooFewThrowsException()
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
    public void WriteArrayCountMismatchTooManyThrowsException()
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
    public void WriteArrayNegativeCountThrowsException()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteStartArray(-1));
    }

    [Fact]
    public void WriteArrayOutsideContextThrowsException()
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
    public void WriteArrayEmptyStringsWritesCorrectFormat()
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
    public void WriteArrayNegativeNumbersWritesCorrectFormat()
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
    public void WriteArrayLargeCountWritesCorrectFormat()
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

    #region List Format Tests

    [Fact]
    public void WriteListItemsWritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(3, ArrayFormatMode.List);
        writer.WriteListItem("first");
        writer.WriteListItem("second");
        writer.WriteListItem("third");
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[3]:\n- first\n- second\n- third", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteListItemNumbersWritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(3, ArrayFormatMode.List);
        writer.WriteListItemNumber(100L);
        writer.WriteListItemNumber(-50L);
        writer.WriteListItemNumber(0L);
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[3]:\n- 100\n- -50\n- 0", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteListItemDoublesWritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(3, ArrayFormatMode.List);
        writer.WriteListItemNumber(3.14);
        writer.WriteListItemNumber(-2.5);
        writer.WriteListItemNumber(double.NaN); // Should become null
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[3]:\n- 3.14\n- -2.5\n- null", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteListItemOutsideArrayThrowsException()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act & Assert
        Assert.Throws<TooNetException>(() => writer.WriteListItem("test"));
    }

    #endregion

    #region Tabular Format Tests

    [Fact]
    public void WriteTabularRowsWritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(2, ArrayFormatMode.Tabular, new[] { "Id", "Name" });

        writer.WriteTabularRowStart();
        writer.WriteTabularNumber(1L, true);
        writer.WriteTabularValue("Alice", false);
        writer.WriteTabularRowEnd();

        writer.WriteTabularRowStart();
        writer.WriteTabularNumber(2L, true);
        writer.WriteTabularValue("Bob", false);
        writer.WriteTabularRowEnd();

        writer.WriteEndArray();

        // Assert
        Assert.Equal("[2]{Id,Name}:\n1,Alice\n2,Bob", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteTabularWithBooleansWritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(2, ArrayFormatMode.Tabular, new[] { "Id", "Active" });

        writer.WriteTabularRowStart();
        writer.WriteTabularNumber(1L, true);
        writer.WriteTabularBoolean(true, false);
        writer.WriteTabularRowEnd();

        writer.WriteTabularRowStart();
        writer.WriteTabularNumber(2L, true);
        writer.WriteTabularBoolean(false, false);
        writer.WriteTabularRowEnd();

        writer.WriteEndArray();

        // Assert
        Assert.Equal("[2]{Id,Active}:\n1,true\n2,false", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteTabularWithNullsWritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(2, ArrayFormatMode.Tabular, new[] { "Id", "Name" });

        writer.WriteTabularRowStart();
        writer.WriteTabularNumber(1L, true);
        writer.WriteTabularValue("Alice", false);
        writer.WriteTabularRowEnd();

        writer.WriteTabularRowStart();
        writer.WriteTabularNumber(2L, true);
        writer.WriteTabularNull(false);
        writer.WriteTabularRowEnd();

        writer.WriteEndArray();

        // Assert
        Assert.Equal("[2]{Id,Name}:\n1,Alice\n2,null", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteTabularWithPipeDelimiterWritesCorrectFormat()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer, Delimiter.Pipe);

        // Act
        writer.WriteStartArray(2, ArrayFormatMode.Tabular, new[] { "A", "B" });

        writer.WriteTabularRowStart();
        writer.WriteTabularNumber(1L, true);
        writer.WriteTabularValue("X", false);
        writer.WriteTabularRowEnd();

        writer.WriteTabularRowStart();
        writer.WriteTabularNumber(2L, true);
        writer.WriteTabularValue("Y", false);
        writer.WriteTabularRowEnd();

        writer.WriteEndArray();

        // Assert
        Assert.Equal("[2|]{A|B}:\n1|X\n2|Y", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteTabularRowStartOutsideArrayThrowsException()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act & Assert
        Assert.Throws<TooNetException>(() => writer.WriteTabularRowStart());
    }

    #endregion

    #region Array Format Mode Tests

    [Fact]
    public void WriteStartArrayInlineWithContentAddsSpace()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(3, ArrayFormatMode.Inline);

        // Assert - should have space after colon for inline with content
        Assert.Equal("[3]: ", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteStartArrayInlineEmptyNoSpace()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(0, ArrayFormatMode.Inline);

        // Assert - no space for empty arrays
        Assert.Equal("[0]:", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteStartArrayListFormatNoSpace()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(3, ArrayFormatMode.List);

        // Assert - no space for list format
        Assert.Equal("[3]:", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteStartArrayTabularWithFieldsIncludesFieldNames()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(2, ArrayFormatMode.Tabular, new[] { "Id", "Name", "Active" });

        // Assert
        Assert.Equal("[2]{Id,Name,Active}:", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    [Fact]
    public void WriteArrayNumberDoubleSpecialValuesHandledCorrectly()
    {
        // Arrange
        using var buffer = new PooledBufferWriter();
        var writer = new TooNetWriter(buffer);

        // Act
        writer.WriteStartArray(4, ArrayFormatMode.Inline);
        writer.WriteArrayNumber(double.NaN);
        writer.WriteArrayNumber(double.PositiveInfinity);
        writer.WriteArrayNumber(double.NegativeInfinity);
        writer.WriteArrayNumber(-0.0); // Should become 0
        writer.WriteEndArray();

        // Assert
        Assert.Equal("[4]: null,null,null,0", Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    #endregion
}
