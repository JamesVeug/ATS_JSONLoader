using System;
using ATS_API.Helpers;

[SchemaHelpURL("https://github.com/JamesVeug/AgainstTheStormAPI/blob/master/ATS_API/Scripts/Helpers/Enums/BuildingPerkTypes.cs")]
public class SchemaBuildingPerkTypeAttribute : SchemaEnumAttribute<BuildingPerkTypes>
{
    public SchemaBuildingPerkTypeAttribute(string description) : base(BuildingPerkTypes.Hauler_Cart, description)
    {
    }
}