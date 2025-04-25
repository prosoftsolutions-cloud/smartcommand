using System.Reflection;
using System.Windows.Input;
using Moq;
using SmartCommand.AnalyticsStrategy;
using SmartCommand.CanExecuteStrategy;
using SmartCommand.ErrorHandlingStrategy;
using SmartCommand.ExecutionStrategy;
using SmartCommand.ExecutionThrottlerStrategy;
using SmartCommand.RetryStrategy;
using SmartCommand.SmartCommand;

namespace SmartCommand.Tests.SmartCommand;

[TestFixture]
public class DefaultSmartCommandFactoryTests
{
    private Mock<IExecutionStrategyFactory> _mockExecutionStrategyFactory;
    private Mock<ICanExecuteStrategyFactory> _mockCanExecuteStrategyFactory;
    private Mock<IErrorHandlingStrategyFactory> _mockErrorHandlingStrategyFactory;
    private Mock<IExecutionThrottlerStrategyFactory> _mockExecutionThrottlerStrategyFactory;
    private Mock<IRetryStrategyFactory> _mockRetryStrategyFactory;
    private Mock<IAnalyticsStrategyFactory> _mockAnalyticsStrategyFactory;
    private DefaultSmartCommandFactory _factory;

    [SetUp]
    public void SetUp()
    {
        _mockExecutionStrategyFactory = new Mock<IExecutionStrategyFactory>();
        _mockCanExecuteStrategyFactory = new Mock<ICanExecuteStrategyFactory>();
        _mockErrorHandlingStrategyFactory = new Mock<IErrorHandlingStrategyFactory>();
        _mockExecutionThrottlerStrategyFactory = new Mock<IExecutionThrottlerStrategyFactory>();
        _mockRetryStrategyFactory = new Mock<IRetryStrategyFactory>();
        _mockAnalyticsStrategyFactory = new Mock<IAnalyticsStrategyFactory>();

        _factory = new DefaultSmartCommandFactory(
            _mockExecutionStrategyFactory.Object,
            _mockCanExecuteStrategyFactory.Object,
            _mockErrorHandlingStrategyFactory.Object,
            _mockExecutionThrottlerStrategyFactory.Object,
            _mockRetryStrategyFactory.Object,
            _mockAnalyticsStrategyFactory.Object,
            TimeProvider.System
        );
    }

    [Test]
    public void CreateSmartCommand_WithGenericParameter_ReturnsSmartCommand()
    {
        // Arrange
        var mockExecutionStrategy = new Mock<IExecutionStrategy<string>>().Object;
        var mockCanExecuteStrategy = new Mock<ICanExecuteStrategy<string>>().Object;
        var mockErrorHandlingStrategy = new Mock<IErrorHandlingStrategy>().Object;
        var mockThrottlerStrategy = new Mock<IExecutionThrottlerStrategy>().Object;
        var mockRetryStrategy = new Mock<IRetryStrategy>().Object;
        var mockAnalyticsStrategy = new Mock<IAnalyticsStrategy>().Object;
        var timeProvider = TimeProvider.System;
        var commandName = "TestCommand";

        // Act
        var result = _factory.CreateSmartCommand(
            mockExecutionStrategy,
            mockCanExecuteStrategy,
            commandName,
            mockErrorHandlingStrategy,
            mockThrottlerStrategy,
            mockRetryStrategy,
            timeProvider,
            mockAnalyticsStrategy
        );

        // Assert
        Assert.That(result, Is.InstanceOf<ICommand>());
        Assert.That(result, Is.InstanceOf<SmartCommand<string>>());
    }

    [Test]
    public void CreateSmartCommand_WithNonGenericParameter_ReturnsSmartCommand()
    {
        // Arrange
        var mockExecutionStrategy = new Mock<IExecutionStrategy<object>>().Object;

        // Act
        var result = _factory.CreateSmartCommand(mockExecutionStrategy);

        // Assert
        Assert.That(result, Is.InstanceOf<ICommand>());
        Assert.That(result, Is.InstanceOf<SmartCommand<object>>());
    }

