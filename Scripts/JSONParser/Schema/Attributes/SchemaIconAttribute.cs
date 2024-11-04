using ATS_API.Helpers;
using UnityEngine;

public class SchemaIconAttribute : SchemaFieldAttribute
{
    private TextureHelper.SpriteType spriteType;
    
    public SchemaIconAttribute(TextureHelper.SpriteType spriteType) : base("", false, "Name of the file containing the icon for this object. Example: MyCustomGood.png")
    {
        this.spriteType = spriteType;
    }
    
    public SchemaIconAttribute(TextureHelper.SpriteType spriteType, string description) : base("", false, "Name of the file containing the icon for this object. Example: MyCustomGood.png\n" + description)
    {
        this.spriteType = spriteType;        
    }

    public override string GetDescription()
    {
        Vector2 size = TextureHelper.GetSpriteSize(spriteType); 
        return base.GetDescription() + "\n" + $"Icon size: {(int)size.x}x{(int)size.y} pixels";
    }
}