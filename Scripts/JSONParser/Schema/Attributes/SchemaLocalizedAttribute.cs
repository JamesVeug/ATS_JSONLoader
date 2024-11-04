public class SchemaLocalizedAttribute : SchemaFieldAttribute
{
    public SchemaLocalizedAttribute(string description) : base(null, description + "\n" + LocalizedSubtext)
    {
        
    }

    private const string LocalizedSubtext = "Defaults to english. To include more languages, add a new field with the language code to the end of the key. For example, 'displayName' => 'displayName_pl' for polish.";
}