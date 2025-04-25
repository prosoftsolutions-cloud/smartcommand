using System.Reflection;
using Microsoft.Extensions.Logging;
using SmartCommand.Base;

namespace SmartCommand.ErrorHandlingStrategy;

public class DefaultErrorHandlingStrategyFactory : SmartCommandBaseFactory<IErrorHandlingStrategy>, IErrorHandlingStrategyFactory
{
    private readonly ILogger _logger;

    public DefaultErrorHandlingStrategyFactory(ILogger logger)
    {
        _logger = logger;
    }
    
    public IErrorHandlingStrategy CreateErrorHandlingStrategy(PropertyInfo propertyInfo)
    {
        var result = CreateInstance(propertyInfo);
        return result;
    }

    protected override IErrorHandlingStrategy CreateDefaultInstance()
    {
        return new SwallowErrorHandlingStrategy();
    }

    protected override IErrorHandlingStrategy CreateSingleInstance(SmartCommandBaseAttribute<IErrorHandlingStrategy> attribute)
    {
        if (attribute is LogErrorHandlingStrategyAttribute)
        {
            return new LogErrorHandlingStrategy(_logger);
        }
        if (attribute is PanicErrorHandlingStrategyAttribute)
        {
            return new PanicErrorHandlingStrategy();
        }
        throw new NotSupportedException("Unknown attribute for error handling strategy is not supported.");
    }

    protected override IErrorHandlingStrategy CreateCompositeInstance(IList<SmartCommandBaseAttribute<IErrorHandlingStrategy>> attributes)
    {
        var list = new List<IErrorHandlingStrategy>();
        foreach (var attribute in attributes)
        {
            list.Add(CreateSingleInstance(attribute));
        }
        return new CompositeErrorHandlingStrategy(list);
    }
}