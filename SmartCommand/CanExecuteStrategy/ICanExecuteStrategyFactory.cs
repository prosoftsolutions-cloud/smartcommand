namespace SmartCommand.CanExecuteStrategy;

public interface ICanExecuteStrategyFactory
{
    object CreateCanExecuteStrategy(object target, string methodName, Type parameterType);
}