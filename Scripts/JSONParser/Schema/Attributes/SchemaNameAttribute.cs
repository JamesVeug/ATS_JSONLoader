public class SchemaNameAttribute : SchemaFieldAttribute
{
    public SchemaNameAttribute() : base(null, true, "Unique identifier")
    {
        Pattern = "^[a-zA-Z\\d_]+$";
        MinLength = 1;
    }
}