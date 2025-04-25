using System.Reflection;

namespace SmartCommand.CanExecuteStrategy;

public class CanExecutedMethodStrategy<TParameter> : ICanExecuteStrategy<TParameter>
{
    private readonly object _target;
    private readonly MethodInfo _method;
    
    public CanExecutedMethodStrategy(object target, string methodName)
    {
        _target = target;
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
        {
            throw new ArgumentException($"Method {methodName} not found in {target.GetType().FullName}");
        }
        _method = method;
        if (_method.ReturnType != typeof(bool))
        {
            throw new InvalidOperationException($"Method '{methodName}' must return a boolean.");
        }
        var parameters = _method.GetParameters();
        if (parameters.Length != 1)
        {
            throw new InvalidOperationException($"Method '{methodName}' must accept exactly one parameter.");
        }

        var expectedType = typeof(TParameter);
        var actualType = parameters[0].ParameterType;

        if (!actualType.IsAssignableFrom(expectedType))
        {
            throw new InvalidOperationException(
                $"Method '{methodName}' parameter type must be assignable from '{expectedType.FullName}', but was '{actualType.FullName}'.");
        }
    }
    
    public bool CanExecute(TParameter parameter)
    {
        var result = _method.Invoke(_target, [parameter]);
        return (bool) result!;
    }
}