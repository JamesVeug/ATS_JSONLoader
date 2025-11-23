using System;
using System.Linq;
using ATS_API.Helpers;
using ATS_JSONLoader;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;
using TinyJson;

public class HelperMethods
{
    public class GoodRefData : JSONSerializer<GoodRef, GoodRefData>, JSONSerializer<GoodRefData, GoodRef>
    {
        public string goodName;
        public int? amount;

        public GoodRef Convert(GoodRefData data)
        {
            GoodRef goodRef = new GoodRef();
            goodRef.good = data.goodName.ToGoodModel();
            goodRef.amount = data.amount.GetValueOrDefault();
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
        [SchemaField(false, false, "Can only an automaton be assigned to this slow?")]
        public bool onlyForAutomatons;
        
        public WorkplaceModel Convert(WorkPlaceData data)
        {
            WorkplaceModel workplaceModel = new WorkplaceModel();
            workplaceModel.onlyForAutomatons = data.onlyForAutomatons;
            return workplaceModel;
        }

        public WorkPlaceData Convert(WorkplaceModel value)
        {
            WorkPlaceData workPlaceData = new WorkPlaceData();
            workPlaceData.onlyForAutomatons = value.onlyForAutomatons;
            return workPlaceData;
        }
    }

    public class BuildingLevelData : JSONSerializer<BuildingLevelModel, BuildingLevelData>, JSONSerializer<BuildingLevelData, BuildingLevelModel>
    {
        [SchemaField(null)]
        public GoodSetData[] requiredGoods;
        
        [SchemaEnum<BuildingPerkTypes>(BuildingPerkTypes.Arch_Inst_Extra_Production, "")] 
        public string[] options;
        
        public BuildingLevelModel Convert(BuildingLevelData data)
        {
            BuildingLevelModel workplaceModel = new BuildingLevelModel();
            workplaceModel.requiredGoods = data.requiredGoods.Select(a=>a.Convert(a)).ToArray();
            workplaceModel.options = data.options.ToBuildingPerkModelArrayNoNulls();
            
            return workplaceModel;
        }

        public BuildingLevelData Convert(BuildingLevelModel value)
        {
            BuildingLevelData workPlaceData = new BuildingLevelData();
            workPlaceData.requiredGoods = value.requiredGoods.Select(a=> new GoodSetData().Convert(a)).ToArray();
            workPlaceData.options = value.options.Select(a => a.name).ToArray();
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
            if (Enum.TryParse(name, out DecorationTierTypes tierTypes) && tierTypes != DecorationTierTypes.None)
            {
                return tierTypes.ToDecorationTier();
            }

            DecorationTier tier = SO.Settings.decorationsTiers.FirstOrDefault(a => a.name == name);
            if (tier == null)
            {
                Plugin.Log.LogError("Decoration tier not found: " + name);
            }
            return tier;
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

    public class BuildingCategoryModelData : JSONSerializer<string, BuildingCategoryModel>, JSONSerializer<BuildingCategoryModel, string>
    {
        public string Convert(BuildingCategoryModel model)
        {
            return model.name;
        }

        public BuildingCategoryModel Convert(string value)
        {
            return value.ToBuildingCategoryModel();
        }
    }

    public class BuildingTagModelData : JSONSerializer<string, BuildingTagModel>, JSONSerializer<BuildingTagModel, string>
    {
        public string Convert(BuildingTagModel model)
        {
            return model.name;
        }

        public BuildingTagModel Convert(string value)
        {
            return value.ToBuildingTagModel();
        }
    }

    public class ProfessionModelData : JSONSerializer<string, ProfessionModel>, JSONSerializer<ProfessionModel, string>
    {
        public string Convert(ProfessionModel model)
        {
            return model.name;
        }

        public ProfessionModel Convert(string value)
        {
            return value.ToProfessionModel();
        }
    }

    public class RaceModelData : JSONSerializer<string, RaceModel>, JSONSerializer<RaceModel, string>
    {
        public string Convert(RaceModel model)
        {
            return model.name;
        }

        public RaceModel Convert(string value)
        {
            return value.ToRaceModel();
        }
    }
    
    [SchemaHelpURL("https://github.com/JamesVeug/AgainstTheStormAPI/blob/master/ATS_API/Scripts/Helpers/Enums/BuildingTagTypes.cs", "https://hoodedhorse.com/wiki/Against_the_Storm/Trading")]
    public class SchemaBuildingTagTypesAttribute : SchemaEnumAttribute<BuildingTagTypes>
    {
        public SchemaBuildingTagTypesAttribute(BuildingTagTypes defaultValue, string description) : base(defaultValue, description)
        {
        }
    }

    public class GoodSetData : IInitializable, JSONSerializer<GoodSetData, GoodsSet>, JSONSerializer<GoodsSet, GoodSetData>
    {
        [SchemaField(null, "Rewards the player can choose when starting a new settlement. If not included then will not affect existing meta rewards.")] 
        public RequiredGood[] goods;

        public GoodsSet Convert(GoodSetData from)
        {
            GoodsSet goodsSet = new GoodsSet();
            goodsSet.goods = from.goods.Select(g => new GoodRef {good = g?.good.ToGoodModel(), amount = g.amount}).ToArray();
            return goodsSet;
        }

        public GoodSetData Convert(GoodsSet from)
        {
            GoodSetData goodSetData = new GoodSetData();
            goodSetData.goods = from.goods.Select(g => new RequiredGood {good = g.good?.Name, amount = g.amount}).ToArray();
            return goodSetData;
        }

        public void Initialize()
        {
        
        }
    }

    public class GoodRefChanceData : IInitializable, JSONSerializer<GoodRefChance, GoodRefChanceData>, JSONSerializer<GoodRefChanceData, GoodRefChance>
    {
        [SchemaEnum<GoodsTypes>(GoodsTypes.Food_Raw_Herbs, "The good given")] 
        public string good;

        [SchemaField(0, "The amount of the good produced")]
        public int? amount;

        [SchemaField(0, "The chance of the good being produced. 0 never, 0.5 is 50% of the time, 1 is always")]
        public float? chance;

        public GoodRefChance Convert(GoodRefChanceData from)
        {
            GoodRefChance goodRefChance = new GoodRefChance();
            goodRefChance.good = from?.good?.ToGoodModel();
            goodRefChance.amount = from?.amount ?? 0;
            goodRefChance.chance = from?.chance ?? 0;
            return goodRefChance;
        }

        public GoodRefChanceData Convert(GoodRefChance from)
        {
            GoodRefChanceData goodRefChanceData = new GoodRefChanceData();
            goodRefChanceData.good = from?.good?.Name;
            goodRefChanceData.amount = from?.amount;
            goodRefChanceData.chance = from?.chance;
            return goodRefChanceData;
        }

        public void Initialize()
        {
        
        }
    }
    
    public class RequiredGood : IInitializable
    {
        [SchemaEnum<GoodsTypes>(GoodsTypes.Food_Raw_Herbs, "The good required")]
        public string good;
    
        [SchemaField(0, "The amount of the good required")]
        public int amount;

        public void Initialize()
        {
        
        }
    }
    
    public class WaterData : JSONSerializer<string, WaterModel>, JSONSerializer<WaterModel, string>
    {
        public WaterModel Convert(string from)
        {
            return from.ToWaterModel();
        }

        public string Convert(WaterModel from)
        {
            return from.name;
        }
    }
}