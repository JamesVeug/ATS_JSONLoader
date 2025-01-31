using System.Linq;
using ATS_API.Helpers;
using ATS_API.Recipes;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;
using UnityEngine;

public class HelperMethods
{
    public class GoodRefData : IJSONConvertsTo<GoodRef>
    {
        public string goodName;
        public int amount;

        public GoodRef ConvertTo()
        {
            GoodRef goodRef = new GoodRef();
            goodRef.good = goodName.ToGoodModel();
            goodRef.amount = amount;
            return goodRef;
        }
        
        public void ConvertFrom(GoodRef goodRef)
        {
            goodName = goodRef.good?.Name;
            amount = goodRef.amount;
        }
    }

    public class WorkPlaceData : IJSONConvertsTo<WorkplaceModel>
    {
        public string[] races;
        
        public WorkplaceModel ConvertTo()
        {
            WorkplaceModel workplaceModel = new WorkplaceModel();
            workplaceModel.allowedRaces = races.ToRaceModelArray();
            return workplaceModel;
        }

        public void ConvertFrom(WorkplaceModel value)
        {
            races = value.allowedRaces.Select(a => a?.Name).ToArray();
        }
    }

    public class NeedData : IJSONConvertsTo<NeedModel>
    {
        public string needName;

        public NeedModel ConvertTo()
        {
            return SO.Settings.Needs.FirstOrDefault(a=>a.name == needName);
        }

        public void ConvertFrom(NeedModel value)
        {
            needName = value.name;
        }
    }
    
    [SchemaHelpURL("https://github.com/JamesVeug/AgainstTheStormAPI/blob/master/ATS_API/Scripts/Helpers/Enums/BuildingTagTypes.cs", "https://hoodedhorse.com/wiki/Against_the_Storm/Trading")]
    public class SchemaBuildingTagTypesAttribute : SchemaEnumAttribute<BuildingTagTypes>
    {
        public SchemaBuildingTagTypesAttribute(BuildingTagTypes defaultValue, string description) : base(defaultValue, description)
        {
        }
    }
}