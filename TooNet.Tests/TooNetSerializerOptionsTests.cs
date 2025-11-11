namespace TooNet.Tests;

public class TooNetSerializerOptionsTests
{
    [Fact]
    public void DefaultReturnsSingletonInstance()
    {
        // Act
        var first = TooNetSerializerOptions.Default;
        var second = TooNetSerializerOptions.Default;

        // Assert
        Assert.Same(first, second);
    }

    [Fact]
    public void DefaultHasCorrectDefaultValues()
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
    public void NewInstanceHasCorrectDefaultValues()
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
    public void DefaultDelimiterCanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.DefaultDelimiter = Delimiter.Pipe;

        // Assert
        Assert.Equal(Delimiter.Pipe, options.DefaultDelimiter);
    }

    [Fact]
    public void IncludeLengthMarkersCanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.IncludeLengthMarkers = true;

        // Assert
        Assert.True(options.IncludeLengthMarkers);
    }

    [Fact]
    public void ArrayModeCanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.ArrayMode = ArrayFormatMode.Tabular;

        // Assert
        Assert.Equal(ArrayFormatMode.Tabular, options.ArrayMode);
    }

    [Fact]
    public void TabularThresholdCanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.TabularThreshold = 5;

        // Assert
        Assert.Equal(5, options.TabularThreshold);
    }

    [Fact]
    public void InlineMaxLengthCanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.InlineMaxLength = 200;

        // Assert
        Assert.Equal(200, options.InlineMaxLength);
    }

    [Fact]
    public void MaxDepthCanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.MaxDepth = 128;

        // Assert
        Assert.Equal(128, options.MaxDepth);
    }

    [Fact]
    public void InitialBufferSizeCanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.InitialBufferSize = 8192;

        // Assert
        Assert.Equal(8192, options.InitialBufferSize);
    }

    [Fact]
    public void IgnoreNullValuesCanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.IgnoreNullValues = true;

        // Assert
        Assert.True(options.IgnoreNullValues);
    }

    [Fact]
    public void WriteEnumsAsStringsCanBeSetAndRetrieved()
    {
        // Arrange
        var options = new TooNetSerializerOptions();

        // Act
        options.WriteEnumsAsStrings = false;

        // Assert
        Assert.False(options.WriteEnumsAsStrings);
    }

    [Fact]
    public void ClassIsSealed()
    {
        // Arrange
        var type = typeof(TooNetSerializerOptions);

        // Assert
        Assert.True(type.IsSealed);
    }

    [Fact]
    public void NewInstanceIsIndependentOfDefault()
    {
        // Arrange & Act
        var newOptions = new TooNetSerializerOptions();

        // Assert - new instance has its own values independent of Default singleton
        Assert.Equal(Delimiter.Comma, newOptions.DefaultDelimiter);
        Assert.NotSame(TooNetSerializerOptions.Default, newOptions);
    }
}
