namespace SmartCommand.RetryStrategy;

public interface IRetryStrategy
{
    bool CanRetry();
    void Retry();
    
    void ResetRetry();
}