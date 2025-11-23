using System.Collections.Generic;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;

public class FishingHutBuildingLoader : ABuildingLoader<FishingHutModel, FishingHutBuildingData>
{
    public const string fileExtension = "_FishingHutBuilding.json";
    public override string FileExtension => fileExtension;
    public override string Category => "FishingHutBuilding";
    public override IEnumerable<FishingHutModel> AllModels => MB.Settings.Buildings.Where(a=>a is FishingHutModel).Cast<FishingHutModel>();
    public override bool SupportsNewModels => false;
    
    public override FishingHutModel CreateNewModelModel(string guid, string name)
    {
        return null;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        rawName = null;
        guid = null;
        return false;
    }

    public override void Apply(FishingHutModel model, FishingHutBuildingData data, bool toModel, string modelName, bool isNewGood)
    {
        base.Apply(model, data, toModel, modelName, isNewGood);

        FishingHubBuildingBuilder builder = new FishingHubBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.levels, ref data.levels, toModel, Category, "levels");
        
        ImportExportUtils.ApplyValueNoNull(ref model.profession, ref data.profession, toModel, Category, "profession");
        ImportExportUtils.ApplyValueNoNull(ref model.workplaces, ref data.workplaces, toModel, Category, "workplaces");
        ImportExportUtils.ApplyValueNoNull(ref model.recipes, ref data.recipes, toModel, Category, "recipes");
        
        ImportExportUtils.ApplyValueNoNull(ref model.maxDistance, ref data.maxDistance, toModel, Category, "maxDistance");
        ImportExportUtils.ApplyValueNoNull(ref model.baitIngredient, ref data.baitIngredient, toModel, Category, "baitIngredient");
        ImportExportUtils.ApplyValueNoNull(ref model.bait, ref data.bait, toModel, Category, "bait");
        ImportExportUtils.ApplyValueNoNull(ref model.baitMultiplier, ref data.baitMultiplier, toModel, Category, "baitMultiplier");
    }
}

[GenerateSchema("FishingHutBuilding", "Buildings that are used to gather fish from Fishing resources in the world", FishingHutBuildingLoader.fileExtension)]
public class FishingHutBuildingData : ABuildingData
{
    [SchemaEnum<ProfessionTypes>(ProfessionTypes.Alchemist, "Which races are good in this building.")]
    public string profession;
    
    [SchemaField(null)]
    public HelperMethods.WorkPlaceData[] workplaces;
    
    [SchemaField(null)]
    public string[] recipes;

    [SchemaField(null)]
    public float? baitMultiplier;

    [SchemaField(null)]
    public float? maxDistance;
    
    [SchemaField(null)]
    public HelperMethods.GoodRefData baitIngredient;
    
    [SchemaField(null)]
    public HelperMethods.GoodRefData bait;

    [SchemaField(null)]
    public HelperMethods.BuildingLevelData[] levels;
}

public class FishingHubBuildingBuilder : BuildingBuilder<FishingHutModel, FishingHutData>
{
    public FishingHubBuildingBuilder(FishingHutModel model) : base(model)
    {
    }

    public FishingHubBuildingBuilder(string guid, string name, string iconPath) : base(guid, name, iconPath)
    {
    }

    protected override FishingHutData GetNewData(FishingHutModel model)
    {
        return new FishingHutData("", model.name, BuildingTypes.Ruined_Fishing_Hut, model, BuildingBehaviourTypes.Hearth);
    }

    protected override FishingHutData CreateNewData(string guid, string name)
    {
        return new FishingHutData(guid, name, BuildingTypes.Ruined_Fishing_Hut, null, BuildingBehaviourTypes.Hearth);
    }
}

public class FishingHutData : GenericBuildingData<FishingHutModel>
{
    public FishingHutData(string guid, string name, BuildingTypes id, FishingHutModel model, BuildingBehaviourTypes behaviour) : base(guid, name, id, model, behaviour)
    {
    }
}