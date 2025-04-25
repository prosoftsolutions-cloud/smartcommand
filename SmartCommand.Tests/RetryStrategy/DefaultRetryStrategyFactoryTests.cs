using SmartCommand.Base;
using SmartCommand.RetryStrategy;

namespace SmartCommand.Tests.RetryStrategy;

public class DefaultRetryStrategyFactoryTests
{
        private DefaultRetryStrategyFactory _factory;

        [SetUp]
        public void Setup()
        {
            // Initialize the factory before each test
            _factory = new DefaultRetryStrategyFactory();
        }

        public class OneMoreRetryAttribute : SmartCommandBaseAttribute<IRetryStrategy>
        {
        }

        public class SomeTestClass
        {
            [SimpleRetryStrategy(RetryCount = 5)]
            public string CorrectCommandProperty { get; set; }
            
            [SimpleRetryStrategy(RetryCount = 7)]
            [OneMoreRetry]
            public string DoubleAttributeCommandProperty { get; set; }

            public string NoAttributeCommandProperty { get; set; }
            
            [OneMoreRetry]
            public string UnsupportedAttributeCommandProperty { get; set; }
        }


        [Test]
        public void CreateRetryStrategy_CorrectAttribute_ShouldReturnSimpleRetryStrategyWithGivenParams()
        {
            // Arrange
            var propertyInfo = typeof(SomeTestClass).GetProperty("CorrectCommandProperty");
            
            //Act
            var result = _factory.CreateRetryStrategy(propertyInfo);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SimpleRetryStrategy>());
            var castedResult = (SimpleRetryStrategy)result;
            Assert.That(castedResult.MaxRetryCount, Is.EqualTo(5));
        }
        
        [Test]
        public void CreateRetryStrategy_NoAttribute_ShouldReturnSimpleRetryStrategy()
        {
            // Arrange
            var propertyInfo = typeof(SomeTestClass).GetProperty("NoAttributeCommandProperty");
            
            //Act
            var result = _factory.CreateRetryStrategy(propertyInfo);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SimpleRetryStrategy>());
            var castedResult = (SimpleRetryStrategy)result;
            Assert.That(castedResult.MaxRetryCount, Is.EqualTo(1));

        }
        
        [Test]
        public void CreateRetryStrategy_WrongAttribute_ShouldThrowException()
        {
            // Arrange
            var propertyInfo = typeof(SomeTestClass).GetProperty("UnsupportedAttributeCommandProperty");
            
            var ex = Assert.Throws<NotSupportedException>(() => _factory.CreateRetryStrategy(propertyInfo));
            Assert.That(ex!.Message, Does.Contain("Unknown attribute for retry strategy is not supported."));

        }

        [Test]
        public void CreateRetryStrategy_MultiplyAttribute_ShouldThrowException()
        {
            // Arrange
            var propertyInfo = typeof(SomeTestClass).GetProperty("DoubleAttributeCommandProperty");
            
            var ex = Assert.Throws<NotSupportedException>(() => _factory.CreateRetryStrategy(propertyInfo));
            Assert.That(ex!.Message, Does.Contain("Composite retry strategy is not supported."));
        }

}