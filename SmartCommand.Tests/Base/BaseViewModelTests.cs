using Moq;
using SmartCommand.Base;
using SmartCommand.SmartCommand;

namespace SmartCommand.Tests.Base;

[TestFixture]
public class BaseViewModelTests
{
    private Mock<IMainThreadDispatcher> _mainThreadDispatcherMock;
    private Mock<ISmartCommandFactory> _smartCommandFactoryMock;
    private TestViewModel _viewModel;

    [SetUp]
    public void Setup()
    {
        _mainThreadDispatcherMock = new Mock<IMainThreadDispatcher>();
        _smartCommandFactoryMock = new Mock<ISmartCommandFactory>();
        
        // Setup main thread dispatcher to execute action immediately
        _mainThreadDispatcherMock
            .Setup(x => x.InvokeOnMainThread(It.IsAny<Action>()))
            .Callback<Action>(action => action());
            
        _viewModel = new TestViewModel(_mainThreadDispatcherMock.Object);
    }

    [Test]
    public void Constructor_WithSmartCommandFactory_CallsCreateSmartCommands()
    {
        // Arrange & Act
        var viewModel = new TestViewModel(_mainThreadDispatcherMock.Object, _smartCommandFactoryMock.Object);

        // Assert
        _smartCommandFactoryMock.Verify(x => x.CreateSmartCommandsFor(viewModel), Times.Once);
    }

    [Test]
    public void Constructor_WithoutSmartCommandFactory_DoesNotThrow()
    {
        // Assert
        Assert.That(() => new TestViewModel(_mainThreadDispatcherMock.Object), Throws.Nothing);
    }

    [Test]
    public void SetValue_WhenValueChanges_RaisesPropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        var propertyName = string.Empty;
        
        _viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            propertyName = args.PropertyName ?? string.Empty;
        };

        // Act
        _viewModel.TestProperty = "new value";

        // Assert
        Assert.That(propertyChangedRaised, Is.True);
        Assert.That(propertyName, Is.EqualTo("TestProperty"));
        Assert.That(_viewModel.TestProperty, Is.EqualTo("new value"));
    }

    [Test]
    public void SetValue_WhenValueDoesNotChange_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        _viewModel.TestProperty = "initial value";
        
        _viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        // Act
        _viewModel.TestProperty = "initial value";

        // Assert
        Assert.That(propertyChangedRaised, Is.False);
        Assert.That(_viewModel.TestProperty, Is.EqualTo("initial value"));
    }

    [Test]
    public void OnPropertyChanged_InvokesOnMainThread()
    {
        // Arrange &A ct
        _viewModel.TestProperty = "test";

        // Assert
        _mainThreadDispatcherMock.Verify(
            x => x.InvokeOnMainThread(It.IsAny<Action>()),
            Times.Once);
    }

    // Helper class for testing
    private class TestViewModel : BaseViewModel
    {
        private string _testProperty = string.Empty;

        public TestViewModel(IMainThreadDispatcher mainThreadDispatcher, ISmartCommandFactory? factory = null) 
            : base(mainThreadDispatcher, factory)
        {
        }

        public string TestProperty
        {
            get => _testProperty;
            set => SetValue(ref _testProperty, value);
        }

        public void ForcePropertyChanged()
        {
            OnPropertyChanged(nameof(TestProperty));
        }
    }
    
}