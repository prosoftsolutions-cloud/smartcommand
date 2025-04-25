namespace SmartCommand.ExecutionThrottlerStrategy;

public class CountedExecutionThrottlerStrategy : IExecutionThrottlerStrategy
{
    private int _count = 0;
    private readonly int _maxPalallelExecutions;
    public CountedExecutionThrottlerStrategy(int maxPalallelExecutions = 1)
    {
        _maxPalallelExecutions = maxPalallelExecutions;
    }
    
    public int MaxParallelExecution => _maxPalallelExecutions;
    
    public bool CanStart()
    {
        return _count < _maxPalallelExecutions;
    }

    public void MarkStarted()
    {
        _count++;
    }

    public void MarkFinished()
    {
        _count--;
    }
}