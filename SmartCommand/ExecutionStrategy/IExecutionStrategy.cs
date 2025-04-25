namespace SmartCommand.ExecutionStrategy;

public interface IExecutionStrategy<in TParameter>
{
    Task ExecuteAsync(TParameter? parameter);
}