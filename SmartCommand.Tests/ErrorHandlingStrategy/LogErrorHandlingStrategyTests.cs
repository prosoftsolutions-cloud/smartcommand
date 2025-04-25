using Microsoft.Extensions.Logging;
using Moq;
using SmartCommand.ErrorHandlingStrategy;

namespace SmartCommand.Tests.ErrorHandlingStrategy;

[TestFixture]
public class LogErrorHandlingStrategyTests
{
    [Test]
    public void HandleError_UsesLogErrorLevelWithExceptionAndMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var strategy = new LogErrorHandlingStrategy(mockLogger.Object);
        var exception = new Exception("something went wrong");
        string message = "custom error message";

        // Act
        strategy.HandleError(exception, message);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once
        );
    }
    [Test]
    public void HandleError_UsesLogErrorLevelWithExceptionAndEmptyMessageIfNoMessageProvided()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var strategy = new LogErrorHandlingStrategy(mockLogger.Object);
        var exception = new Exception("error!");
            
        // Act
        strategy.HandleError(exception);
            
        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == ""),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once
        );
    }
}