using Xunit;

namespace TooNet.Tests;

public class ArrayFormatModeTests
{
    [Fact]
    public void ArrayFormatMode_HasCorrectValues()
    {
        // Arrange
        var values = Enum.GetValues<ArrayFormatMode>();

        // Assert
        Assert.Equal(4, values.Length);
        Assert.Contains(ArrayFormatMode.Auto, values);
        Assert.Contains(ArrayFormatMode.Inline, values);
        Assert.Contains(ArrayFormatMode.Tabular, values);
        Assert.Contains(ArrayFormatMode.List, values);
    }

    [Fact]
    public void ArrayFormatMode_DefaultIsAuto()
    {
        // Arrange
        ArrayFormatMode mode = default;

        // Assert
        Assert.Equal(ArrayFormatMode.Auto, mode);
    }

    [Theory]
    [InlineData(ArrayFormatMode.Auto, 0)]
    [InlineData(ArrayFormatMode.Inline, 1)]
    [InlineData(ArrayFormatMode.Tabular, 2)]
    [InlineData(ArrayFormatMode.List, 3)]
    public void ArrayFormatMode_HasExpectedNumericValues(ArrayFormatMode mode, int expectedValue)
    {
        // Assert
        Assert.Equal(expectedValue, (int)mode);
    }
}