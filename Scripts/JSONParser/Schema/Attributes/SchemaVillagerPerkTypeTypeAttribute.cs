using System;
using ATS_API.Helpers;

[SchemaHelpURL("https://github.com/JamesVeug/AgainstTheStormAPI/blob/master/ATS_API/Scripts/Helpers/Enums/VillagerPerkTypes.cs")]
public class SchemaVillagerPerkTypeTypeAttribute : SchemaEnumAttribute<VillagerPerkTypes>
{
    public SchemaVillagerPerkTypeTypeAttribute(string description) : base(VillagerPerkTypes.Proficiency, description)
    {
    }
}