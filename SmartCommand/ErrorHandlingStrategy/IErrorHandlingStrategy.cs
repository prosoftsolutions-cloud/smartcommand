namespace SmartCommand.ErrorHandlingStrategy;

public interface IErrorHandlingStrategy
{
    void HandleError(Exception ex, string message = "");
}