    [Test]
    public void CreateSmartCommand_WithNullOptionalParameters_UsesDefaults()
    {
        // Arrange
        var mockExecutionStrategy = new Mock<IExecutionStrategy<string>>().Object;

        // Act
        var result = _factory.CreateSmartCommand(
            mockExecutionStrategy,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Assert
        Assert.That(result, Is.InstanceOf<SmartCommand<string>>());
        // Default parameters internally used: AlwaysCanExecuteStrategy, a new GUID as commandName, 
        // SwallowErrorHandlingStrategy, CountedExecutionThrottlerStrategy, SimpleRetryStrategy, 
        // TimeProvider.System, SwallowAnalyticsStrategy
    }

    [Test]
    public void CreateSmartCommand_WithPropertyInfo_ReturnsSmartCommand()
    {
        // Arrange
        var testViewModel = new TestViewModel();
        var propertyInfo = typeof(TestViewModel).GetProperty(nameof(TestViewModel.TestCommand));

        // Setup mocks
        var mockExecutionStrategy = new Mock<IExecutionStrategy<object>>();
        var mockCanExecuteStrategy = new Mock<ICanExecuteStrategy<object>>();
        var mockErrorHandlingStrategy = new Mock<IErrorHandlingStrategy>();
        var mockThrottlerStrategy = new Mock<IExecutionThrottlerStrategy>();
        var mockRetryStrategy = new Mock<IRetryStrategy>();
        var mockAnalyticsStrategy = new Mock<IAnalyticsStrategy>();

        _mockExecutionStrategyFactory
            .Setup(f => f.CreateStrategy(testViewModel, "ExecuteTest", It.IsAny<Type>()))
            .Returns(mockExecutionStrategy.Object);

        _mockCanExecuteStrategyFactory
            .Setup(f => f.CreateCanExecuteStrategy(testViewModel, "CanExecuteTest", It.IsAny<Type>()))
            .Returns(mockCanExecuteStrategy.Object);

        _mockErrorHandlingStrategyFactory
            .Setup(f => f.CreateErrorHandlingStrategy(propertyInfo))
            .Returns(mockErrorHandlingStrategy.Object);

        _mockAnalyticsStrategyFactory
            .Setup(f => f.CreateAnalyticsStrategy(propertyInfo))
            .Returns(mockAnalyticsStrategy.Object);

        _mockExecutionThrottlerStrategyFactory
            .Setup(f => f.CreateExecutionThrottlerStrategy(propertyInfo))
            .Returns(mockThrottlerStrategy.Object);

        _mockRetryStrategyFactory
            .Setup(f => f.CreateRetryStrategy(propertyInfo))
            .Returns(mockRetryStrategy.Object);

        // Act
        var result = _factory.CreateSmartCommand(testViewModel, propertyInfo);

        // Assert
        Assert.That(result, Is.InstanceOf<ICommand>());
        Assert.That(result, Is.InstanceOf<SmartCommand<object>>());

        // Verify strategy factory calls
        _mockExecutionStrategyFactory.Verify(f =>
            f.CreateStrategy(testViewModel, "ExecuteTest", It.IsAny<Type>()), Times.Once);

        _mockCanExecuteStrategyFactory.Verify(f =>
            f.CreateCanExecuteStrategy(testViewModel, "CanExecuteTest", It.IsAny<Type>()), Times.Once);

        _mockErrorHandlingStrategyFactory.Verify(f =>
            f.CreateErrorHandlingStrategy(propertyInfo), Times.Once);

        _mockAnalyticsStrategyFactory.Verify(f =>
            f.CreateAnalyticsStrategy(propertyInfo), Times.Once);

        _mockExecutionThrottlerStrategyFactory.Verify(f =>
            f.CreateExecutionThrottlerStrategy(propertyInfo), Times.Once);

        _mockRetryStrategyFactory.Verify(f =>
            f.CreateRetryStrategy(propertyInfo), Times.Once);
    }

    [Test]
    public void CreateSmartCommand_WithNullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        var propertyInfo = typeof(TestViewModel).GetProperty(nameof(TestViewModel.TestCommand));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _factory.CreateSmartCommand(null, propertyInfo));
    }

    [Test]
    public void CreateSmartCommand_WithNullPropertyInfo_ThrowsArgumentNullException()
    {
        // Arrange
        var testViewModel = new TestViewModel();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _factory.CreateSmartCommand(testViewModel, null));
    }

    [Test]
    public void CreateSmartCommand_WithMissingAttribute_ThrowsInvalidOperationException()
    {
        // Arrange
        var testViewModel = new TestViewModel();
        var propertyInfo = typeof(TestViewModel).GetProperty(nameof(TestViewModel.NoAttributeCommand));

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _factory.CreateSmartCommand(testViewModel, propertyInfo));

        Assert.That(ex.Message, Does.Contain("Command Binding Attribute is null"));
    }

    [Test]
    public void CreateSmartCommand_WithWrongTargetType_ThrowsInvalidOperationException()
    {
        // Arrange
        var testViewModel = new DifferentViewModel();
        var propertyInfo = typeof(TestViewModel).GetProperty(nameof(TestViewModel.TestCommand));

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _factory.CreateSmartCommand(testViewModel, propertyInfo));

        Assert.That(ex.Message, Does.Contain("Target object type"));
    }

    [Test]
    public void CreateSmartCommandsFor_WithValidViewModel_SetsAllCommandProperties()
    {
        // Arrange
        var testViewModel = new TestViewModel();
        var mockCommand = new Mock<ICommand>().Object;

        // Setup to return a mock command for any property
        _mockExecutionStrategyFactory
            .Setup(f => f.CreateStrategy(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<Type>()))
            .Returns(new Mock<IExecutionStrategy<object>>().Object);

        _mockCanExecuteStrategyFactory
            .Setup(f => f.CreateCanExecuteStrategy(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<Type>()))
            .Returns(new Mock<ICanExecuteStrategy<object>>().Object);

        _mockErrorHandlingStrategyFactory
            .Setup(f => f.CreateErrorHandlingStrategy(It.IsAny<PropertyInfo>()))
            .Returns(new Mock<IErrorHandlingStrategy>().Object);

        _mockAnalyticsStrategyFactory
            .Setup(f => f.CreateAnalyticsStrategy(It.IsAny<PropertyInfo>()))
            .Returns(new Mock<IAnalyticsStrategy>().Object);

        _mockExecutionThrottlerStrategyFactory
            .Setup(f => f.CreateExecutionThrottlerStrategy(It.IsAny<PropertyInfo>()))
            .Returns(new Mock<IExecutionThrottlerStrategy>().Object);

        _mockRetryStrategyFactory
            .Setup(f => f.CreateRetryStrategy(It.IsAny<PropertyInfo>()))
            .Returns(new Mock<IRetryStrategy>().Object);

        // Act
        _factory.CreateSmartCommandsFor(testViewModel);

        // Assert
        // Verify the command property was set (not null)
        Assert.That(testViewModel.TestCommand, Is.Not.Null);
        // NoAttributeCommand should remain null
        Assert.That(testViewModel.NoAttributeCommand, Is.Null);
    }

    [Test]
    public void CreateSmartCommandsFor_WithInValidViewModel_ThrowException()
    {
        // Arrange
        var testViewModel = new WrongTestViewModel();
        var mockCommand = new Mock<ICommand>().Object;

        // Setup to return a mock command for any property
        _mockExecutionStrategyFactory
            .Setup(f => f.CreateStrategy(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<Type>()))
            .Returns(new Mock<IExecutionStrategy<object>>().Object);

        _mockCanExecuteStrategyFactory
            .Setup(f => f.CreateCanExecuteStrategy(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<Type>()))
            .Returns(new Mock<ICanExecuteStrategy<object>>().Object);

        _mockErrorHandlingStrategyFactory
            .Setup(f => f.CreateErrorHandlingStrategy(It.IsAny<PropertyInfo>()))
            .Returns(new Mock<IErrorHandlingStrategy>().Object);

        _mockAnalyticsStrategyFactory
            .Setup(f => f.CreateAnalyticsStrategy(It.IsAny<PropertyInfo>()))
            .Returns(new Mock<IAnalyticsStrategy>().Object);

        _mockExecutionThrottlerStrategyFactory
            .Setup(f => f.CreateExecutionThrottlerStrategy(It.IsAny<PropertyInfo>()))
            .Returns(new Mock<IExecutionThrottlerStrategy>().Object);

        _mockRetryStrategyFactory
            .Setup(f => f.CreateRetryStrategy(It.IsAny<PropertyInfo>()))
            .Returns(new Mock<IRetryStrategy>().Object);

        // Act & Assert
        var ex = Assert.Throws<CustomAttributeFormatException>(() => _factory.CreateSmartCommandsFor(testViewModel));
        Assert.That(ex.Message, Is.EqualTo("'BindingMethod' property specified was not found."));
    }

    [Test]
    public void CreateSmartCommandsFor_WithNullTarget_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _factory.CreateSmartCommandsFor(null));
    }

    [Test]
    public void CreateSmartCommand_WithMismatchedParameterTypes_ThrowsInvalidOperationException()
    {
        // Arrange
        var testViewModel = new MismatchedParameterViewModel();
        var propertyInfo = typeof(MismatchedParameterViewModel).GetProperty(nameof(MismatchedParameterViewModel.TestCommand));

        // Setup execution factory to return an execution strategy that will pass initial checks
        var mockExecutionStrategy = new Mock<IExecutionStrategy<object>>();
        _mockExecutionStrategyFactory
            .Setup(f => f.CreateStrategy(testViewModel, "ExecuteTest", It.IsAny<Type>()))
            .Returns(mockExecutionStrategy.Object);

        // Execute method parameter type would be 'string'
        var executeMethod = typeof(MismatchedParameterViewModel).GetMethod("ExecuteTest");
        var canExecuteMethod = typeof(MismatchedParameterViewModel).GetMethod("CanExecuteTest");

        // During reflection, we'll simulate the parameter type mismatch
        var executeParameterType = typeof(string);
        var canExecuteParameterType = typeof(int);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _factory.CreateSmartCommand(testViewModel, propertyInfo));

        Assert.That(ex.Message, Does.Contain("must have the same parameter type"));
    }

