namespace SmartCommand.ExecutionStrategy;

public class FuncExecutionStrategy<TParameter> : IExecutionStrategy<TParameter>
{
    private readonly Func<TParameter?, Task> _func;

    public FuncExecutionStrategy(Func<TParameter?, Task> asyncFuncWithParameter)
    {
        _func = asyncFuncWithParameter;       
    }
    
    public FuncExecutionStrategy(Func<Task> asyncFunc):this(_ => asyncFunc())
    {
    }
    
    public FuncExecutionStrategy(Action<TParameter?> syncFuncWithParameter):
        this(parameter =>
        {
            syncFuncWithParameter(parameter);
            return Task.CompletedTask;
        })
    {
    }
    public FuncExecutionStrategy(Action syncFunc):
        this(() =>
        {
            syncFunc();
            return Task.CompletedTask;
        })
    {
    }
    
    public async Task ExecuteAsync(TParameter? parameter)
    {
        await _func(parameter).ConfigureAwait(false);
    }
}