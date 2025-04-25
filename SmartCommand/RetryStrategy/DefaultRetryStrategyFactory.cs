using System.Reflection;
using SmartCommand.Base;

namespace SmartCommand.RetryStrategy;

public class DefaultRetryStrategyFactory : SmartCommandBaseFactory<IRetryStrategy>, IRetryStrategyFactory
{
    public IRetryStrategy CreateRetryStrategy(PropertyInfo propertyInfo)
    {
        var result = CreateInstance(propertyInfo);
        return result;
    }

    protected override IRetryStrategy CreateDefaultInstance()
    {
        return new SimpleRetryStrategy();
    }

    protected override IRetryStrategy CreateSingleInstance(SmartCommandBaseAttribute<IRetryStrategy> attribute)
    {
        if (attribute is SimpleRetryStrategyAttribute simpleRetryStrategyAttribute)
        {
            return new SimpleRetryStrategy(simpleRetryStrategyAttribute.RetryCount);
        }
        throw new NotSupportedException("Unknown attribute for retry strategy is not supported.");
    }

    protected override IRetryStrategy CreateCompositeInstance(IList<SmartCommandBaseAttribute<IRetryStrategy>> attributes)
    {
        throw new NotSupportedException("Composite retry strategy is not supported.");
    }
}