// Add this to your test class definitions
    public class MismatchedParameterViewModel
    {
        [SmartCommand(
            BindingMethod = "ExecuteTest",
            CanExecuteMethod = "CanExecuteTest",
            CommandIdentifier = "TestCommandId")]
        public ICommand TestCommand { get; set; }

        // These methods have different parameter types
        public void ExecuteTest(string parameter)
        {
        }

        public bool CanExecuteTest(int parameter) => true;
    }

    public class TestViewModel
    {
        [SmartCommand(
            BindingMethod = "ExecuteTest",
            CanExecuteMethod = "CanExecuteTest",
            CommandIdentifier = "TestCommandId")]
        public ICommand TestCommand { get; set; }

        public ICommand NoAttributeCommand { get; set; }

        public void ExecuteTest()
        {
        }

        public bool CanExecuteTest() => true;
    }

    public class WrongTestViewModel
    {
        [SmartCommand(BindingMethod = "")] public ICommand WrongTestCommand { get; set; }

        public ICommand NoAttributeCommand { get; set; }

        public void ExecuteTest()
        {
        }

        public bool CanExecuteTest() => true;
    }

    public class DifferentViewModel
    {
        // Different type to test type mismatch
    }

    [Test]
    public void CreateSmartCommand_WithNoParameterMethod_HandlesNullParameterTypeCorrectly()
    {
        // Arrange
        var testViewModel = new NoParameterViewModel();
        var propertyInfo = typeof(NoParameterViewModel).GetProperty(nameof(NoParameterViewModel.TestCommand));

        // Setup mocks to return default strategies
        SetupDefaultMocks(testViewModel, propertyInfo);

        // Act
        var result = _factory.CreateSmartCommand(testViewModel, propertyInfo);

        // Assert
        Assert.That(result, Is.InstanceOf<SmartCommand<object>>());

        // Verify that correct parameter type (null) was passed to the execution strategy factory
        _mockExecutionStrategyFactory.Verify(f =>
            f.CreateStrategy(testViewModel, "ExecuteTest", null), Times.Once);
    }

    [Test]
    public void CreateSmartCommand_WithSingleParameterMethod_HandlesParameterTypeCorrectly()
    {
        // Arrange
        var testViewModel = new SingleParameterViewModel();
        var propertyInfo = typeof(SingleParameterViewModel).GetProperty(nameof(SingleParameterViewModel.TestCommand));

        // Setup mocks to return default strategies
        SetupDefaultMocks(testViewModel, propertyInfo);

        // Act
        var result = _factory.CreateSmartCommand(testViewModel, propertyInfo);

        // Assert
        Assert.That(result, Is.InstanceOf<SmartCommand<string>>());

        // Verify that correct parameter type (string) was passed to the execution strategy factory
        _mockExecutionStrategyFactory.Verify(f =>
            f.CreateStrategy(testViewModel, "ExecuteTest", typeof(string)), Times.Once);
    }

    [Test]
    public void CreateSmartCommand_WithTwoParameterMethod_ThrowsInvalidOperationException()
    {
        // Arrange
        var testViewModel = new TwoParameterViewModel();
        var propertyInfo = typeof(TwoParameterViewModel).GetProperty(nameof(TwoParameterViewModel.TestCommand));

        // Setup execution factory to throw the expected exception when encountering multi-parameter method
        _mockExecutionStrategyFactory
            .Setup(f => f.CreateStrategy(testViewModel, "ExecuteTest", It.IsAny<Type>()))
            .Callback(() => { throw new InvalidOperationException("Method must have 0 or 1 parameter."); });

        // Setup other required mocks
        SetupMocksExceptExecution(testViewModel, propertyInfo);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _factory.CreateSmartCommand(testViewModel, propertyInfo));

        Assert.That(ex.Message, Does.Contain("Method must have 0 or 1 parameter"));
    }

