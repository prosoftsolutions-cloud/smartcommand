namespace SmartCommand.ErrorHandlingStrategy;

public class PanicErrorHandlingStrategy : IErrorHandlingStrategy
{
    public void HandleError(Exception ex, string message = "")
    {
        throw new Exception(message, ex);//Exception from exception handler leads to panic and global error handler call
    }
}