using System;
using ATS_API.Helpers;

[SchemaHelpURL("https://github.com/JamesVeug/AgainstTheStormAPI/blob/master/ATS_API/Scripts/Helpers/Enums/GoodsTypes.cs", "https://hoodedhorse.com/wiki/Against_the_Storm/Resources")]
public class SchemaGoodTypeAttribute : SchemaEnumAttribute<GoodsTypes>
{
    public SchemaGoodTypeAttribute(string description) : base(GoodsTypes.Valuable_Amber, description)
    {
    }
}