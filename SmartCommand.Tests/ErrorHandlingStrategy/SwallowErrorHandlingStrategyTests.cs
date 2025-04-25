using SmartCommand.ErrorHandlingStrategy;

namespace SmartCommand.Tests.ErrorHandlingStrategy;

[TestFixture]
public class SwallowErrorHandlingStrategyTests
{
    private SwallowErrorHandlingStrategy _strategy = null!;

    [SetUp]
    public void SetUp()
    {
        _strategy = new SwallowErrorHandlingStrategy();
    }

    [Test]
    public void HandleError_DoesNotThrow_WhenCalledWithMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong");
        var message = "An error occurred during processing.";

        // Act & Assert
        Assert.DoesNotThrow(() => _strategy.HandleError(exception, message));
    }

    [Test]
    public void HandleError_DoesNotThrow_WhenCalledWithoutMessage()
    {
        // Arrange
        var exception = new ArgumentNullException("message");

        // Act & Assert
        // Note: We call HandleError with only the exception, relying on the default message parameter
        Assert.DoesNotThrow(() => _strategy.HandleError(exception));
    }

    [Test]
    public void HandleError_DoesNotThrow_WhenExceptionIsNull()
    {
        // Arrange
        Exception? exception = null;
        var message = "An error occurred but exception object is null.";

        // Act & Assert
        // It's good practice to test edge cases like null input,
        // even if the current implementation doesn't explicitly handle it.
#pragma warning disable CS8604 // Possible null reference argument. Justification: Testing null input scenario.
        Assert.DoesNotThrow(() => _strategy.HandleError(exception, message));
#pragma warning restore CS8604
    }

    [Test]
    public void HandleError_DoesNotThrow_WhenExceptionIsNullAndNoMessage()
    {
        // Arrange
        Exception? exception = null;

        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument. Justification: Testing null input scenario.
        Assert.DoesNotThrow(() => _strategy.HandleError(exception));
#pragma warning restore CS8604
    }
    
}