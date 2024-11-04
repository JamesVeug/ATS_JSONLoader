using System;

public class SchemaHelpURLAttribute : Attribute
{
    public string URL;
    
    public SchemaHelpURLAttribute(string url)
    {
        URL = url;
    }
    
    public SchemaHelpURLAttribute(string urlA, string urlB)
    {
        URL = $"{urlA}\n{urlB}";
    }
}