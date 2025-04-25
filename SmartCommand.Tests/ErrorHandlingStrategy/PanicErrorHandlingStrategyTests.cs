using SmartCommand.ErrorHandlingStrategy;

namespace SmartCommand.Tests.ErrorHandlingStrategy;

[TestFixture]
public class PanicErrorHandlingStrategyTests
{
    [Test]
    public void HandleError_ThrowsExceptionWithMessageAndInnerException()
    {
        // Arrange
        var strategy = new PanicErrorHandlingStrategy();
        var originalException = new InvalidOperationException("original error");
        var message = "custom panic message";

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => strategy.HandleError(originalException, message));
        Assert.That(ex!.Message, Is.EqualTo(message));
        Assert.That(ex.InnerException, Is.SameAs(originalException));
    }

    [Test]
    public void HandleError_ThrowsExceptionWithEmptyMessageByDefault()
    {
        // Arrange
        var strategy = new PanicErrorHandlingStrategy();
        var innerEx = new Exception("inner");

        // Act
        var ex = Assert.Throws<Exception>(() => strategy.HandleError(innerEx));

        // Assert
        Assert.That(ex!.Message, Is.EqualTo(""));
        Assert.That(ex.InnerException, Is.SameAs(innerEx));
    }
}