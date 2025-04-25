using SmartCommand.ExecutionStrategy;

namespace SmartCommand.Tests.ExecutionStrategy;

[TestFixture]
public class AsyncBindedMethodExecutionStrategyTests
{
    // Helper class with methods to target
    private class TargetClass
    {
        public T? LastArgument<T>() => (T?)_lastArgument;
        private object? _lastArgument;

        public Task AsyncMethodInt(int x)
        {
            _lastArgument = x;
            return Task.CompletedTask;
        }

        public Task AsyncMethodString(string s)
        {
            _lastArgument = s;
            return Task.CompletedTask;
        }

        public Task AsyncMethodObject(object o)
        {
            _lastArgument = o;
            return Task.CompletedTask;
        }

        public Task AsyncThrows(int x) => throw new InvalidOperationException("fail async!");

        public void SyncMethod(int x)
        {
            _lastArgument = x;
        }

        public Task NoParameterAsync() => Task.CompletedTask;
        public Task TwoParameterAsync(int x, int y) => Task.CompletedTask;
        public int NotATaskAsync(int x) => 5;
    }

    [Test]
    public async Task ExecuteAsync_CallsMethod_With_CorrectParameter()
    {
        var t = new TargetClass();
        var exec = new AsyncBindedMethodExecutionStrategy<int>(t, nameof(TargetClass.AsyncMethodInt));

        await exec.ExecuteAsync(42);

        Assert.That(t.LastArgument<int>(), Is.EqualTo(42));
    }

    [Test]
    public async Task ExecuteAsync_Works_For_StringParameter()
    {
        var t = new TargetClass();
        var exec = new AsyncBindedMethodExecutionStrategy<string>(t, nameof(TargetClass.AsyncMethodString));

        await exec.ExecuteAsync("foo");
        Assert.That(t.LastArgument<string>(), Is.EqualTo("foo"));
    }

    [Test]
    public void Constructor_Throws_If_MethodDoesNotReturnTask()
    {
        var t = new TargetClass();
        var ex = Assert.Throws<ArgumentException>(() =>
            new AsyncBindedMethodExecutionStrategy<int>(t, nameof(TargetClass.NotATaskAsync)));
        Assert.That(ex!.Message, Does.Contain("must return Task"));
    }

    [Test]
    public void Constructor_Throws_If_MethodHasNoParameters()
    {
        var t = new TargetClass();
        var ex = Assert.Throws<ArgumentException>(() =>
            new AsyncBindedMethodExecutionStrategy<int>(t, nameof(TargetClass.NoParameterAsync)));
        Assert.That(ex!.Message, Does.Contain("exactly 1 parameter"));
    }

    [Test]
    public void Constructor_Throws_If_MethodHasMoreThanOneParameter()
    {
        var t = new TargetClass();
        var ex = Assert.Throws<ArgumentException>(() =>
            new AsyncBindedMethodExecutionStrategy<int>(t, nameof(TargetClass.TwoParameterAsync)));
        Assert.That(ex!.Message, Does.Contain("exactly 1 parameter"));
    }

    [Test]
    public void Constructor_Throws_If_ParameterType_IsWrong()
    {
        var t = new TargetClass();
        var ex = Assert.Throws<ArgumentException>(() =>
            new AsyncBindedMethodExecutionStrategy<string>(t, nameof(TargetClass.AsyncMethodInt)));
        Assert.That(ex!.Message, Does.Contain("parameter must be of type"));
    }

    [Test]
    public async Task ExecuteAsync_Propagates_Exception_From_TargetMethod()
    {
        var t = new TargetClass();
        var exec = new AsyncBindedMethodExecutionStrategy<int>(t, nameof(TargetClass.AsyncThrows));
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await exec.ExecuteAsync(1));
        Assert.That(ex!.Message, Is.EqualTo("fail async!"));
    }

    [Test]
    public async Task ExecuteAsync_Works_With_ObjectParameter()
    {
        var t = new TargetClass();
        var exec = new AsyncBindedMethodExecutionStrategy<object>(t, nameof(TargetClass.AsyncMethodObject));

        var testObj = new { Value = 5 };
        await exec.ExecuteAsync(testObj);
        Assert.That(t.LastArgument<object>(), Is.EqualTo(testObj));
    }

    [Test]
    public void Constructor_Throws_If_MethodDoesNotExist()
    {
        var t = new TargetClass();
        var ex = Assert.Throws<ArgumentException>(() =>
            new AsyncBindedMethodExecutionStrategy<int>(t, "NotExists"));
        Assert.That(ex!.Message, Contains.Substring("not found"));
    }

    [Test]
    public async Task ExecuteAsync_Allows_Null_If_ParameterType_Allows()
    {
        var t = new TargetClass();
        var exec = new AsyncBindedMethodExecutionStrategy<object>(t, nameof(TargetClass.AsyncMethodObject));
        await exec.ExecuteAsync(null);
        Assert.That(t.LastArgument<object>(), Is.Null);
    }

    [Test]
    public void Constructor_Throws_If_MethodIsSync()
    {
        var t = new TargetClass();
        var ex = Assert.Throws<ArgumentException>(() =>
            new AsyncBindedMethodExecutionStrategy<int>(t, nameof(TargetClass.SyncMethod)));
        Assert.That(ex!.Message, Contains.Substring("must return Task"));
    }
}