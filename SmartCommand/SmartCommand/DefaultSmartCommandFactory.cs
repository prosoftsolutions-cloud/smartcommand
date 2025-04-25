using System.Reflection;
using System.Windows.Input;
using SmartCommand.AnalyticsStrategy;
using SmartCommand.CanExecuteStrategy;
using SmartCommand.ErrorHandlingStrategy;
using SmartCommand.ExecutionStrategy;
using SmartCommand.ExecutionThrottlerStrategy;
using SmartCommand.RetryStrategy;

namespace SmartCommand.SmartCommand;

public class DefaultSmartCommandFactory : ISmartCommandFactory
{
    private readonly IAnalyticsStrategyFactory _analyticsStrategyFactory;
    private readonly ICanExecuteStrategyFactory _canExecuteStrategyFactory;
    private readonly IErrorHandlingStrategyFactory _errorHandlingStrategyFactory;
    private readonly IExecutionStrategyFactory _executionStrategyFactory;
    private readonly IExecutionThrottlerStrategyFactory _executionThrottlerStrategyFactory;
    private readonly IRetryStrategyFactory _retryStrategyFactory;
    private readonly TimeProvider _timeProvider;
    public DefaultSmartCommandFactory(
        IExecutionStrategyFactory executionStrategyFactory,
        ICanExecuteStrategyFactory canExecuteStrategyFactory, 
        IErrorHandlingStrategyFactory errorHandlingStrategyFactory, 
        IExecutionThrottlerStrategyFactory executionThrottlerStrategyFactory,
        IRetryStrategyFactory retryStrategyFactory, 
        IAnalyticsStrategyFactory analyticsStrategyFactory,
        TimeProvider timeProvider)
    {
        _retryStrategyFactory = retryStrategyFactory;
        _analyticsStrategyFactory = analyticsStrategyFactory;
        _canExecuteStrategyFactory = canExecuteStrategyFactory;
        _errorHandlingStrategyFactory = errorHandlingStrategyFactory;
        _executionStrategyFactory = executionStrategyFactory;
        _executionThrottlerStrategyFactory = executionThrottlerStrategyFactory;
        _timeProvider = timeProvider;
    }

    public ICommand CreateSmartCommand<TParameter>(
        IExecutionStrategy<TParameter> executionStrategy, 
        ICanExecuteStrategy<TParameter>? canExecuteStrategy = null,
        string commandName = "", 
        IErrorHandlingStrategy? errorHandlerStrategy = null, 
        IExecutionThrottlerStrategy? throttlerStrategy = null, 
        IRetryStrategy? retryStrategy = null,
        TimeProvider? timeProvider = null, 
        IAnalyticsStrategy? analyticsStrategy = null)
    {
        return new SmartCommand<TParameter>(
            executionStrategy,
            canExecuteStrategy ?? new AlwaysCanExecuteStrategy<TParameter>(),
            String.IsNullOrEmpty(commandName) ? Guid.NewGuid().ToString() : commandName,
            errorHandlerStrategy ?? new SwallowErrorHandlingStrategy(),
            throttlerStrategy?? new CountedExecutionThrottlerStrategy(),
            retryStrategy ?? new SimpleRetryStrategy(),
            timeProvider?? TimeProvider.System,
            analyticsStrategy?? new SwallowAnalyticsStrategy());
    }

    public ICommand CreateSmartCommand(
        IExecutionStrategy<object> executionStrategy, 
        ICanExecuteStrategy<object>? canExecuteStrategy = null,
        string commandName = "", 
        IErrorHandlingStrategy? errorHandlerStrategy = null, 
        IExecutionThrottlerStrategy? throttlerStrategy = null, 
        IRetryStrategy? retryStrategy = null,
        TimeProvider? timeProvider = null, 
        IAnalyticsStrategy? analyticsStrategy = null)
    {
        return CreateSmartCommand<object>(executionStrategy, canExecuteStrategy, commandName, errorHandlerStrategy, throttlerStrategy, retryStrategy,
            timeProvider, analyticsStrategy);
    }

    public ICommand CreateSmartCommand(object target, PropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(target, nameof(target));
        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));
        
        var attribute = propertyInfo.GetCustomAttribute<SmartCommandAttribute>() ??
                        throw new InvalidOperationException($"Command Binding Attribute is null for property {propertyInfo.Name}");
        var targetType = propertyInfo.DeclaringType ?? 
                         throw new InvalidOperationException("Declaring type is null");
        if (targetType != target.GetType())
        {
            throw new InvalidOperationException($"Target object type {targetType.FullName} does not match target property type {target.GetType().FullName}");       
        }
        
        var executeMethod = targetType.GetMethod(attribute.BindingMethod,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)??
                            throw new InvalidOperationException($"Method '{attribute.BindingMethod}' not found.");
        var executeParameterType = GetParameterType(executeMethod);
        var executionStrategy = _executionStrategyFactory.CreateStrategy(target, attribute.BindingMethod, executeParameterType);
        
        var canExecuteMethod = targetType.GetMethod(attribute.CanExecuteMethod, 
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var canExecuteParameterType = typeof(object);
        if (canExecuteMethod != null)
        {
            canExecuteParameterType = GetParameterType(canExecuteMethod);
            if (canExecuteParameterType != executeParameterType)
            {
                throw new InvalidOperationException($"Method '{attribute.BindingMethod}' must have the same parameter type as '{attribute.BindingMethod}'");
            }
        }
        var canExecuteStrategy = _canExecuteStrategyFactory.CreateCanExecuteStrategy(target, attribute.CanExecuteMethod, canExecuteParameterType??typeof(object));
        var errorHandlingStrategy = _errorHandlingStrategyFactory.CreateErrorHandlingStrategy(propertyInfo);
        var analyticsStrategy = _analyticsStrategyFactory.CreateAnalyticsStrategy(propertyInfo);
        var throttlerStrategy = _executionThrottlerStrategyFactory.CreateExecutionThrottlerStrategy(propertyInfo);
        var retryStrategy = _retryStrategyFactory.CreateRetryStrategy(propertyInfo);
        var openType = typeof(SmartCommand<>);
        var closedType = openType.MakeGenericType(executeParameterType ?? typeof(object));
        return Activator.CreateInstance(closedType, 
                   executionStrategy, 
                   canExecuteStrategy, 
                   attribute.CommandIdentifier, 
                   errorHandlingStrategy, 
                   throttlerStrategy, 
                   retryStrategy, 
                   _timeProvider, 
                   analyticsStrategy) as ICommand ?? throw new InvalidOperationException("Activator.CreateInstance returned null");
    }

    public void CreateSmartCommandsFor(object target)
    {
        ArgumentNullException.ThrowIfNull(target, nameof(target));
        var targetType = target.GetType();
        var properties = targetType.GetProperties();
        foreach (var propertyInfo in properties)
        {
            var attribute = propertyInfo.GetCustomAttribute<SmartCommandAttribute>();
            if (attribute == null)
            {
                continue;
            }
            var command = CreateSmartCommand(target, propertyInfo);
            propertyInfo.SetValue(target, command);
        } 
    }

    private Type? GetParameterType(MethodInfo methodInfo)
    {
        var methodParams = methodInfo.GetParameters();
        switch (methodParams.Length)
        {
            case 0: return null;
            case 1: return methodParams[0].ParameterType;
            default: throw new InvalidOperationException("Method must have 0 or 1 parameter.");
        }
    }


}