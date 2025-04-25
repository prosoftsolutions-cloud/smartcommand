using Microsoft.Extensions.DependencyInjection;
using SmartCommand.AnalyticsStrategy;
using SmartCommand.CanExecuteStrategy;
using SmartCommand.ErrorHandlingStrategy;
using SmartCommand.ExecutionStrategy;
using SmartCommand.ExecutionThrottlerStrategy;
using SmartCommand.RetryStrategy;
using SmartCommand.SmartCommand;

namespace SmartCommand.Base;

public static class SmartCommandServiceCollectionExtension
{
    public static IServiceCollection RegisterSmartCommand(this IServiceCollection collection)
    {
        collection.AddSingleton<IAnalyticsStrategyFactory, DefaultAnalyticsStrategyFactory>();
        collection.AddSingleton<ICanExecuteStrategyFactory, DefaultCanExecuteStrategyFactory>();
        collection.AddSingleton<IErrorHandlingStrategyFactory, DefaultErrorHandlingStrategyFactory>();
        collection.AddSingleton<IExecutionStrategyFactory, DefaultExecutionStrategyFactory>();
        collection.AddSingleton<IExecutionThrottlerStrategyFactory, DefaultExecutionThrottlerStrategyFactory>();
        collection.AddSingleton<IRetryStrategyFactory, DefaultRetryStrategyFactory>();
        collection.AddSingleton<ISmartCommandFactory, DefaultSmartCommandFactory>();
        return collection;
    }
}