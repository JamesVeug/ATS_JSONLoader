public class SchemaGuidAttribute : SchemaFieldAttribute
{
    public SchemaGuidAttribute() : base(null, true, "Unique identifier for the mod that added this. Blank if it's added as part of the base game.")
    {
        Pattern = "^[a-zA-Z\\d]+$";
    }
}