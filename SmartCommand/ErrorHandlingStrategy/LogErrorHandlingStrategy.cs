using Microsoft.Extensions.Logging;

namespace SmartCommand.ErrorHandlingStrategy;

public class LogErrorHandlingStrategy : IErrorHandlingStrategy
{
    private readonly ILogger _logger;
    
    public LogErrorHandlingStrategy(ILogger logger)
    {
        _logger = logger;
    }
    
    public void HandleError(Exception ex, string message = "")
    {
        _logger.Log(LogLevel.Error, ex, message);
    }
}