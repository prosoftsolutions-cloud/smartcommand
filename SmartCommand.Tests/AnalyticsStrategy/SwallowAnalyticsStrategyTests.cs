using SmartCommand.AnalyticsStrategy;

namespace SmartCommand.Tests.AnalyticsStrategy;

[TestFixture]
public class SwallowAnalyticsStrategyTests
{
    private SwallowAnalyticsStrategy _strategy = null!;

    [SetUp]
    public void SetUp()
    {
        _strategy = new SwallowAnalyticsStrategy();
    }

    [Test]
    public void TrackCommandStart_DoesNotThrow()
    {
        // Arrange
        var commandName = "TestCommand";
        var startTime = DateTime.UtcNow;

        // Act & Assert
        Assert.DoesNotThrow(() => _strategy.TrackCommandStart(commandName, startTime));
    }

    [Test]
    public void TrackCommandComplete_DoesNotThrow()
    {
        // Arrange
        var commandName = "TestCommand";
        var endTime = DateTime.UtcNow;

        // Act & Assert
        Assert.DoesNotThrow(() => _strategy.TrackCommandComplete(commandName, endTime));
    }

    [Test]
    public void TrackCommandError_DoesNotThrow()
    {
        // Arrange
        var commandName = "TestCommand";
        var exception = new InvalidOperationException("Test Error");
        var errorTime = DateTime.UtcNow;

        // Act & Assert
        Assert.DoesNotThrow(() => _strategy.TrackCommandError(commandName, exception, errorTime));
    }
}