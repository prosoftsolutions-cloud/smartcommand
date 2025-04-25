namespace SmartCommand.ExecutionStrategy;

public class SyncNoParameterBindedMethodExecutionStrategy<TParameter> : BindedMethodExecutionStrategy<TParameter>
{
    public SyncNoParameterBindedMethodExecutionStrategy(object target, string methodName) : base(target, methodName)
    {
        if (Method.ReturnType != typeof(void))
        {
            throw new ArgumentException("Method must return void");       
        }
        if (Method.GetParameters().Length != 0)
        {
            throw new ArgumentException("Method must have no parameters");
        }       
    }

    protected override Task InnerExecuteAsync(TParameter? parameter)
    {
        Method.Invoke(Target, null);
        return Task.CompletedTask;
    }
}