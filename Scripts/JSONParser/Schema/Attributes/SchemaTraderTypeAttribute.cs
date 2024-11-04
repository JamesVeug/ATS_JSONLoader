using System;
using ATS_API.Helpers;

[SchemaHelpURL("https://github.com/JamesVeug/AgainstTheStormAPI/blob/master/ATS_API/Scripts/Helpers/Enums/TraderTypes.cs", "https://hoodedhorse.com/wiki/Against_the_Storm/Trading")]
public class SchemaTraderTypeAttribute : SchemaEnumAttribute<TraderTypes>
{
    public SchemaTraderTypeAttribute(TraderTypes defaultValue, string description) : base(defaultValue, description)
    {
    }
}