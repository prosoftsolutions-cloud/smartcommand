using SmartCommand.Base;

namespace SmartCommand.ExecutionThrottlerStrategy;

[AttributeUsage(AttributeTargets.Property)]
public class CountedExecutionThrottlerStrategyAttribute : SmartCommandBaseAttribute<IExecutionThrottlerStrategy>
{
    public int MaxParallelExecution { get; set; } = 1;
}