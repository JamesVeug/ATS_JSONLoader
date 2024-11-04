using System;
using ATS_API.Helpers;

[SchemaHelpURL("https://github.com/JamesVeug/AgainstTheStormAPI/blob/master/ATS_API/Scripts/Helpers/Enums/EffectTypes.cs", "https://hoodedhorse.com/wiki/Against_the_Storm/Perks")]
public class SchemaEffectTypeAttribute : SchemaEnumAttribute<EffectTypes>
{
    public SchemaEffectTypeAttribute(EffectTypes defaultValue, string description) : base(defaultValue, description)
    {
    }
}