namespace SmartCommand.Base;

public interface IMainThreadDispatcher
{
    void InvokeOnMainThread(Action action);
}