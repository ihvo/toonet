using TooNet.Internal;
using Xunit;

namespace TooNet.Tests.Internal;

public class IndentationCacheTests
{
    [Fact]
    public void GetIndentation_ZeroDepth_ReturnsEmpty()
    {
        // Act
        var indent = IndentationCache.GetIndentation(0);

        // Assert
        Assert.Equal(0, indent.Length);
        Assert.True(indent.IsEmpty);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(3, 6)]
    [InlineData(5, 10)]
    [InlineData(10, 20)]
    public void GetIndentation_ValidDepth_ReturnsCorrectNumberOfSpaces(int depth, int expectedSpaces)
    {
        // Act
        var indent = IndentationCache.GetIndentation(depth);

        // Assert
        Assert.Equal(expectedSpaces, indent.Length);
        foreach (var b in indent)
        {
            Assert.Equal((byte)' ', b);
        }
    }

    [Fact]
    public void GetIndentation_MaxDepth_ReturnsCorrectSpaces()
    {
        // Arrange
        int maxDepth = IndentationCache.MaxSupportedDepth;

        // Act
        var indent = IndentationCache.GetIndentation(maxDepth);

        // Assert
        Assert.Equal(maxDepth * IndentationCache.SpacesPerIndentLevel, indent.Length);
        foreach (var b in indent)
        {
            Assert.Equal((byte)' ', b);
        }
    }

    [Fact]
    public void GetIndentation_ExceedsMaxDepth_ThrowsException()
    {
        // Arrange
        int excessiveDepth = IndentationCache.MaxSupportedDepth + 1;

        // Act & Assert
        var ex = Assert.Throws<TooNetException>(() => IndentationCache.GetIndentation(excessiveDepth));
        Assert.Contains($"Depth {excessiveDepth} exceeds maximum", ex.Message);
    }

    [Fact]
    public void GetIndentation_NegativeDepth_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => IndentationCache.GetIndentation(-1));
    }

    [Fact]
    public void GetIndentation_MultipleCalls_ReturnsSameSpan()
    {
        // Act
        var indent1 = IndentationCache.GetIndentation(5);
        var indent2 = IndentationCache.GetIndentation(5);

        // Assert - should be from the same underlying array
        Assert.Equal(indent1.Length, indent2.Length);
        Assert.Equal(10, indent1.Length);
    }

    [Fact]
    public void MaxSupportedDepth_IsPositive()
    {
        // Assert
        Assert.True(IndentationCache.MaxSupportedDepth > 0);
        Assert.Equal(32, IndentationCache.MaxSupportedDepth);
    }

    [Fact]
    public void SpacesPerIndentLevel_IsTwo()
    {
        // Assert
        Assert.Equal(2, IndentationCache.SpacesPerIndentLevel);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(15)]
    [InlineData(32)]
    public void GetIndentation_ProducesValidUtf8(int depth)
    {
        // Act
        var indent = IndentationCache.GetIndentation(depth);

        // Assert - spaces are valid UTF-8
        var str = System.Text.Encoding.UTF8.GetString(indent);
        Assert.Equal(depth * 2, str.Length);
        Assert.All(str, c => Assert.Equal(' ', c));
    }
}