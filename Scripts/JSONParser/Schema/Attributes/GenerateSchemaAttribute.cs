using System;

public class GenerateSchemaAttribute : Attribute
{
    private readonly string Title;
    private readonly string Description;
    private readonly string FileExtension;
    

    public GenerateSchemaAttribute(string title, string description, string fileExtension)
    {
        Title = title;
        Description = description;
        FileExtension = fileExtension;
    }
    
    public string GetTitle()
    {
        return Title;
    }
    
    public string GetDescription()
    {
        string description = Description;

        if (!string.IsNullOrEmpty(FileExtension))
        {
            description += "\n\nFor JSONLoader to load this file into Against the Storm create a file on your computer ending with " + FileExtension + " and paste the contents of the JSON inside. Example: \"MyFile" + FileExtension + "\"";
        }
        return description;
    }
    
    public string GetFileExtension()
    {
        return FileExtension;
    }
}