using Microsoft.Extensions.Logging;
using Moq;
using SmartCommand.Base;
using SmartCommand.ErrorHandlingStrategy;

namespace SmartCommand.Tests.ErrorHandlingStrategy;

[TestFixture]
public class DefaultErrorHandlingStrategyFactoryTests
{
    private DefaultErrorHandlingStrategyFactory _factory;
    private Mock<ILogger> _loggerMock;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger>();
        _factory = new DefaultErrorHandlingStrategyFactory(_loggerMock.Object);
    }

    public class UnrecognizedErrorHandlingStrategyAttribute : SmartCommandBaseAttribute<IErrorHandlingStrategy> { }

    public class TestClass
    {
        [LogErrorHandlingStrategy]
        public string LogErrorHandlingProperty { get; set; }

        [PanicErrorHandlingStrategy]
        public string PanicErrorHandlingProperty { get; set; }

        [LogErrorHandlingStrategy]
        [PanicErrorHandlingStrategy]
        public string MultipleAttributesProperty { get; set; }

        [UnrecognizedErrorHandlingStrategy]
        public string UnsupportedAttributeProperty { get; set; }

        public string NoAttributeProperty { get; set; }
    }

    [Test]
    public void CreateErrorHandlingStrategy_WithLogErrorHandlingAttribute_ShouldReturnLogStrategy()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.LogErrorHandlingProperty));

        // Act
        var result = _factory.CreateErrorHandlingStrategy(propertyInfo);

        // Assert
        Assert.That(result, Is.InstanceOf<LogErrorHandlingStrategy>());
    }

    [Test]
    public void CreateErrorHandlingStrategy_WithPanicErrorHandlingAttribute_ShouldReturnPanicStrategy()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.PanicErrorHandlingProperty));

        // Act
        var result = _factory.CreateErrorHandlingStrategy(propertyInfo);

        // Assert
        Assert.That(result, Is.InstanceOf<PanicErrorHandlingStrategy>());
    }

    [Test]
    public void CreateErrorHandlingStrategy_NoAttribute_ShouldReturnSwallowErrorHandlingStrategy()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.NoAttributeProperty));

        // Act
        var result = _factory.CreateErrorHandlingStrategy(propertyInfo);

        // Assert
        Assert.That(result, Is.InstanceOf<SwallowErrorHandlingStrategy>());
    }

    [Test]
    public void CreateErrorHandlingStrategy_UnsupportedAttribute_ShouldThrowException()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.UnsupportedAttributeProperty));

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => _factory.CreateErrorHandlingStrategy(propertyInfo));
        Assert.That(ex.Message, Does.Contain("Unknown attribute for error handling strategy is not supported."));
    }

    [Test]
    public void CreateErrorHandlingStrategy_WithMultipleAttributes_ShouldReturnCompositeErrorHandlingStrategy()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.MultipleAttributesProperty));

        // Act
        var result = _factory.CreateErrorHandlingStrategy(propertyInfo);

        // Assert
        Assert.That(result, Is.InstanceOf<CompositeErrorHandlingStrategy>());
    }
   
}