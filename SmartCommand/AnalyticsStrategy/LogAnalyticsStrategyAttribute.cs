using Microsoft.Extensions.Logging;
using SmartCommand.Base;

namespace SmartCommand.AnalyticsStrategy;

[AttributeUsage(AttributeTargets.Property)]
public class LogAnalyticsStrategyAttribute : SmartCommandBaseAttribute<IAnalyticsStrategy>
{
    public LogLevel AnalyticsLogLevel { get; set; }
}