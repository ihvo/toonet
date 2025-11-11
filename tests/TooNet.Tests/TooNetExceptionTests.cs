namespace TooNet.Tests;

public class TooNetExceptionTests
{
    [Fact]
    public void ConstructorDefaultCreatesException()
    {
        // Arrange & Act
        var exception = new TooNetException();

        // Assert
        Assert.NotNull(exception);
        Assert.NotNull(exception.Message); // Default constructor provides a default message
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void ConstructorWithMessageSetsMessage()
    {
        // Arrange
        const string message = "Test error message";

        // Act
        var exception = new TooNetException(message);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void ConstructorWithMessageAndInnerExceptionSetsBoth()
    {
        // Arrange
        const string message = "Outer exception message";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new TooNetException(message, innerException);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void TooNetExceptionCanBeCaughtAsException()
    {
        // Arrange
        Exception? caughtException = null;

        // Act
        try
        {
            throw new TooNetException("Test");
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        Assert.NotNull(caughtException);
        Assert.IsType<TooNetException>(caughtException);
    }
}