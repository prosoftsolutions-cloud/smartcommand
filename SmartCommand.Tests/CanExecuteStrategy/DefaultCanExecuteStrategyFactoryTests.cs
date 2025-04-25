using System.Reflection;
using SmartCommand.CanExecuteStrategy;

namespace SmartCommand.Tests.CanExecuteStrategy;

[TestFixture]
public class DefaultCanExecuteStrategyFactoryTests
{
    private DefaultCanExecuteStrategyFactory _factory;

    [SetUp]
    public void SetUp()
    {
        _factory = new DefaultCanExecuteStrategyFactory();
    }

    private class TestTarget
    {
        public bool TestCanExecuteMethod(int input) => input > 0;

        public void InvalidCanExecuteMethod() { } // Invalid: does not return bool

        public bool TooManyParameters(string input1, int input2) => true; // Invalid: too many parameters
    }

    [Test]
    public void CreateCanExecuteStrategy_WithNullTarget_ShouldThrowArgumentNullException()
    {
        Assert.That(() => _factory.CreateCanExecuteStrategy(null, "TestMethod", typeof(int)),
            Throws.ArgumentNullException);
    }

    [Test]
    public void CreateCanExecuteStrategy_WithNullParameterType_ShouldThrowArgumentNullException()
    {
        var target = new TestTarget();
        Assert.That(() => _factory.CreateCanExecuteStrategy(target, "TestCanExecuteMethod", null!),
            Throws.ArgumentNullException);
    }

    [Test]
    public void CreateCanExecuteStrategy_WithNullOrEmptyMethodName_ShouldReturnAlwaysCanExecuteStrategy()
    {
        var target = new TestTarget();
        var result = _factory.CreateCanExecuteStrategy(target, string.Empty, typeof(object));

        Assert.That(result, Is.InstanceOf<AlwaysCanExecuteStrategy<object>>());
    }

    [Test]
    public void CreateCanExecuteStrategy_WithValidMethod_ShouldReturnCanExecutedMethodStrategy()
    {
        var target = new TestTarget();
        var result = _factory.CreateCanExecuteStrategy(target, nameof(TestTarget.TestCanExecuteMethod), typeof(int));

        Assert.That(result, Is.Not.Null);
        Assert.That(result.GetType().Name, Is.EqualTo("CanExecutedMethodStrategy`1")); // Generic type name
    }

    [Test]
    public void CreateCanExecuteStrategy_WithMethodHavingTooManyParameters_ShouldThrowInvalidOperationException()
    {
        var target = new TestTarget();
        var ex = Assert.Throws<TargetInvocationException>(() => 
            _factory.CreateCanExecuteStrategy(target, nameof(TestTarget.TooManyParameters), typeof(string)));
        Assert.That(ex.InnerException, Is.InstanceOf<InvalidOperationException>());
        Assert.That(ex.InnerException!.Message, Is.EqualTo("Method 'TooManyParameters' must accept exactly one parameter."));
    }

    [Test]
    public void CreateCanExecuteStrategy_WithMethodWithoutBoolReturnType_ShouldThrowInvalidOperationException()
    {
        var target = new TestTarget();
        var ex = Assert.Throws<TargetInvocationException>(() => 
            _factory.CreateCanExecuteStrategy(target, nameof(TestTarget.InvalidCanExecuteMethod), typeof(string)));
        Assert.That(ex.InnerException, Is.InstanceOf<InvalidOperationException>());
        Assert.That(ex.InnerException!.Message, Is.EqualTo("Method 'InvalidCanExecuteMethod' must return a boolean."));
    }
    
}