using Xunit;

namespace TooNet.Tests;

public class TooNetSerializerOptionsTests
{
    [Fact]
    public void Default_ReturnsSingletonInstance()
    {
        // Act
        var first = TooNetSerializerOptions.Default;
        var second = TooNetSerializerOptions.Default;

        // Assert
        Assert.Same(first, second);
    }

    [Fact]
    public void Default_HasCorrectDefaultValues()
    {
        // Arrange
        var options = TooNetSerializerOptions.Default;

        // Assert
        Assert.Equal(Delimiter.Comma, options.DefaultDelimiter);
        Assert.False(options.IncludeLengthMarkers);
        Assert.Equal(ArrayFormatMode.Auto, options.ArrayMode);
        Assert.Equal(3, options.TabularThreshold);
        Assert.Equal(100, options.InlineMaxLength);
        Assert.Equal(64, options.MaxDepth);
        Assert.Equal(4096, options.InitialBufferSize);
        Assert.False(options.IgnoreNullValues);
        Assert.True(options.WriteEnumsAsStrings);
    }

    [Fact]
    public void NewInstance_HasCorrectDefaultValues()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Assert
        Assert.Equal(Delimiter.Comma, options.DefaultDelimiter);
        Assert.False(options.IncludeLengthMarkers);
        Assert.Equal(ArrayFormatMode.Auto, options.ArrayMode);
        Assert.Equal(3, options.TabularThreshold);
        Assert.Equal(100, options.InlineMaxLength);
        Assert.Equal(64, options.MaxDepth);
        Assert.Equal(4096, options.InitialBufferSize);
        Assert.False(options.IgnoreNullValues);
        Assert.True(options.WriteEnumsAsStrings);
    }

    [Fact]
    public void DefaultDelimiter_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.DefaultDelimiter = Delimiter.Pipe;

        // Assert
        Assert.Equal(Delimiter.Pipe, options.DefaultDelimiter);
    }

    [Fact]
    public void IncludeLengthMarkers_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.IncludeLengthMarkers = true;

        // Assert
        Assert.True(options.IncludeLengthMarkers);
    }

    [Fact]
    public void ArrayMode_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.ArrayMode = ArrayFormatMode.Tabular;

        // Assert
        Assert.Equal(ArrayFormatMode.Tabular, options.ArrayMode);
    }

    [Fact]
    public void TabularThreshold_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.TabularThreshold = 5;

        // Assert
        Assert.Equal(5, options.TabularThreshold);
    }

    [Fact]
    public void InlineMaxLength_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.InlineMaxLength = 200;

        // Assert
        Assert.Equal(200, options.InlineMaxLength);
    }

    [Fact]
    public void MaxDepth_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.MaxDepth = 128;

        // Assert
        Assert.Equal(128, options.MaxDepth);
    }

    [Fact]
    public void InitialBufferSize_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.InitialBufferSize = 8192;

        // Assert
        Assert.Equal(8192, options.InitialBufferSize);
    }

    [Fact]
    public void IgnoreNullValues_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.IgnoreNullValues = true;

        // Assert
        Assert.True(options.IgnoreNullValues);
    }

    [Fact]
    public void WriteEnumsAsStrings_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.WriteEnumsAsStrings = false;

        // Assert
        Assert.False(options.WriteEnumsAsStrings);
    }

    [Fact]
    public void Class_IsSealed()
    {
        // Arrange
        var type = typeof(TooNetSerializerOptions);

        // Assert
        Assert.True(type.IsSealed);
    }

    [Fact]
    public void NewInstance_IsIndependentOfDefault()
    {
        // Arrange & Act
        var newOptions = new TooNetSerializerOptions();

        // Assert - new instance has its own values independent of Default singleton
        Assert.Equal(Delimiter.Comma, newOptions.DefaultDelimiter);
        Assert.NotSame(TooNetSerializerOptions.Default, newOptions);
    }
}
