namespace SmartCommand.CanExecuteStrategy;

public class CanExecuteFuncStrategy<TParameter> : ICanExecuteStrategy<TParameter>
{
    private readonly Func<TParameter, bool> _canExecute;
    
    public CanExecuteFuncStrategy(Func<TParameter, bool>? canExecute = null)
    {
        _canExecute = canExecute ?? (_ => true);
    }
    
    public bool CanExecute(TParameter parameter)
    {
        return _canExecute(parameter);
    }
}