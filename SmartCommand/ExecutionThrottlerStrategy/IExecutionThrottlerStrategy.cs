namespace SmartCommand.ExecutionThrottlerStrategy;

public interface IExecutionThrottlerStrategy
{
    bool CanStart();
    void MarkStarted();
    void MarkFinished();
}