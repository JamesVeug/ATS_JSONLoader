using System;
using ATS_API.Helpers;

[SchemaHelpURL("https://github.com/JamesVeug/AgainstTheStormAPI/blob/master/ATS_API/Scripts/Helpers/Enums/AscensionModifierTypes.cs", "https://hoodedhorse.com/wiki/Against_the_Storm/Difficulty")]
public class SchemaAscensionModifierTypeAttribute : SchemaEnumAttribute<AscensionModifierTypes>
{
    public SchemaAscensionModifierTypeAttribute(string description) : base(AscensionModifierTypes.Parasites, description)
    {
    }
}