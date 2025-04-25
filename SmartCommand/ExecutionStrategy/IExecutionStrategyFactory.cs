namespace SmartCommand.ExecutionStrategy;

public interface IExecutionStrategyFactory
{
    IExecutionStrategy<TParameter> CreateStrategy<TParameter>(Action syncAction);
    IExecutionStrategy<object> CreateStrategy(Action syncAction);
    IExecutionStrategy<TParameter> CreateStrategy<TParameter>(Action<TParameter?> syncAction);
    IExecutionStrategy<TParameter> CreateStrategy<TParameter>(Func<Task> asyncAction);
    IExecutionStrategy<object> CreateStrategy(Func<Task> asyncAction);
    IExecutionStrategy<TParameter> CreateStrategy<TParameter>(Func<TParameter?, Task> asyncAction);
    
    object CreateStrategy(object target, string methodName, Type? parameterType);
}