namespace SmartCommand.Base;

public class SmartCommandBaseAttribute<T> : Attribute
{
    protected SmartCommandBaseAttribute()
    {
        var type = GetType();
        if (type == typeof(SmartCommandBaseAttribute<>))
        {
            throw new InvalidOperationException("SmartCommandBaseAttribute cannot be used directly. Inherit from it instead.");
        }
    }
}