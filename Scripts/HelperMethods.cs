using System.Linq;
using ATS_API.Helpers;
using ATS_API.Recipes;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;
using UnityEngine;

public class HelperMethods
{
    public class GoodRefData : JSONSerializer<GoodRef, GoodRefData>, JSONSerializer<GoodRefData, GoodRef>
    {
        public string goodName;
        public int amount;

        public GoodRef Convert(GoodRefData data)
        {
            GoodRef goodRef = new GoodRef();
            goodRef.good = data.goodName.ToGoodModel();
            goodRef.amount = data.amount;
            return goodRef;
        }
        
        public GoodRefData Convert(GoodRef goodRef)
        {
            GoodRefData goodRefData = new GoodRefData();
            goodRefData.goodName = goodRef.good?.Name;
            goodRefData.amount = goodRef.amount;
            return goodRefData;
        }
    }

    public class WorkPlaceData : JSONSerializer<WorkplaceModel, WorkPlaceData>, JSONSerializer<WorkPlaceData, WorkplaceModel>
    {
        public string[] races;
        
        public WorkplaceModel Convert(WorkPlaceData data)
        {
            WorkplaceModel workplaceModel = new WorkplaceModel();
            workplaceModel.allowedRaces = data.races.ToRaceModelArray();
            return workplaceModel;
        }

        public WorkPlaceData Convert(WorkplaceModel value)
        {
            WorkPlaceData workPlaceData = new WorkPlaceData();
            workPlaceData.races = value.allowedRaces.Select(a => a?.Name).ToArray();
            return workPlaceData;
        }
    }

    public class NeedData : JSONSerializer<string, NeedModel>, JSONSerializer<NeedModel, string>
    {
        public NeedModel Convert(string name)
        {
            return SO.Settings.Needs.FirstOrDefault(a=>a.name == name);
        }

        public string Convert(NeedModel value)
        {
            return value.name;
        }
    }

    public class TagData : JSONSerializer<string, ModelTag>, JSONSerializer<ModelTag, string>
    {
        public ModelTag Convert(string name)
        {
            return SO.Settings.tags.FirstOrDefault(a=>a.name == name);
        }

        public string Convert(ModelTag value)
        {
            return value.name;
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