namespace SmartCommand.CanExecuteStrategy;

public class AlwaysCanExecuteStrategy<TParameter> : ICanExecuteStrategy<TParameter>
{
    public bool CanExecute(TParameter parameter)
    {
        return true;
    }
}