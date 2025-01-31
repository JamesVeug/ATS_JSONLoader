using System.Linq;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;

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
            return name.ToNeedModel();
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
            return name.ToModelTag();
        }

        public string Convert(ModelTag value)
        {
            return value.name;
        }
    }

    public class DecorationTierData : JSONSerializer<string, DecorationTier>, JSONSerializer<DecorationTier, string>
    {
        public DecorationTier Convert(string name)
        {
            return name.ToDecorationTierTypes().ToDecorationTier();
        }

        public string Convert(DecorationTier value)
        {
            return value.name.ToDecorationTierTypes().ToString();
        }
    }

    public class WorkshopRecipeModelData : JSONSerializer<string, WorkshopRecipeModel>, JSONSerializer<WorkshopRecipeModel, string>
    {
        public WorkshopRecipeModel Convert(string name)
        {
            return name.ToWorkshopRecipeModel();
        }

        public string Convert(WorkshopRecipeModel value)
        {
            return value.name;
        }
    }

    public class InstitutionRecipeModelData : JSONSerializer<string, InstitutionRecipeModel>, JSONSerializer<InstitutionRecipeModel, string>
    {
        public InstitutionRecipeModel Convert(string name)
        {
            return name.ToInstitutionRecipeModel();
        }

        public string Convert(InstitutionRecipeModel value)
        {
            return value.name;
        }
    }

    public class InstitutionActiveEffectData : JSONSerializer<InstitutionEffectModel, InstitutionActiveEffectData>, JSONSerializer<InstitutionActiveEffectData, InstitutionEffectModel>
    {
        public int MinWorkers;
        public string Effect;
        
        public InstitutionActiveEffectData Convert(InstitutionEffectModel model)
        {
            InstitutionActiveEffectData data = new InstitutionActiveEffectData();
            data.MinWorkers = model.minWorkers;
            data.Effect = model.effect.name;
            return data;
        }

        public InstitutionEffectModel Convert(InstitutionActiveEffectData value)
        {
            InstitutionEffectModel model = new InstitutionEffectModel();
            model.minWorkers = value.MinWorkers;
            model.effect = value.Effect.ToEffectModel();
            return model;
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