using System.Collections.Generic;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;

public class FarmBuildingLoader : ABuildingLoader<FarmModel, FarmBuildingData>
{
    public const string fileExtension = "_FarmBuilding.json";
    public override string FileExtension => fileExtension;
    public override string Category => "FarmBuilding";
    public override IEnumerable<FarmModel> AllModels => MB.Settings.Buildings.Where(a=>a is FarmModel).Cast<FarmModel>();
    public override bool SupportsNewModels => false;
    
    public override FarmModel CreateNewModelModel(string guid, string name)
    {
        return null;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        rawName = null;
        guid = null;
        return false;
    }

    public override void Apply(FarmModel model, FarmBuildingData data, bool toModel, string modelName, bool isNewGood)
    {
        base.Apply(model, data, toModel, modelName, isNewGood);

        FarmBuildingBuilder builder = new FarmBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.levels, ref data.levels, toModel, Category, "levels");
        
        ImportExportUtils.ApplyValueNoNull(ref model.maxStorage, ref data.maxStorage, toModel, Category, "maxStorage");
        ImportExportUtils.ApplyVector2Int(ref model.workArea, ref data.minWorkArea, ref data.maxWorkArea, toModel, Category, "workArea");
        ImportExportUtils.ApplyValueNoNull(ref model.profession, ref data.profession, toModel, Category, "profession");
        // ImportExportUtils.ApplyValueNoNull(ref model.farmfieldModel, ref data.farmfieldModel, toModel, Category, "farmfieldModel");
        ImportExportUtils.ApplyValueNoNull(ref model.plowingTime, ref data.plowingTime, toModel, Category, "plowingTime");
        ImportExportUtils.ApplyValueNoNull(ref model.plowingExtraProductionChance, ref data.plowingExtraProductionChance, toModel, Category, "plowingExtraProductionChance");
        ImportExportUtils.ApplyValueNoNull(ref model.workplaces, ref data.workplaces, toModel, Category, "workplaces");
        ImportExportUtils.ApplyValueNoNull(ref model.recipes, ref data.recipes, toModel, Category, "recipes");
        // ImportExportUtils.ApplyValueNoNull(ref model.prefab, ref data.prefab, toModel, Category, "prefab");
    }
}

[GenerateSchema("FarmBuilding", "Buildings that are used to farm from farm fields to get resources", FarmBuildingLoader.fileExtension)]
public class FarmBuildingData : ABuildingData
{
    [SchemaEnum<ProfessionTypes>(ProfessionTypes.Alchemist, "Which races are good in this building.")]
    public string profession;

    [SchemaField(null)]
    public int maxStorage;
    
    [SchemaField(0, false, "Minimum area the farm reaches")]
    public int minWorkArea;
    
    [SchemaField(0, false, "Maximum area the farm reaches")]
    public int maxWorkArea;
    
    [SchemaField(null)]
    public float plowingTime;
    
    [SchemaField(null)]
    public float plowingExtraProductionChance;
    
    [SchemaField(null)]
    public string[] recipes;
    
    [SchemaField(null)]
    public HelperMethods.WorkPlaceData[] workplaces;

    [SchemaField(null)]
    public HelperMethods.BuildingLevelData[] levels;
}

public class FarmBuildingBuilder : BuildingBuilder<FarmModel, FarmData>
{
    public FarmBuildingBuilder(FarmModel model) : base(model)
    {
    }

    public FarmBuildingBuilder(string guid, string name, string iconPath) : base(guid, name, iconPath)
    {
    }

    protected override FarmData GetNewData(FarmModel model)
    {
        return new FarmData("", model.name, BuildingTypes.Ruined_Fishing_Hut, model, BuildingBehaviourTypes.Hearth);
    }

    protected override FarmData CreateNewData(string guid, string name)
    {
        return new FarmData(guid, name, BuildingTypes.Ruined_Fishing_Hut, null, BuildingBehaviourTypes.Hearth);
    }
}

public class FarmData : GenericBuildingData<FarmModel>
{
    public FarmData(string guid, string name, BuildingTypes id, FarmModel model, BuildingBehaviourTypes behaviour) : base(guid, name, id, model, behaviour)
    {
    }
}