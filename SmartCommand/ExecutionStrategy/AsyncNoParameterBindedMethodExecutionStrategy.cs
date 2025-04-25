namespace SmartCommand.ExecutionStrategy;

public class AsyncNoParameterBindedMethodExecutionStrategy<TParameter> : BindedMethodExecutionStrategy<TParameter>
{
    public AsyncNoParameterBindedMethodExecutionStrategy(object target, string methodName) : base(target, methodName)
    {
        if (Method.ReturnType != typeof(Task))
        {
            throw new ArgumentException("Method must return Task");       
        }
        if (Method.GetParameters().Length != 0)
        {
            throw new ArgumentException("Method must have no parameters");
        }
    }

    protected override async Task InnerExecuteAsync(TParameter? parameter)
    {
        var result = (Task)Method.Invoke(Target, null)!;
        await result.ConfigureAwait(false);
    }
}