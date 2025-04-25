using System.Reflection;

namespace SmartCommand.ExecutionStrategy;

public class DefaultExecutionStrategyFactory : IExecutionStrategyFactory
{
    public IExecutionStrategy<TParameter> CreateStrategy<TParameter>(Action syncAction)
    {
        return new FuncExecutionStrategy<TParameter>(syncAction);
    }

    public IExecutionStrategy<object> CreateStrategy(Action syncAction)
    {
        return new FuncExecutionStrategy<object>(syncAction);
    }

    public IExecutionStrategy<TParameter> CreateStrategy<TParameter>(Action<TParameter?> syncAction)
    {
        return new FuncExecutionStrategy<TParameter>(syncAction);
    }

    public IExecutionStrategy<TParameter> CreateStrategy<TParameter>(Func<Task> asyncAction)
    {
        return new FuncExecutionStrategy<TParameter>(asyncAction);
    }

    public IExecutionStrategy<object> CreateStrategy(Func<Task> asyncAction)
    {
        return new FuncExecutionStrategy<object>(asyncAction);
    }

    public IExecutionStrategy<TParameter> CreateStrategy<TParameter>(Func<TParameter?, Task> asyncAction)
    {
        return new FuncExecutionStrategy<TParameter>(asyncAction);
    }
    
    public object CreateStrategy(object target, string methodName, Type? parameterType)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentException.ThrowIfNullOrEmpty(methodName);
        var type = target.GetType() ?? 
                   throw new InvalidOperationException("target.GetType() returned null");
        var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
                   throw new InvalidOperationException($"Method {methodName} not found in {type.FullName}");
        var strategyOpenType = GetStrategyOpenGenericType(method.ReturnType, parameterType != null);
        var genericParameterType = parameterType ?? typeof(object);
        var strategyClosedType = strategyOpenType.MakeGenericType(genericParameterType);
        var result = Activator.CreateInstance(strategyClosedType, target, methodName);
        return result??
               throw new InvalidOperationException("Activator.CreateInstance returned null");  
    }
    
    private Type GetStrategyOpenGenericType(Type returnType, bool hasParameter)
    {
        var isAsync = returnType == typeof(Task) || (returnType == typeof(void)? false: 
            throw new InvalidOperationException("Method must return void or Task."));
        return (isAsync, hasParameter) switch
        {
            (true, true) => 
                typeof(AsyncBindedMethodExecutionStrategy<>),
            (true, false) => 
                typeof(AsyncNoParameterBindedMethodExecutionStrategy<>),
            (false, true) => 
                typeof(SyncBindedMethodExecutionStrategy<>),
            (false, false) => 
                typeof(SyncNoParameterBindedMethodExecutionStrategy<>)
        };
    }
}