using System.ComponentModel;
using System.Runtime.CompilerServices;
using SmartCommand.SmartCommand;

namespace SmartCommand.Base;

public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly IMainThreadDispatcher _mainThreadDispatcher;

    public BaseViewModel(IMainThreadDispatcher mainThreadDispatcher, ISmartCommandFactory? factory = null)
    {
        _mainThreadDispatcher = mainThreadDispatcher;
        factory?.CreateSmartCommandsFor(this);
    }
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        _mainThreadDispatcher.InvokeOnMainThread(() =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
    }
    
    protected void SetValue<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}