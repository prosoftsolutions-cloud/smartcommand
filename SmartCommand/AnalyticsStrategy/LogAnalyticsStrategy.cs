using Microsoft.Extensions.Logging;

namespace SmartCommand.AnalyticsStrategy;

public class LogAnalyticsStrategy : IAnalyticsStrategy
{
    private readonly ILogger _logger;
    private readonly LogLevel _logLevel;
    public LogAnalyticsStrategy(ILogger logger, LogLevel logLevel)
    {
        _logger = logger;
        _logLevel = logLevel;
    }
    public LogLevel LogLevel => _logLevel;
    public void TrackCommandStart(string commandName, DateTime startTime)
    {
        _logger.Log(_logLevel, "Command {CommandName} started at {StartTime}", commandName, startTime);
    }

    public void TrackCommandComplete(string commandName, DateTime endTime)
    {
        _logger.Log(_logLevel, "Command {CommandName} stopped at {StartTime}", commandName, endTime);
    }

    public void TrackCommandError(string commandName, Exception ex, DateTime errorTime)
    {
        _logger.Log(_logLevel, ex, "Command {CommandName} stopped at {ErrorTime}", commandName, errorTime);
    }
}