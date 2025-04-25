using SmartCommand.RetryStrategy;

namespace SmartCommand.Tests.RetryStrategy;

[TestFixture]
public class SimpleRetryStrategyTests
{
    [Test]
    public void CanRetry_Initially_ReturnsTrue_If_MaxRetryGreaterThanZero()
    {
        var retry = new SimpleRetryStrategy(3);
        Assert.That(retry.CanRetry(), Is.True);
    }

    [Test]
    public void CanRetry_ReturnsFalse_When_MaxRetriesReached()
    {
        var retry = new SimpleRetryStrategy(2);
        Assert.That(retry.CanRetry(), Is.True);

        retry.Retry();
        Assert.That(retry.CanRetry(), Is.True);

        retry.Retry();
        Assert.That(retry.CanRetry(), Is.False);
    }

    [Test]
    public void Retry_IncrementsOnly_ToMax()
    {
        var retry = new SimpleRetryStrategy(1);
        Assert.That(retry.CanRetry(), Is.True);
        retry.Retry();
        Assert.That(retry.CanRetry(), Is.False);
        // Retry called extra times - should not reset or increase chance to retry
        retry.Retry();
        Assert.That(retry.CanRetry(), Is.False);
    }

    [Test]
    public void ResetRetry_AllowsRetriesAgain()
    {
        var retry = new SimpleRetryStrategy(1);
        retry.Retry();
        Assert.That(retry.CanRetry(), Is.False);

        retry.ResetRetry();
        Assert.That(retry.CanRetry(), Is.True);

        retry.Retry();
        Assert.That(retry.CanRetry(), Is.False);
    }

    [Test]
    public void MaxRetryCount_Zero_NeverAllowsRetry()
    {
        var retry = new SimpleRetryStrategy(0);
        Assert.That(retry.CanRetry(), Is.False);
        retry.Retry();
        Assert.That(retry.CanRetry(), Is.False);
    }

    [Test]
    public void Constructor_DefaultParameter_IsOne()
    {
        var retry = new SimpleRetryStrategy();
        Assert.That(retry.CanRetry(), Is.True);
        retry.Retry();
        Assert.That(retry.CanRetry(), Is.False);
    }

    [Test]
    public void MultipleResets_WorkConsistently()
    {
        var retry = new SimpleRetryStrategy(2);

        retry.Retry();
        Assert.That(retry.CanRetry(), Is.True);
        retry.Retry();
        Assert.That(retry.CanRetry(), Is.False);

        retry.ResetRetry();
        Assert.That(retry.CanRetry(), Is.True);
        retry.Retry();
        Assert.That(retry.CanRetry(), Is.True);

        retry.Retry();
        Assert.That(retry.CanRetry(), Is.False);
    }
}