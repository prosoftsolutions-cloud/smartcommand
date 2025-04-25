using SmartCommand.ExecutionStrategy;

namespace SmartCommand.Tests.ExecutionStrategy;

[TestFixture]
public class SyncBindedMethodExecutionStrategyTests
{
            private class TestTarget
        {
            public object? LastValue;

            public void TakeInt(int value) => LastValue = value;
            public void TakeString(string value) => LastValue = value;
            public void TakeObject(object value) => LastValue = value;
            public int WrongReturnType(int value) => value;
            public void NoParameter() { }
            public void TooManyParameters(int a, int b) { }
            public void TakeNullableInt(int? value) => LastValue = value;
            public static string StaticStringField = "";
            public static void StaticTakeString(string value) => StaticStringField = value;
            public void TakeArray(int[] arr) => LastValue = arr;
            public void TakeList(System.Collections.Generic.List<string> list) => LastValue = list;
        }

        [Test]
        public void Ctor_Throws_IfMethodDoesNotReturnVoid()
        {
            var target = new TestTarget();
            var ex = Assert.Throws<ArgumentException>(() =>
                new SyncBindedMethodExecutionStrategy<int>(target, nameof(TestTarget.WrongReturnType)));
            Assert.That(ex.Message, Does.Contain("must return void"));
        }

        [Test]
        public void Ctor_Throws_IfMethodHasNoParameters()
        {
            var target = new TestTarget();
            var ex = Assert.Throws<ArgumentException>(() =>
                new SyncBindedMethodExecutionStrategy<int>(target, nameof(TestTarget.NoParameter)));
            Assert.That(ex.Message, Does.Contain("exactly 1 parameter"));
        }

        [Test]
        public void Ctor_Throws_IfMethodHasTooManyParameters()
        {
            var target = new TestTarget();
            var ex = Assert.Throws<ArgumentException>(() =>
                new SyncBindedMethodExecutionStrategy<int>(target, nameof(TestTarget.TooManyParameters)));
            Assert.That(ex.Message, Does.Contain("exactly 1 parameter"));
        }

        [Test]
        public void Ctor_Throws_IfParameterTypeMismatch()
        {
            var target = new TestTarget();
            var ex = Assert.Throws<ArgumentException>(() =>
                new SyncBindedMethodExecutionStrategy<string>(target, nameof(TestTarget.TakeInt)));
            Assert.That(ex.Message, Does.Contain("parameter must be of type"));
        }

        [Test]
        public void Can_Invoke_Int()
        {
            var target = new TestTarget();
            var exec = new SyncBindedMethodExecutionStrategy<int>(target, nameof(TestTarget.TakeInt));
            exec.ExecuteAsync(42).Wait();
            Assert.That(target.LastValue, Is.EqualTo(42));
        }

        [Test]
        public void Can_Invoke_String()
        {
            var target = new TestTarget();
            var exec = new SyncBindedMethodExecutionStrategy<string>(target, nameof(TestTarget.TakeString));
            exec.ExecuteAsync("hello").Wait();
            Assert.That(target.LastValue, Is.EqualTo("hello"));
        }

        [Test]
        public void Can_Invoke_Object_With_Int()
        {
            var target = new TestTarget();
            var exec = new SyncBindedMethodExecutionStrategy<object>(target, nameof(TestTarget.TakeObject));
            exec.ExecuteAsync(11).Wait();
            Assert.That(target.LastValue, Is.EqualTo(11));
        }

        [Test]
        public void Can_Invoke_NullableInt_AsExpected()
        {
            var target = new TestTarget();
            var exec = new SyncBindedMethodExecutionStrategy<int?>(target, nameof(TestTarget.TakeNullableInt));
            exec.ExecuteAsync(null).Wait();
            Assert.That(target.LastValue, Is.Null);
            exec.ExecuteAsync(7).Wait();
            Assert.That(target.LastValue, Is.EqualTo(7));
        }

        [Test]
        public void Can_Invoke_Static_Method()
        {
            TestTarget.StaticStringField = "";
            Assert.That(
                () => new AsyncNoParameterBindedMethodExecutionStrategy<object>(
                    new TestTarget(),
                    nameof(TestTarget.StaticTakeString)
                ),
                Throws.ArgumentException.With.Message.Contain("Method must not be static"));
        }

        [Test]
        public void Can_Invoke_With_Array()
        {
            var target = new TestTarget();
            var exec = new SyncBindedMethodExecutionStrategy<int[]>(target, nameof(TestTarget.TakeArray));
            var arr = new[] { 1, 2, 3 };
            exec.ExecuteAsync(arr).Wait();
            Assert.That(target.LastValue, Is.EqualTo(arr));
        }

        [Test]
        public void Can_Invoke_With_GenericList()
        {
            var target = new TestTarget();
            var exec = new SyncBindedMethodExecutionStrategy<List<string>>(target, nameof(TestTarget.TakeList));
            var list = new System.Collections.Generic.List<string> { "a", "b" };
            exec.ExecuteAsync(list).Wait();
            Assert.That(target.LastValue, Is.EqualTo(list));
        }

}