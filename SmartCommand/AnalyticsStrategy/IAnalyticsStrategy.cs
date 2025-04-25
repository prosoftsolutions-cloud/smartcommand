namespace SmartCommand.AnalyticsStrategy;

public interface IAnalyticsStrategy
{
    void TrackCommandStart(string commandName, DateTime startTime);
    void TrackCommandComplete(string commandName, DateTime endTime);
    void TrackCommandError(string commandName, Exception ex, DateTime errorTime);
}