using System.Windows.Input;
using SmartCommand.Base;

namespace SmartCommand.SmartCommand;

public class SmartCommandAttribute :SmartCommandBaseAttribute<ICommand>
{
    private string _bindingMethod = String.Empty;

    public required string BindingMethod
    {
        get => _bindingMethod;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Binding Method must not be null or empty", nameof(value));
            }
            _bindingMethod = value;       
        }
    }
    public string CanExecuteMethod { get; set; } = String.Empty;

    public string CommandIdentifier { get; set; } = String.Empty;
}