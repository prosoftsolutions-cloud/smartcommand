namespace SmartCommand.AnalyticsStrategy;

public class SwallowAnalyticsStrategy : IAnalyticsStrategy
{
    public void TrackCommandStart(string commandName, DateTime startTime)
    {
        //Do nothing
    }

    public void TrackCommandComplete(string commandName, DateTime endTime)
    {
        //Do nothing
    }

    public void TrackCommandError(string commandName, Exception ex, DateTime errorTime)
    {
        //Do nothing
    }
}