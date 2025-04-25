using Moq;
using SmartCommand.AnalyticsStrategy;
using SmartCommand.Base;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SmartCommand.Tests.AnalyticsStrategy;

[TestFixture]
public class DefaultAnalyticsStrategyFactoryTests
{
    private DefaultAnalyticsStrategyFactory _factory;
    private Mock<ILogger> _loggerMock;

    [SetUp]
    public void Setup()
    {
        // Initialize the mock logger and factory before each test
        _loggerMock = new Mock<ILogger>();
        _factory = new DefaultAnalyticsStrategyFactory(_loggerMock.Object);
    }

    public class SomeTestClass
    {
        [LogAnalyticsStrategy(AnalyticsLogLevel = LogLevel.Debug)]
        public string CorrectCommandProperty { get; set; }

        [LogAnalyticsStrategy(AnalyticsLogLevel = LogLevel.Error)]
        [AnotherAnalyticsStrategy]
        public string DoubleAttributeCommandProperty { get; set; }

        public string NoAttributeCommandProperty { get; set; }

        [AnotherAnalyticsStrategy]
        public string UnsupportedAttributeCommandProperty { get; set; }
    }

    public class AnotherAnalyticsStrategy : SmartCommandBaseAttribute<IAnalyticsStrategy>
    {
    }

    [Test]
    public void CreateAnalyticsStrategy_WithCorrectAttribute_ShouldReturnLogAnalyticsStrategy()
    {
        // Arrange
        var propertyInfo = typeof(SomeTestClass).GetProperty(nameof(SomeTestClass.CorrectCommandProperty));

        // Act
        var result = _factory.CreateAnalyticsStrategy(propertyInfo);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<LogAnalyticsStrategy>());
        var castedResult = (LogAnalyticsStrategy)result;
        Assert.That(castedResult.LogLevel, Is.EqualTo(LogLevel.Debug));
    }

    [Test]
    public void CreateAnalyticsStrategy_WithoutAttribute_ShouldReturnSwallowAnalyticsStrategy()
    {
        // Arrange
        var propertyInfo = typeof(SomeTestClass).GetProperty(nameof(SomeTestClass.NoAttributeCommandProperty));

        // Act
        var result = _factory.CreateAnalyticsStrategy(propertyInfo);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<SwallowAnalyticsStrategy>());
    }

    [Test]
    public void CreateAnalyticsStrategy_WithUnsupportedAttribute_ShouldThrowException()
    {
        // Arrange
        var propertyInfo = typeof(SomeTestClass).GetProperty(nameof(SomeTestClass.UnsupportedAttributeCommandProperty));

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => _factory.CreateAnalyticsStrategy(propertyInfo));
        Assert.That(ex!.Message, Does.Contain("Unknown attribute for analytics strategy is not supported."));
    }

    [Test]
    public void CreateAnalyticsStrategy_WithMultipleAttributes_ShouldThrowException()
    {
        // Arrange
        var propertyInfo = typeof(SomeTestClass).GetProperty(nameof(SomeTestClass.DoubleAttributeCommandProperty));

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => _factory.CreateAnalyticsStrategy(propertyInfo));
        Assert.That(ex!.Message, Does.Contain("Composite analytics strategy is not supported."));
    }
    
}