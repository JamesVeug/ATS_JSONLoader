using System;
using System.Linq;

public class SchemaEnumAttribute<T> : SchemaFieldAttribute where T: Enum
{
    private static readonly string[] IgnoreEnumNames = ["None", "Unknown", "MAX"];

    public SchemaEnumAttribute(T defaultValue, string description) : base(defaultValue, false, description)
    {
        Enum = System.Enum.GetNames(typeof(T)).Where(a=>!IgnoreEnumNames.Contains(a)).ToArray();
    }
    
    public SchemaEnumAttribute(T defaultValue, bool required, string description) : base(defaultValue, required, description)
    {
        Enum = System.Enum.GetNames(typeof(T)).Where(a=>!IgnoreEnumNames.Contains(a)).ToArray();
    }
}