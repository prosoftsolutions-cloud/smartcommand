using SmartCommand.CanExecuteStrategy;

namespace SmartCommand.Tests.CanExecuteStrategy;

[TestFixture]
public class AlwaysCanExecuteStrategyTests
{
    [Test]
    public void CanExecute_ShouldAlwaysReturnTrue_ForAnyParameter()
    {
        // Arrange
        var strategy = new AlwaysCanExecuteStrategy<object>();
            
        // Act
        var result = strategy.CanExecute(new object());

        // Assert
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void CanExecute_ShouldAlwaysReturnTrue_ForIntParameter()
    {
        // Arrange
        var strategy = new AlwaysCanExecuteStrategy<int>();
            
        // Act
        var result = strategy.CanExecute(42);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void CanExecute_ShouldAlwaysReturnTrue_ForNullParameter()
    {
        // Arrange
        var strategy = new AlwaysCanExecuteStrategy<object>();
            
        // Act
        var result = strategy.CanExecute(null!);

        // Assert
        Assert.That(result, Is.True);
    }
}