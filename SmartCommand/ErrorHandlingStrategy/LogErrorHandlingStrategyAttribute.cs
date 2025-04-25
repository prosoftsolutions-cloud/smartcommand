using SmartCommand.Base;

namespace SmartCommand.ErrorHandlingStrategy;

[AttributeUsage(AttributeTargets.Property)]
public class LogErrorHandlingStrategyAttribute : SmartCommandBaseAttribute<IErrorHandlingStrategy>
{
    
}