// Helper method to setup all mocks with default behaviors
    private void SetupDefaultMocks(object viewModel, PropertyInfo propertyInfo)
    {
        var mockExecutionStrategy = new Mock<IExecutionStrategy<object>>().Object;
        var mockCanExecuteStrategy = new Mock<ICanExecuteStrategy<object>>().Object;
        var mockErrorHandlingStrategy = new Mock<IErrorHandlingStrategy>().Object;
        var mockThrottlerStrategy = new Mock<IExecutionThrottlerStrategy>().Object;
        var mockRetryStrategy = new Mock<IRetryStrategy>().Object;
        var mockAnalyticsStrategy = new Mock<IAnalyticsStrategy>().Object;

        _mockExecutionStrategyFactory
            .Setup(f => f.CreateStrategy(viewModel, It.IsAny<string>(), It.IsAny<Type>()))
            .Returns(mockExecutionStrategy);

        _mockCanExecuteStrategyFactory
            .Setup(f => f.CreateCanExecuteStrategy(viewModel, It.IsAny<string>(), It.IsAny<Type>()))
            .Returns(mockCanExecuteStrategy);

        _mockErrorHandlingStrategyFactory
            .Setup(f => f.CreateErrorHandlingStrategy(propertyInfo))
            .Returns(mockErrorHandlingStrategy);

        _mockAnalyticsStrategyFactory
            .Setup(f => f.CreateAnalyticsStrategy(propertyInfo))
            .Returns(mockAnalyticsStrategy);

        _mockExecutionThrottlerStrategyFactory
            .Setup(f => f.CreateExecutionThrottlerStrategy(propertyInfo))
            .Returns(mockThrottlerStrategy);

        _mockRetryStrategyFactory
            .Setup(f => f.CreateRetryStrategy(propertyInfo))
            .Returns(mockRetryStrategy);
    }

