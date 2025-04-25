using System.Reflection;
using System.Runtime.ExceptionServices;

namespace SmartCommand.ExecutionStrategy;

public abstract class BindedMethodExecutionStrategy<TParameter> : IExecutionStrategy<TParameter> 
{
    protected MethodInfo Method { get; }
    protected object Target { get; }
    protected Type TargetType { get; }

    public BindedMethodExecutionStrategy(object target, string methodName)
    {
        ArgumentNullException.ThrowIfNull(target);
        Target = target;
        TargetType = target.GetType();
        var methodInfo = TargetType.GetMethod(methodName);
        if (methodInfo == null)
        {
            throw new ArgumentException($"Method {methodName} not found in {target.GetType().FullName}");
        }
        if (methodInfo.IsStatic)
        {
            throw new ArgumentException("Method must not be static");
        }

        Method = methodInfo;
    }

    protected abstract Task InnerExecuteAsync(TParameter? parameter);

    public async Task ExecuteAsync(TParameter? parameter)
    {
        try
        {
            await InnerExecuteAsync(parameter).ConfigureAwait(false);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
        }
    }
}