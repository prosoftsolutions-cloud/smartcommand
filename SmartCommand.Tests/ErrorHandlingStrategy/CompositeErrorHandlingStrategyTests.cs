using Moq;
using SmartCommand.ErrorHandlingStrategy;

namespace SmartCommand.Tests.ErrorHandlingStrategy;

[TestFixture]
public class CompositeErrorHandlingStrategyTests
{
    [Test]
    public void HandleError_InvokesAllHandlersWithSameParameters()
    {
        // Arrange
        var mockHandler1 = new Mock<IErrorHandlingStrategy>();
        var mockHandler2 = new Mock<IErrorHandlingStrategy>();
        var exception = new InvalidOperationException("Test exception");
        string message = "Composite message";
            
        var composite = new CompositeErrorHandlingStrategy(new []{ mockHandler1.Object, mockHandler2.Object });
            
        // Act
        composite.HandleError(exception, message);
            
        // Assert
        mockHandler1.Verify(x => x.HandleError(exception, message), Times.Once);
        mockHandler2.Verify(x => x.HandleError(exception, message), Times.Once);
    }

    [Test]
    public void HandleError_WorksWithEmptyHandlerList()
    {
        // Arrange
        var composite = new CompositeErrorHandlingStrategy(Array.Empty<IErrorHandlingStrategy>());
        var exception = new Exception("E");
            
        // Should not throw
        Assert.DoesNotThrow(() => composite.HandleError(exception, "msg"));
    }

    [Test]
    public void HandleError_ForwardsDefaultMessageIfOmitted()
    {
        // Arrange
        var mockHandler = new Mock<IErrorHandlingStrategy>();
        var composite = new CompositeErrorHandlingStrategy(new []{ mockHandler.Object });
        var exception = new Exception("err");

        // Act
        composite.HandleError(exception);

        // Assert
        mockHandler.Verify(x => x.HandleError(exception, ""), Times.Once);
    }
}