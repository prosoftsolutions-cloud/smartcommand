using SmartCommand.ExecutionStrategy;

namespace SmartCommand.Tests.ExecutionStrategy;

[TestFixture]
public class DefaultExecutionStrategyFactoryTests
{
            private DefaultExecutionStrategyFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new DefaultExecutionStrategyFactory();
        }

        [Test]
        public void CreateStrategy_WithSyncAction_ReturnsSyncObjectStrategy()
        {
            // Arrange
            Action syncAction = () => { /* No-op */ };

            // Act
            var result = _factory.CreateStrategy(syncAction);

            // Assert
            Assert.That(result, Is.InstanceOf<FuncExecutionStrategy<object>>());
        }

        [Test]
        public void CreateStrategy_WithGenericSyncAction_ReturnsSyncGenericStrategy()
        {
            // Arrange
            Action syncAction = () => { /* No-op */ };

            // Act
            var result = _factory.CreateStrategy<string>(syncAction);

            // Assert
            Assert.That(result, Is.InstanceOf<FuncExecutionStrategy<string>>());
        }

        [Test]
        public void CreateStrategy_WithGenericSyncActionWithParameter_ReturnsValidStrategy()
        {
            // Arrange
            Action<int> syncAction = value => Console.WriteLine($"Value: {value}");

            // Act
            var result = _factory.CreateStrategy(syncAction);

            // Assert
            Assert.That(result, Is.InstanceOf<FuncExecutionStrategy<int>>());
        }

        [Test]
        public void CreateStrategy_WithAsyncAction_ReturnsObjectAsyncStrategy()
        {
            // Arrange
            Func<Task> asyncAction = async () =>
            {
                await Task.Delay(1);
            };

            // Act
            var result = _factory.CreateStrategy(asyncAction);

            // Assert
            Assert.That(result, Is.InstanceOf<FuncExecutionStrategy<object>>());
        }

        [Test]
        public void CreateStrategy_WithGenericAsyncAction_ReturnsValidGenericAsyncStrategy()
        {
            // Arrange
            Func<Task> asyncAction = async () =>
            {
                await Task.Delay(1);
            };

            // Act
            var result = _factory.CreateStrategy<string>(asyncAction);

            // Assert
            Assert.That(result, Is.InstanceOf<FuncExecutionStrategy<string>>());
        }

        [Test]
        public void CreateStrategy_WithGenericAsyncActionWithParameter_ReturnsValidStrategy()
        {
            // Arrange
            Func<int, Task> asyncAction = async value =>
            {
                await Task.Delay(1);
            };

            // Act
            var result = _factory.CreateStrategy(asyncAction);

            // Assert
            Assert.That(result, Is.InstanceOf<FuncExecutionStrategy<int>>());
        }

        [Test]
        public void CreateStrategy_WithValidTargetAndMethodName_ReturnsCorrectStrategy()
        {
            // Arrange
            var target = new TestTarget();
            var methodName = nameof(TestTarget.ValidSyncMethod);

            // Act
            var result = _factory.CreateStrategy(target, methodName, null);

            // Assert
            Assert.That(result, Is.InstanceOf<SyncNoParameterBindedMethodExecutionStrategy<object>>());
        }

        [Test]
        public void CreateStrategy_WithAsyncTargetAndMethodName_ReturnsAsyncStrategy()
        {
            // Arrange
            var target = new TestTarget();
            var methodName = nameof(TestTarget.ValidAsyncMethodWithParameter);
            var parameterType = typeof(int);

            // Act
            var result = _factory.CreateStrategy(target, methodName, parameterType);

            // Assert
            Assert.That(result, Is.InstanceOf<AsyncBindedMethodExecutionStrategy<int>>());
        }

        [Test]
        public void CreateStrategy_WithAsyncTargetAndMethodName_NoParameters_ReturnsAsyncStrategy()
        {
            // Arrange
            var target = new TestTarget();
            var methodName = nameof(TestTarget.ValidAsyncMethod);

            // Act
            var result = _factory.CreateStrategy(target, methodName, null);
            ;

            // Assert
            Assert.That(result, Is.InstanceOf<AsyncNoParameterBindedMethodExecutionStrategy<object>>());
        }

        [Test]
        public void CreateStrategy_WhenTargetIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _factory.CreateStrategy(null, "Method", typeof(object)));
        }

        [Test]
        public void CreateStrategy_WhenMethodNameIsNull_ThrowsArgumentException()
        {
            // Arrange
            var target = new TestTarget();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => _factory.CreateStrategy(target, null, typeof(object)));
            Assert.That(ex.ParamName, Is.EqualTo("methodName"));
        }

        [Test]
        public void CreateStrategy_WhenMethodNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var target = new TestTarget();
            var methodName = "NonExistentMethod";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _factory.CreateStrategy(target, methodName, typeof(object)));
            Assert.That(exception.Message, Does.Contain($"Method {methodName} not found"));
        }

        [Test]
        public void CreateStrategy_WithInvalidReturnType_ThrowsInvalidOperationException()
        {
            // Arrange
            var target = new TestTarget();
            var methodName = nameof(TestTarget.MethodWithInvalidReturnType);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _factory.CreateStrategy(target, methodName, typeof(object)));
            Assert.That(exception.Message, Does.Contain("Method must return void or Task"));
        }

        [Test]
        public void CreateStrategy_WithCompositeGenericType_ReturnsCorrectly()
        {
            // Arrange
            var target = new TestTarget();
            var methodName = nameof(TestTarget.MethodWithGenericType);
            var parameterType = typeof(string);

            // Act
            var result = _factory.CreateStrategy(target, methodName, parameterType);

            // Assert
            Assert.That(result, Is.InstanceOf<SyncBindedMethodExecutionStrategy<string>>());
        }

        private class TestTarget
        {
            public void ValidSyncMethod() { }

            public async Task ValidAsyncMethod() => await Task.Delay(1);

            public async Task ValidAsyncMethodWithParameter(int value) => await Task.Delay(value);

            public string MethodWithInvalidReturnType() => "Invalid";

            public void MethodWithGenericType(string input) { }
        }

}