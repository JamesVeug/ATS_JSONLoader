using System;
using ATS_API.Helpers;

[SchemaHelpURL("https://github.com/JamesVeug/AgainstTheStormAPI/blob/master/ATS_API/Scripts/Helpers/Enums/BuildingTagTypes.cs")]
public class SchemaBuildingTagTypeAttribute : SchemaEnumAttribute<BuildingTagTypes>
{
    public SchemaBuildingTagTypeAttribute(string description) : base(BuildingTagTypes.Alchemy, description)
    {
    }
}