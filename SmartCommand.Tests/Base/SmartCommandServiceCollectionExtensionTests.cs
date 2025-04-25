using Microsoft.Extensions.DependencyInjection;
using SmartCommand.AnalyticsStrategy;
using SmartCommand.Base;
using SmartCommand.CanExecuteStrategy;
using SmartCommand.ErrorHandlingStrategy;
using SmartCommand.ExecutionStrategy;
using SmartCommand.ExecutionThrottlerStrategy;
using SmartCommand.RetryStrategy;
using SmartCommand.SmartCommand;

namespace SmartCommand.Tests.Base;

[TestFixture]
public class SmartCommandServiceCollectionExtensionTests
{
    private IServiceCollection _services;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();
    }

    [Test]
    public void RegisterSmartCommand_RegistersAllRequiredServices()
    {
        // Act
        _services.RegisterSmartCommand();

        // Assert
        Assert.Multiple(() =>
        {
            AssertServiceIsRegistered<IAnalyticsStrategyFactory, DefaultAnalyticsStrategyFactory>(ServiceLifetime.Singleton);
            AssertServiceIsRegistered<ICanExecuteStrategyFactory, DefaultCanExecuteStrategyFactory>(ServiceLifetime.Singleton);
            AssertServiceIsRegistered<IErrorHandlingStrategyFactory, DefaultErrorHandlingStrategyFactory>(ServiceLifetime.Singleton);
            AssertServiceIsRegistered<IExecutionStrategyFactory, DefaultExecutionStrategyFactory>(ServiceLifetime.Singleton);
            AssertServiceIsRegistered<IExecutionThrottlerStrategyFactory, DefaultExecutionThrottlerStrategyFactory>(ServiceLifetime.Singleton);
            AssertServiceIsRegistered<IRetryStrategyFactory, DefaultRetryStrategyFactory>(ServiceLifetime.Singleton);
            AssertServiceIsRegistered<ISmartCommandFactory, DefaultSmartCommandFactory>(ServiceLifetime.Singleton);
        });
    }

    [Test]
    public void RegisterSmartCommand_ReturnsServiceCollection()
    {
        // Act
        var result = _services.RegisterSmartCommand();

        // Assert
        Assert.That(result, Is.SameAs(_services));
    }

    [Test]
    public void RegisterSmartCommand_RegistersExactlySevenServices()
    {
        // Act
        _services.RegisterSmartCommand();

        // Assert
        Assert.That(_services, Has.Count.EqualTo(7));
    }

    private void AssertServiceIsRegistered<TService, TImplementation>(ServiceLifetime expectedLifetime)
    {
        var serviceDescriptor = _services.FirstOrDefault(sd => sd.ServiceType == typeof(TService));
        
        Assert.That(serviceDescriptor, Is.Not.Null, $"Service {typeof(TService).Name} is not registered");
        Assert.That(serviceDescriptor.ImplementationType, Is.EqualTo(typeof(TImplementation)), 
            $"Service {typeof(TService).Name} is registered with wrong implementation type");
        Assert.That(serviceDescriptor.Lifetime, Is.EqualTo(expectedLifetime), 
            $"Service {typeof(TService).Name} is registered with wrong lifetime");
    }
    
}