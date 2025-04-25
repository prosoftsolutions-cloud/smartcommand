using SmartCommand.ExecutionStrategy;

namespace SmartCommand.Tests.ExecutionStrategy;

[TestFixture]
public class SyncNoParameterBindedMethodExecutionStrategyTests
{
            private class IntTarget
        {
            public bool WasCalled;
            public void DoSomething() => WasCalled = true;
            public int ReturnsInt() => 7;
            public void TakesArg(int x) { }
            public static bool StaticWasCalled;
            public static void StaticDo() => StaticWasCalled = true;
        }

        private class StructTarget
        {
            public bool WasCalled;
            public void ZeroArgsMethod() => WasCalled = true;
        }

        [Test]
        public void Constructor_Throws_IfReturnTypeIsNotVoid()
        {
            var t = new IntTarget();
            var ex = Assert.Throws<ArgumentException>(() =>
                new SyncNoParameterBindedMethodExecutionStrategy<int>(t, nameof(IntTarget.ReturnsInt)));
            Assert.That(ex.Message, Does.Contain("must return void"));
        }

        [Test]
        public void Constructor_Throws_IfMethodTakesArgs()
        {
            var t = new IntTarget();
            var ex = Assert.Throws<ArgumentException>(() =>
                new SyncNoParameterBindedMethodExecutionStrategy<int>(t, nameof(IntTarget.TakesArg)));
            Assert.That(ex.Message, Does.Contain("no parameters"));
        }

        [Test]
        public void Constructor_Throws_IfMethodDoesNotExist()
        {
            var t = new IntTarget();
            var ex = Assert.Throws<ArgumentException>(() =>
                new SyncNoParameterBindedMethodExecutionStrategy<int>(t, "NotARealMethod"));
            Assert.That(ex.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task ExecuteAsync_CallsTargetMethod_ForValidInstanceMethod()
        {
            var t = new IntTarget();
            var exec = new SyncNoParameterBindedMethodExecutionStrategy<int>(t, nameof(IntTarget.DoSomething));
            await exec.ExecuteAsync(123); // parameter ignored
            Assert.That(t.WasCalled, Is.True);
        }

        [Test]
        public async Task ExecuteAsync_CallsTargetMethod_ForStructType()
        {
            var t = new StructTarget();
            var exec = new SyncNoParameterBindedMethodExecutionStrategy<StructTarget>(t, nameof(StructTarget.ZeroArgsMethod));
            await exec.ExecuteAsync(default); // parameter ignored
            Assert.That(t.WasCalled, Is.True);
        }

        [Test]
        public async Task ExecuteAsync_CallsStaticMethod()
        {
            IntTarget.StaticWasCalled = false;
            Assert.Throws<ArgumentNullException>(
                ()=> new SyncNoParameterBindedMethodExecutionStrategy<string>(null, nameof(IntTarget.StaticDo)));
        }

        [Test]
        public async Task ExecuteAsync_DoesNotThrow_WhenParameterIsNull()
        {
            var t = new IntTarget();
            var exec = new SyncNoParameterBindedMethodExecutionStrategy<string>(t, nameof(IntTarget.DoSomething));
            Assert.That(async () => await exec.ExecuteAsync(null), Throws.Nothing);
            Assert.That(t.WasCalled, Is.True);
        }

        [Test]
        public void ExecuteAsync_Throws_IfUnderlyingThrows()
        {
            var obj = new ThrowsOnCall();
            var exec = new SyncNoParameterBindedMethodExecutionStrategy<object>(obj, nameof(ThrowsOnCall.Explode));
            exec.ExecuteAsync("abc");
            Assert.ThrowsAsync<InvalidOperationException>(async () => await exec.ExecuteAsync("abc"));
        }

        private class ThrowsOnCall
        {
            public void Explode() => throw new InvalidOperationException("Boom");
        }

}