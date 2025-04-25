# Smart Command
Attribute-based command configuration for ICommand interface: WPF, Maui, etc.
## Basic Usage
```C#
class MyViewModel : BaseViewModel
{
    public MyViewModel(IMainThreadDispatcher mainThreadDispatcher, ISmartCommandFactory factory): base(mainThreadDispatcher, factory)
    {
        
    }
    private Task Execute(int i)
    {
        
    }
    
    private bool CanExecute(int i)
    {
        
    }
    
    [SmartCommand(BindingMethod = nameof(Execute), CanExecuteMethod = nameof(CanExecute)] 
    [LogAnalyticsStrategy(AnalyticsLogLevel = LogLevel.Info)]
    [LogErrorHandlingStrategy]
    [SimpleRetryStrategy(RetryCount = 4)]
    public ICommand MyCommand {get;set;}
}
```
See more usage sample on tests