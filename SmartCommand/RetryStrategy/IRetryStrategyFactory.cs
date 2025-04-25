using System.Reflection;

namespace SmartCommand.RetryStrategy;

public interface IRetryStrategyFactory
{
    IRetryStrategy CreateRetryStrategy(PropertyInfo propertyInfo);
}