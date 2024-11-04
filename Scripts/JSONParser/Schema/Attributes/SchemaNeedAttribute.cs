using ATS_API.Helpers;

[SchemaHelpURL("https://github.com/JamesVeug/AgainstTheStormAPI/blob/master/ATS_API/Scripts/Helpers/Enums/NeedTypes.cs", "https://hoodedhorse.com/wiki/Against_the_Storm/Complex_Needs")]
public class SchemaNeedAttribute : SchemaEnumAttribute<NeedTypes>
{
    public SchemaNeedAttribute() : base(NeedTypes.Biscuits, "Names of each Need the villager requires. Example: Housing, Food... etc")
    {
        
    }
}