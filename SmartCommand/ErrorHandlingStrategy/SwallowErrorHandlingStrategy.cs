namespace SmartCommand.ErrorHandlingStrategy;

public class SwallowErrorHandlingStrategy : IErrorHandlingStrategy
{
    public void HandleError(Exception ex, string message = "")
    {
        //do nothing
    }
}