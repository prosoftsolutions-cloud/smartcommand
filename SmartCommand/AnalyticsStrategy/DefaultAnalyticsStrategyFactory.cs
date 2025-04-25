using System.Reflection;
using Microsoft.Extensions.Logging;
using SmartCommand.Base;

namespace SmartCommand.AnalyticsStrategy;

public class DefaultAnalyticsStrategyFactory : SmartCommandBaseFactory<IAnalyticsStrategy>, IAnalyticsStrategyFactory
{
    private readonly ILogger _logger;

    public DefaultAnalyticsStrategyFactory(ILogger logger)
    {
        _logger = logger;
    }
    
    public IAnalyticsStrategy CreateAnalyticsStrategy(PropertyInfo propertyInfo)
    {
        return CreateInstance(propertyInfo);
    }

    protected override IAnalyticsStrategy CreateDefaultInstance()
    {
        return new SwallowAnalyticsStrategy();
    }

    protected override IAnalyticsStrategy CreateSingleInstance(SmartCommandBaseAttribute<IAnalyticsStrategy> attribute)
    {
        if (attribute is LogAnalyticsStrategyAttribute logAttribute)
        {
            return new LogAnalyticsStrategy(_logger, logAttribute.AnalyticsLogLevel);
        }
        throw new NotSupportedException("Unknown attribute for analytics strategy is not supported.");
    }

    protected override IAnalyticsStrategy CreateCompositeInstance(IList<SmartCommandBaseAttribute<IAnalyticsStrategy>> attributes)
    {
        throw new NotSupportedException("Composite analytics strategy is not supported.");
    }
}