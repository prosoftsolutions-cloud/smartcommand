using System.Reflection;

namespace SmartCommand.ExecutionThrottlerStrategy;

public interface IExecutionThrottlerStrategyFactory
{
    IExecutionThrottlerStrategy CreateExecutionThrottlerStrategy(PropertyInfo propertyInfo);
}