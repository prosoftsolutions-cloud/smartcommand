using System.Reflection;

namespace SmartCommand.AnalyticsStrategy;

public interface IAnalyticsStrategyFactory
{
    IAnalyticsStrategy CreateAnalyticsStrategy(PropertyInfo propertyInfo);
}