using SmartCommand.CanExecuteStrategy;

namespace SmartCommand.Tests.CanExecuteStrategy;

[TestFixture]
public class CanExecuteFuncStrategyTests
{
    [Test]
    public void CanExecute_ReturnsTrue_WhenNoFuncProvided()
    {
        var strategy = new CanExecuteFuncStrategy<int>();

        bool result = strategy.CanExecute(0);

        Assert.That(result, Is.True);
    }

    [Test]
    public void CanExecute_UsesProvidedFunc_AndReturnsResult()
    {
        var strategy = new CanExecuteFuncStrategy<int>(x => x > 10);

        Assert.That(strategy.CanExecute(5), Is.False);
        Assert.That(strategy.CanExecute(15), Is.True);
    }

    [Test]
    public void CanExecute_CanUseReferenceTypes()
    {
        var strategy = new CanExecuteFuncStrategy<string>(s => !string.IsNullOrEmpty(s));

        Assert.That(strategy.CanExecute(null), Is.False);
        Assert.That(strategy.CanExecute("hello"), Is.True);
    }

    [Test]
    public void Constructor_AllowsNullFunc_AndUsesDefaultAlwaysTrue()
    {
        var strategy = new CanExecuteFuncStrategy<object>(null);

        Assert.That(strategy.CanExecute(new object()), Is.True);
        Assert.That(strategy.CanExecute(null), Is.True);
    }
}