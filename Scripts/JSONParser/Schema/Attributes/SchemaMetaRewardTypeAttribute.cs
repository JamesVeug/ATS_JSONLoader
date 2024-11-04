using System;
using ATS_API.Helpers;

[SchemaHelpURL("https://github.com/JamesVeug/AgainstTheStormAPI/blob/master/ATS_API/Scripts/Helpers/Enums/MetaRewardTypes.cs")]
public class SchemaMetaRewardTypeAttribute : SchemaEnumAttribute<MetaRewardTypes>
{
    public SchemaMetaRewardTypeAttribute(MetaRewardTypes defaultValue, string description) : base(defaultValue, description)
    {
    }
}