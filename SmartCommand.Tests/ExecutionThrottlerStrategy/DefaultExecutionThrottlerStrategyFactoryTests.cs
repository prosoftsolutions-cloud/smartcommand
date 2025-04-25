using SmartCommand.Base;
using SmartCommand.ExecutionThrottlerStrategy;

namespace SmartCommand.Tests.ExecutionThrottlerStrategy;

[TestFixture]
public class DefaultExecutionThrottlerStrategyFactoryTests
{
    private DefaultExecutionThrottlerStrategyFactory _factory;

    [SetUp]
    public void SetUp()
    {
        _factory = new DefaultExecutionThrottlerStrategyFactory();
    }
    
    public class UnsupportedThrottlerAttribute : SmartCommandBaseAttribute<IExecutionThrottlerStrategy> { }

    public class TestClass
    {
        [CountedExecutionThrottlerStrategy(MaxParallelExecution = 5)]
        public string CountedExecutionProperty { get; set; }

        [UnsupportedThrottler]
        public string UnsupportedAttributeProperty { get; set; }

        [CountedExecutionThrottlerStrategy(MaxParallelExecution = 5)]
        [UnsupportedThrottler]
        public string DoubleAttributeProperty { get; set; }

        public string NoAttributeProperty { get; set; }
    }

    [Test]
    public void CreateExecutionThrottlerStrategy_WithCountedExecutionAttribute_ShouldReturnValidStrategy()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.CountedExecutionProperty));

        // Act
        var result = _factory.CreateExecutionThrottlerStrategy(propertyInfo);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<CountedExecutionThrottlerStrategy>());

        var strategy = (CountedExecutionThrottlerStrategy)result;
        Assert.That(strategy.MaxParallelExecution, Is.EqualTo(5));
    }

    [Test]
    public void CreateExecutionThrottlerStrategy_NoAttribute_ShouldReturnDefaultStrategy()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.NoAttributeProperty));

        // Act
        var result = _factory.CreateExecutionThrottlerStrategy(propertyInfo);

        // Assert
        Assert.That(result, Is.InstanceOf<CountedExecutionThrottlerStrategy>());

        var strategy = (CountedExecutionThrottlerStrategy)result;
        Assert.That(strategy.MaxParallelExecution, Is.EqualTo(1)); // Default value assumed
    }

    [Test]
    public void CreateExecutionThrottlerStrategy_UnsupportedAttribute_ShouldThrowException()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.UnsupportedAttributeProperty));

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => _factory.CreateExecutionThrottlerStrategy(propertyInfo));
        Assert.That(ex.Message, Does.Contain("Unknown attribute for throttling strategy is not supported."));
    }

    [Test]
    public void CreateExecutionThrottlerStrategy_WithMultipleAttributes_ShouldThrowNotSupportedException()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.DoubleAttributeProperty));
        var ex = Assert.Throws<NotSupportedException>(() => _factory.CreateExecutionThrottlerStrategy(propertyInfo));
        Assert.That(ex.Message, Does.Contain("Composite throttling strategy is not supported."));
    }
}