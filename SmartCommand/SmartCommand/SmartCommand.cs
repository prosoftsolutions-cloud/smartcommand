using System.Windows.Input;
using SmartCommand.AnalyticsStrategy;
using SmartCommand.CanExecuteStrategy;
using SmartCommand.ErrorHandlingStrategy;
using SmartCommand.ExecutionStrategy;
using SmartCommand.ExecutionThrottlerStrategy;
using SmartCommand.RetryStrategy;

namespace SmartCommand.SmartCommand;

public class SmartCommand<TParameter> : ICommand
{
    private readonly IExecutionStrategy<TParameter> _executionStrategy;
    private readonly IErrorHandlingStrategy _errorHandler;
    private readonly ICanExecuteStrategy<TParameter> _canExecuteStrategy;
    private readonly IExecutionThrottlerStrategy _throttlerStrategy;
    private readonly IRetryStrategy _retryStrategy;
    private readonly TimeProvider _timeProvider;
    private readonly IAnalyticsStrategy _analyticsStrategy;
    private readonly string _commandName;

    public SmartCommand(
        IExecutionStrategy<TParameter> executionStrategy,
        ICanExecuteStrategy<TParameter> canExecuteStrategy,
        string commandName,
        IErrorHandlingStrategy errorHandlerStrategy,
        IExecutionThrottlerStrategy throttlerStrategy,
        IRetryStrategy retryStrategy,
        TimeProvider timeProvider,
        IAnalyticsStrategy analyticsStrategy)
    {
        _commandName = String.IsNullOrEmpty(commandName) ? Guid.NewGuid().ToString() : commandName;
        _executionStrategy = executionStrategy;
        _canExecuteStrategy = canExecuteStrategy;
        _errorHandler = errorHandlerStrategy;
        _throttlerStrategy = throttlerStrategy;
        _retryStrategy = retryStrategy;
        _timeProvider = timeProvider;
        _analyticsStrategy = analyticsStrategy;
    }

    public bool CanExecute(object? parameter)
    {
        return _throttlerStrategy.CanStart() && _canExecuteStrategy.CanExecute((TParameter)parameter!);
    }

    public async void Execute(object? parameter)
    {
        _retryStrategy.ResetRetry();
        while (_retryStrategy.CanRetry())
        {
            try
            {
                _retryStrategy.Retry();
                _throttlerStrategy.MarkStarted();
                if (!CanExecute(parameter))
                {
                    continue;
                }
                _analyticsStrategy.TrackCommandStart(_commandName, _timeProvider.GetUtcNow().DateTime);
                RaiseCanExecuteChanged();
                await _executionStrategy.ExecuteAsync((TParameter)parameter!);
                _analyticsStrategy.TrackCommandComplete(_commandName, _timeProvider.GetUtcNow().DateTime);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex, $"Error executing command {_commandName}:");
                _analyticsStrategy.TrackCommandError(_commandName, ex, _timeProvider.GetUtcNow().DateTime);
            }
            finally
            {
                _throttlerStrategy.MarkFinished();
                RaiseCanExecuteChanged();
            }
        }
    }

    private void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? CanExecuteChanged;
}