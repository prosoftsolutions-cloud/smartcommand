namespace SmartCommand.CanExecuteStrategy;

public class DefaultCanExecuteStrategyFactory : ICanExecuteStrategyFactory
{
    public object CreateCanExecuteStrategy(object target, string methodName, Type parameterType)
    {
        if (String.IsNullOrEmpty(methodName))
        {
            return new AlwaysCanExecuteStrategy<object>();
        }
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(parameterType);
        var genericType = typeof(CanExecutedMethodStrategy<>).MakeGenericType(parameterType);
        var instance = Activator.CreateInstance(genericType, target, methodName);
        return instance?? throw new InvalidOperationException("Activator failed to create instance of CanExecutedMethodStrategy");
    }
}