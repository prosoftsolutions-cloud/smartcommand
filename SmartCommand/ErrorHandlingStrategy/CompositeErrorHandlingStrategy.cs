namespace SmartCommand.ErrorHandlingStrategy;

public class CompositeErrorHandlingStrategy : IErrorHandlingStrategy
{
    
    private readonly IErrorHandlingStrategy[] _handlers;

    public CompositeErrorHandlingStrategy(IEnumerable<IErrorHandlingStrategy> handlers)
    {
        _handlers = handlers.ToArray();
    }
    public void HandleError(Exception ex, string message = "")
    {
        foreach (var handler in _handlers)
        {
            handler.HandleError(ex, message);
        }
    }
}