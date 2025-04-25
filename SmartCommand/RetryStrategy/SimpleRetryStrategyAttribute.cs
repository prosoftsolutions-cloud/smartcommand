using SmartCommand.Base;

namespace SmartCommand.RetryStrategy;

[AttributeUsage(AttributeTargets.Property)]
public class SimpleRetryStrategyAttribute : SmartCommandBaseAttribute<IRetryStrategy>
{
    public int RetryCount { get; set; } = 1;
}