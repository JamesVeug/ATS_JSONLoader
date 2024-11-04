using System;

public class GenerateSchemaAttribute : Attribute
{
    private readonly string Title;
    private readonly string Description;

    public GenerateSchemaAttribute(string title, string description)
    {
        Title = title;
        Description = description;
    }
    
    public string GetTitle()
    {
        return Title;
    }
    
    public string GetDescription()
    {
        return Description;
    }
}