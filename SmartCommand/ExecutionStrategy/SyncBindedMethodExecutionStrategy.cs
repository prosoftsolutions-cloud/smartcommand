namespace SmartCommand.ExecutionStrategy;

public class SyncBindedMethodExecutionStrategy<TParameter> : BindedMethodExecutionStrategy<TParameter>
{
    public SyncBindedMethodExecutionStrategy(object target, string methodName) : base(target, methodName)
    {
        if (Method.ReturnType != typeof(void))
        {
            throw new ArgumentException("Method must return void");       
        }
        var parameters = Method.GetParameters();
        if (parameters.Length != 1)
        {
            throw new ArgumentException($"Method must have exactly 1 parameter of type {typeof(TParameter).FullName}");
        }
        if (parameters[0].ParameterType != typeof(TParameter))
        {
            throw new ArgumentException($"Method parameter must be of type {typeof(TParameter).FullName}");
        }       
    }

    protected override Task InnerExecuteAsync(TParameter? parameter)
    {
        Method.Invoke(Target, [parameter]);
        return Task.CompletedTask;
    }

}