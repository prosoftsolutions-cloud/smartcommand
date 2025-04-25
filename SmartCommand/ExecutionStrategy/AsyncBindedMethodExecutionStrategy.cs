namespace SmartCommand.ExecutionStrategy;

public class AsyncBindedMethodExecutionStrategy<TParameter> : BindedMethodExecutionStrategy<TParameter>
{
    public AsyncBindedMethodExecutionStrategy(object target, string methodName) : base(target, methodName)
    {
        if (Method.ReturnType != typeof(Task))
        {
            throw new ArgumentException("Method must return Task");       
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

    protected override async Task InnerExecuteAsync(TParameter? parameter)
    {
        var result = (Task)Method.Invoke(Target, [parameter])!;
        await result.ConfigureAwait(false);
    }
}