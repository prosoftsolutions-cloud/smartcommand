namespace SmartCommand.CanExecuteStrategy;

public interface ICanExecuteStrategy<in TParameter>
{
    bool CanExecute(TParameter parameter);
}