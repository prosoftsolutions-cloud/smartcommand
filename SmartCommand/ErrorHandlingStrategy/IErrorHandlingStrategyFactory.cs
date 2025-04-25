using System.Reflection;

namespace SmartCommand.ErrorHandlingStrategy;

public interface IErrorHandlingStrategyFactory
{
    IErrorHandlingStrategy CreateErrorHandlingStrategy(PropertyInfo propertyInfo);
}