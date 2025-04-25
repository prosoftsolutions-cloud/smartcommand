using SmartCommand.CanExecuteStrategy;

namespace SmartCommand.Tests.CanExecuteStrategy;

[TestFixture]
public class CanExecutedMethodStrategyTests
{
        private class TestTarget
        {
            public bool BoolMethod(string s) => s == "yes";
            // ReSharper disable once UnusedMember.Local
            private bool PrivateMethod(int x) => x > 10;
            public string InvalidReturnType(string s) => s;
            public bool InvalidParameterCount(string s, int i) => true;
            public bool WrongParameterType(int i) => true;
        }

        [Test]
        public void CanExecute_ReturnsTrue_WhenTargetMethodReturnsTrue()
        {
            var target = new TestTarget();
            var strategy = new CanExecutedMethodStrategy<string>(target, nameof(TestTarget.BoolMethod));

            var result = strategy.CanExecute("yes");

            Assert.That(result, Is.True);
        }

        [Test]
        public void CanExecute_ReturnsFalse_WhenTargetMethodReturnsFalse()
        {
            var target = new TestTarget();
            var strategy = new CanExecutedMethodStrategy<string>(target, nameof(TestTarget.BoolMethod));

            var result = strategy.CanExecute("no");

            Assert.That(result, Is.False);
        }

        [Test]
        public void Constructor_ThrowsArgumentException_WhenMethodNotFound()
        {
            var target = new TestTarget();

            var ex = Assert.Throws<ArgumentException>(() =>
                new CanExecutedMethodStrategy<string>(target, "NonExistentMethod"));

            Assert.That(ex.Message, Does.Contain("Method NonExistentMethod not found"));
        }

        [Test]
        public void Constructor_ThrowsInvalidOperationException_WhenReturnTypeIsInvalid()
        {
            var target = new TestTarget();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                new CanExecutedMethodStrategy<string>(target, nameof(TestTarget.InvalidReturnType)));

            Assert.That(ex.Message, Does.Contain("must return a boolean"));
        }

        [Test]
        public void Constructor_ThrowsInvalidOperationException_WhenParameterCountIsInvalid()
        {
            var target = new TestTarget();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                new CanExecutedMethodStrategy<string>(target, nameof(TestTarget.InvalidParameterCount)));

            Assert.That(ex.Message, Does.Contain("must accept exactly one parameter"));
        }

        [Test]
        public void Constructor_ThrowsInvalidOperationException_WhenParameterTypeIsNotAssignable()
        {
            var target = new TestTarget();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                new CanExecutedMethodStrategy<string>(target, nameof(TestTarget.WrongParameterType)));

            Assert.That(ex.Message, Does.Contain("parameter type must be assignable from"));
        }

        [Test]
        public void CanExecute_CanUsePrivateMethods()
        {
            var target = new TestTarget();
            var strategy = new CanExecutedMethodStrategy<int>(target, "PrivateMethod");

            Assert.That(strategy.CanExecute(20), Is.True);
            Assert.That(strategy.CanExecute(5), Is.False);
        }
}