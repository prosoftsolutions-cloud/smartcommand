using System.Reflection;
using System.Windows.Input;
using SmartCommand.AnalyticsStrategy;
using SmartCommand.CanExecuteStrategy;
using SmartCommand.ErrorHandlingStrategy;
using SmartCommand.ExecutionStrategy;
using SmartCommand.ExecutionThrottlerStrategy;
using SmartCommand.RetryStrategy;

namespace SmartCommand.SmartCommand;

public interface ISmartCommandFactory
{
    ICommand CreateSmartCommand<TParameter>(
            IExecutionStrategy<TParameter> executionStrategy,
            ICanExecuteStrategy<TParameter>? canExecuteStrategy = null,
            string commandName = "",
            IErrorHandlingStrategy? errorHandlerStrategy = null,
            IExecutionThrottlerStrategy? throttlerStrategy = null,
            IRetryStrategy? retryStrategy = null,
            TimeProvider? timeProvider = null,
            IAnalyticsStrategy? analyticsStrategy = null);    
    ICommand CreateSmartCommand(
        IExecutionStrategy<object> executionStrategy,
        ICanExecuteStrategy<object>? canExecuteStrategy = null,
        string commandName = "",
        IErrorHandlingStrategy? errorHandlerStrategy = null,
        IExecutionThrottlerStrategy? throttlerStrategy = null,
        IRetryStrategy? retryStrategy = null,
        TimeProvider? timeProvider = null,
        IAnalyticsStrategy? analyticsStrategy = null);
    
    ICommand CreateSmartCommand(object target, PropertyInfo propertyInfo);
    void CreateSmartCommandsFor(object target);
    

//    ICommand CreateCommand(Type parameterType, string commandName, Action<object?> execute, Func<object?, bool>? canExecute = null);
}