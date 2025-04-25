using System.Reflection;
using SmartCommand.Base;

namespace SmartCommand.ExecutionThrottlerStrategy;

public class DefaultExecutionThrottlerStrategyFactory : SmartCommandBaseFactory<IExecutionThrottlerStrategy>, IExecutionThrottlerStrategyFactory
{
    public IExecutionThrottlerStrategy CreateExecutionThrottlerStrategy(PropertyInfo propertyInfo)
    {
        return CreateInstance(propertyInfo);
    }

    protected override IExecutionThrottlerStrategy CreateDefaultInstance()
    {
        return new CountedExecutionThrottlerStrategy();
    }

    protected override IExecutionThrottlerStrategy CreateSingleInstance(SmartCommandBaseAttribute<IExecutionThrottlerStrategy> attribute)
    {
        if (attribute is CountedExecutionThrottlerStrategyAttribute countedAttribute)
        {
            return new CountedExecutionThrottlerStrategy(countedAttribute.MaxParallelExecution);
        }
        throw new NotSupportedException("Unknown attribute for throttling strategy is not supported.");
    }

    protected override IExecutionThrottlerStrategy CreateCompositeInstance(IList<SmartCommandBaseAttribute<IExecutionThrottlerStrategy>> attributes)
    {
        throw new NotSupportedException("Composite throttling strategy is not supported.");
    }
}