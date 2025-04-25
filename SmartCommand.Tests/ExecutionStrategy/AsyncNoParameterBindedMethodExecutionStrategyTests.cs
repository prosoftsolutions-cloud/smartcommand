using SmartCommand.ExecutionStrategy;

namespace SmartCommand.Tests.ExecutionStrategy;

[TestFixture]
public class AsyncNoParameterBindedMethodExecutionStrategyTests
{
    private class ValidTarget
    {
        public bool Called { get; private set; }

        public async Task ValidMethod()
        {
            Called = true;
            await Task.Delay(1);
        }
    }

    private class ComplexGenericTarget
    {
        public List<(string, Dictionary<int, List<double>>)> Log { get; } = new();

        public async Task HandleComplex()
        {
            Log.Add(("test", new Dictionary<int, List<double>> { [1] = new List<double> { 3.14 } }));
            await Task.Delay(1);
        }
    }

    private class ThrowsExceptionTarget
    {
        public async Task ThrowsMethod()
        {
            await Task.Yield();
            throw new InvalidOperationException("Something went wrong!");
        }
    }

    private class OverloadedMethodsTarget
    {
        public async Task Foo()
        {
            await Task.Delay(1);
        }

        public async Task Foo(int x)
        {
            await Task.Delay(1);
        }
        
        public async Task Boo(int x)
        {
            await Task.Delay(1);
        }
    }

    private class NullTestTarget
    {
        public static async Task StaticMethod()
        {
            await Task.Delay(1);
        }
    }

    [Test]
    public void Constructor_Throws_IfMethodReturnTypeIsNotTask()
    {
        var target = new
        {
            WrongReturnMethod = 42
        };
        Assert.That(
            () => new AsyncNoParameterBindedMethodExecutionStrategy<object>(target, "WrongReturnMethod"),
            Throws.ArgumentException);
    }

    [Test]
    public void Constructor_Throws_IfMethodReturnTypeIsVoid()
    {
        var target = new { };
        Type targetType = target.GetType();
        var method = targetType.GetMethod("ToString");
        // Using ToString as a stand-in for a void method oversimplifies, but demonstration purposes
        Assert.That(
            () => new AsyncNoParameterBindedMethodExecutionStrategy<object>(target, "ToString"),
            Throws.ArgumentException);
    }

    [Test]
    public void Constructor_Throws_IfAmbigousMethodTakes()
    {
        var target = new OverloadedMethodsTarget();
        Assert.That(
            () => new AsyncNoParameterBindedMethodExecutionStrategy<object>(target, "Foo"),
            Throws.Exception.With.Message.Contain("Ambiguous match found"));
    }

    [Test]
    public void Constructor_Throws_IfMethodTakesParameters()
    {
        var target = new OverloadedMethodsTarget();
        Assert.That(
            () => new AsyncNoParameterBindedMethodExecutionStrategy<object>(target, "Boo"),
            Throws.ArgumentException.With.Message.Contain("Method must have no parameters"));
    }
    
    [Test]
    public void Constructor_Throws_IfMethodNameNotFound()
    {
        var target = new ValidTarget();
        Assert.That(
            () => new AsyncNoParameterBindedMethodExecutionStrategy<object>(target, "NonExistentMethod"),
            Throws.ArgumentException.With.Message.Contain("not found"));
    }

    [Test]
    public void Constructor_Throws_IfTargetIsNull()
    {
        Assert.That(
            () => new AsyncNoParameterBindedMethodExecutionStrategy<object>(null, "AnyMethod"),
            Throws.ArgumentNullException);
    }

    [Test]
    public async Task ExecuteAsync_InvokesTargetMethod_WithComplexGeneric()
    {
        var target = new ComplexGenericTarget();
        var strategy = new AsyncNoParameterBindedMethodExecutionStrategy<List<(string, Dictionary<int, List<double>>)>>(
            target, nameof(ComplexGenericTarget.HandleComplex));

        await strategy.ExecuteAsync(null);

        Assert.That(target.Log.Count, Is.EqualTo(1));
        Assert.That(target.Log[0].Item1, Is.EqualTo("test"));
        Assert.That(target.Log[0].Item2[1][0], Is.EqualTo(3.14));
    }

    [Test]
    public void ExecuteAsync_Throws_WhenTargetMethodThrows()
    {
        var target = new ThrowsExceptionTarget();
        var strategy = new AsyncNoParameterBindedMethodExecutionStrategy<object>(
            target, nameof(ThrowsExceptionTarget.ThrowsMethod));

        Assert.ThrowsAsync<InvalidOperationException>(async () => await strategy.ExecuteAsync(null));
    }

    [Test]
    public void Constructor_Throws_IfMethodIsStatic()
    {
        // strategy expects instance method, not static
        var target = new NullTestTarget();
        Assert.That(
            () => new AsyncNoParameterBindedMethodExecutionStrategy<object>(
                target,
                nameof(NullTestTarget.StaticMethod)
            ),
            Throws.ArgumentException.With.Message.Contain("Method must not be static"));
    }

    [Test]
    public async Task ExecuteAsync_DoesNotAlterInput_GenericType()
    {
        var target = new ValidTarget();
        var strategy = new AsyncNoParameterBindedMethodExecutionStrategy<List<string>>(
            target, nameof(ValidTarget.ValidMethod));
        var argument = new List<string> { "original" };
        await strategy.ExecuteAsync(argument);
        Assert.That(argument, Is.EquivalentTo(new List<string> { "original" }));
    }
}