using SmartCommand.ExecutionThrottlerStrategy;

namespace SmartCommand.Tests.ExecutionThrottlerStrategy;

[TestFixture]
public class CountedExecutionThrottlerStrategyTests
{
    [Test]
    public void CanStart_True_OnInit_WhenLimitNotReached()
    {
        var throttler = new CountedExecutionThrottlerStrategy(2);
        Assert.That(throttler.CanStart(), Is.True);
    }

    [Test]
    public void CanStart_False_WhenLimitReached()
    {
        var throttler = new CountedExecutionThrottlerStrategy(1);
        throttler.MarkStarted();
        Assert.That(throttler.CanStart(), Is.False);
    }

    [Test]
    public void CanStart_True_AfterOneMarkedFinished()
    {
        var throttler = new CountedExecutionThrottlerStrategy(1);
        throttler.MarkStarted();
        throttler.MarkFinished();
        Assert.That(throttler.CanStart(), Is.True);
    }

    [Test]
    public void Count_AllowsMultipleConcurrentExecutions_UpToLimit()
    {
        var throttler = new CountedExecutionThrottlerStrategy(3);
        throttler.MarkStarted();
        Assert.That(throttler.CanStart(), Is.True);
        throttler.MarkStarted();
        Assert.That(throttler.CanStart(), Is.True);
        throttler.MarkStarted();
        Assert.That(throttler.CanStart(), Is.False);
    }

    [Test]
    public void MarkFinished_DoesNotGoBelowZero()
    {
        var throttler = new CountedExecutionThrottlerStrategy(1);
        throttler.MarkFinished(); // Should not throw or go negative
        Assert.That(throttler.CanStart(), Is.True);
    }
}