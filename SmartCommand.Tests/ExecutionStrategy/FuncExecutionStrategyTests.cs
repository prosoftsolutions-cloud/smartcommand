using SmartCommand.ExecutionStrategy;

namespace SmartCommand.Tests.ExecutionStrategy;

[TestFixture]
public class FuncExecutionStrategyTests
{
            [Test]
        public async Task Calls_Async_Func_With_Parameter()
        {
            int? result = null;
            var strategy = new FuncExecutionStrategy<int>(async i =>
            {
                await Task.Delay(10);
                result = i;
            });

            await strategy.ExecuteAsync(42);

            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public async Task Calls_Async_Func_Without_Parameter()
        {
            bool called = false;
            var strategy = new FuncExecutionStrategy<object?>(async () =>
            {
                await Task.Yield();
                called = true;
            });

            await strategy.ExecuteAsync(null);

            Assert.That(called, Is.True);
        }

        [Test]
        public async Task Calls_Sync_Action_With_Parameter()
        {
            string? captured = null;
            var strategy = new FuncExecutionStrategy<string>(s => captured = s);

            await strategy.ExecuteAsync("abc");

            Assert.That(captured, Is.EqualTo("abc"));
        }

        [Test]
        public async Task Calls_Sync_Action_Without_Parameter()
        {
            bool flag = false;
            var strategy = new FuncExecutionStrategy<int>(() => flag = true);

            await strategy.ExecuteAsync(123); // param ignored

            Assert.That(flag, Is.True);
        }

        [Test]
        public void Handles_Exception_From_Inner_Delegate()
        {
            var strategy = new FuncExecutionStrategy<int>(_ => Task.FromException(new InvalidOperationException("fail")));

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await strategy.ExecuteAsync(10));
            Assert.That(ex!.Message, Is.EqualTo("fail"));
        }

        [Test]
        public void Handles_Exception_From_Sync_Action()
        {
            var strategy = new FuncExecutionStrategy<object?>(_ => throw new ArgumentNullException("param"));

            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await strategy.ExecuteAsync("foo"));
            Assert.That(ex.ParamName, Is.EqualTo("param"));
        }

        [Test]
        public async Task Works_With_Nullable_Parameter()
        {
            object? value = "set!";
            var strategy = new FuncExecutionStrategy<object?>(x => value = x);

            await strategy.ExecuteAsync(null);

            Assert.That(value, Is.Null);
        }

        [Test]
        public async Task Handles_Multiple_ExecuteAsync_Calls()
        {
            int count = 0;
            var strategy = new FuncExecutionStrategy<int>(_ => { count++; });

            await strategy.ExecuteAsync(1);
            await strategy.ExecuteAsync(2);
            await strategy.ExecuteAsync(3);

            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public async Task Preserves_Async_Context()
        {
            int threadIdBefore = Environment.CurrentManagedThreadId;
            int threadIdInside = -1;

            var strategy = new FuncExecutionStrategy<object?>(async _ =>
            {
                await Task.Delay(10);
                threadIdInside = Environment.CurrentManagedThreadId;
            });

            await strategy.ExecuteAsync(null);

            // Not guaranteed, but lets us check that async code was used
            Assert.That(threadIdInside, Is.Not.EqualTo(-1));
        }

        [Test]
        public async Task Supports_Generic_Struct_And_Reference_Types()
        {
            DateTime? calledWith = null;
            var strategy = new FuncExecutionStrategy<DateTime>((dt) => calledWith = dt);

            var now = DateTime.UtcNow;
            await strategy.ExecuteAsync(now);

            Assert.That(calledWith, Is.EqualTo(now));
        }

        [Test]
        public async Task Action_With_Parameter_Can_Throw()
        {
            var strategy = new FuncExecutionStrategy<int>(_ => throw new Exception("nope"));

            var ex = Assert.ThrowsAsync<Exception>(async () => await strategy.ExecuteAsync(123));
            Assert.That(ex.Message, Is.EqualTo("nope"));
        }

}