using System;

public class SchemaFieldAttribute : Attribute
{
    protected object DefaultValue;
    protected bool Required;
    protected string Description;
    protected string Pattern;
    protected int MinLength;
    protected string[] Enum = [];

    public SchemaFieldAttribute(object defaultValue, bool required, string description)
    {
        DefaultValue = defaultValue;
        Required = required;
        Description = description;
    }
    
    public SchemaFieldAttribute(object defaultValue, string description)
    {
        DefaultValue = defaultValue;
        Required = false;
        Description = description;
    }
    
    public SchemaFieldAttribute(object defaultValue)
    {
        DefaultValue = defaultValue;
        Required = false;
        Description = "";
    }
    
    public virtual object GetDefaultValue()
    {
        return DefaultValue;
    }
    
    public virtual bool IsRequired()
    {
        return Required;
    }
    
    public virtual string GetDescription()
    {
        return Description;
    }
    
    public virtual string GetPattern()
    {
        return Pattern;
    }
    
    public virtual int GetMinLength()
    {
        return MinLength;
    }
    
    public virtual string[] GetEnum()
    {
        return Enum;
    }
}