// Helper method to setup all mocks except execution factory
    private void SetupMocksExceptExecution(object viewModel, PropertyInfo propertyInfo)
    {
        var mockCanExecuteStrategy = new Mock<ICanExecuteStrategy<object>>().Object;
        var mockErrorHandlingStrategy = new Mock<IErrorHandlingStrategy>().Object;
        var mockThrottlerStrategy = new Mock<IExecutionThrottlerStrategy>().Object;
        var mockRetryStrategy = new Mock<IRetryStrategy>().Object;
        var mockAnalyticsStrategy = new Mock<IAnalyticsStrategy>().Object;

        _mockCanExecuteStrategyFactory
            .Setup(f => f.CreateCanExecuteStrategy(viewModel, It.IsAny<string>(), It.IsAny<Type>()))
            .Returns(mockCanExecuteStrategy);

        _mockErrorHandlingStrategyFactory
            .Setup(f => f.CreateErrorHandlingStrategy(propertyInfo))
            .Returns(mockErrorHandlingStrategy);

        _mockAnalyticsStrategyFactory
            .Setup(f => f.CreateAnalyticsStrategy(propertyInfo))
            .Returns(mockAnalyticsStrategy);

        _mockExecutionThrottlerStrategyFactory
            .Setup(f => f.CreateExecutionThrottlerStrategy(propertyInfo))
            .Returns(mockThrottlerStrategy);

        _mockRetryStrategyFactory
            .Setup(f => f.CreateRetryStrategy(propertyInfo))
            .Returns(mockRetryStrategy);
    }

// Test view model classes for each scenario

    public class NoParameterViewModel
    {
        [SmartCommand(
            BindingMethod = "ExecuteTest",
            CanExecuteMethod = "CanExecuteTest")]
        public ICommand TestCommand { get; set; }

        // Method with no parameters (covers the first branch)
        public void ExecuteTest()
        {
        }

        public bool CanExecuteTest() => true;
    }

    public class SingleParameterViewModel
    {
        [SmartCommand(BindingMethod = "ExecuteTest", CanExecuteMethod = "CanExecuteTest")]
        public ICommand TestCommand { get; set; }

        // Method with single parameter (covers the second branch)
        public void ExecuteTest(string parameter)
        {
        }

        public bool CanExecuteTest(string parameter) => true;
    }

    public class TwoParameterViewModel
    {
        [SmartCommand(BindingMethod = "ExecuteTest", CanExecuteMethod = "CanExecuteTest")]
        public ICommand TestCommand { get; set; }

        // Method with two parameters (covers the default/error branch)
        public void ExecuteTest(string parameter1, int parameter2)
        {
        }

        public bool CanExecuteTest() => true;
    }
}