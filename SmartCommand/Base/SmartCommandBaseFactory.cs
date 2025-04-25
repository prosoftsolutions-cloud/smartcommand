using System.Reflection;

namespace SmartCommand.Base;

public abstract class SmartCommandBaseFactory<TInterface>
{
    protected abstract TInterface CreateDefaultInstance(); 
    protected abstract TInterface CreateSingleInstance(SmartCommandBaseAttribute<TInterface> attribute);
    
    protected abstract TInterface CreateCompositeInstance(IList<SmartCommandBaseAttribute<TInterface>> attributes);
    
    protected TInterface CreateInstance(PropertyInfo propertyInfo)
    {
        var attributes = propertyInfo
            .GetCustomAttributes(inherit: true)
            .OfType<SmartCommandBaseAttribute<TInterface>>()
            .ToList();
        return attributes.Count switch
        {
            0 => CreateDefaultInstance(),
            1 => CreateSingleInstance(attributes.First()),
            _ => CreateCompositeInstance(attributes)
        };
    }
}