namespace SmartCommand.RetryStrategy;

public class SimpleRetryStrategy : IRetryStrategy
{
    private int _retryCount = 0;
    private readonly int _maxRetryCount;

    public SimpleRetryStrategy(int maxRetryCount = 1)
    {
        _maxRetryCount = maxRetryCount;
    }

    public int MaxRetryCount => _maxRetryCount;

    public bool CanRetry()
    {
        return _retryCount < _maxRetryCount;
    }

    public void Retry()
    {
        _retryCount++;
    }

    public void ResetRetry()
    {
        _retryCount = 0;
    }
}