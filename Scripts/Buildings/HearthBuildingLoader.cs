using System.Collections.Generic;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;

public class HearthBuildingLoader : ABuildingLoader<HearthModel, HearthBuildingData>
{
    public const string fileExtension = "_HearthBuilding.json";
    public override string FileExtension => fileExtension;
    public override string Category => "HearthBuilding";
    public override IEnumerable<HearthModel> AllModels => MB.Settings.Buildings.Where(a=>a is HearthModel).Cast<HearthModel>();
    public override bool SupportsNewModels => false;
    
    public override HearthModel CreateNewModelModel(string guid, string name)
    {
        return null;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        rawName = null;
        guid = null;
        return false;
    }

    public override void Apply(HearthModel model, HearthBuildingData data, bool toModel, string modelName, bool isNewGood)
    {
        base.Apply(model, data, toModel, modelName, isNewGood);

        HearthBuildingBuilder builder = new HearthBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.profession, ref data.profession, toModel, Category, "profession");
        ImportExportUtils.ApplyValueNoNull(ref model.workplaces, ref data.workplaces, toModel, Category, "workplaces");
        // ImportExportUtils.ApplyValueNoNull(ref model.recipes, ref data.recipes, toModel, Category, "recipes");
        // ImportExportUtils.ApplyValueNoNull(ref model.extraRecipes, ref data.extraRecipes, toModel, Category, "extraRecipes");
        // ImportExportUtils.ApplyValueNoNull(ref model.sacrificeRecipes, ref data.sacrificeRecipes, toModel, Category, "sacrificeRecipes");
        
        ImportExportUtils.ApplyValueNoNull(ref model.maxBurningTime, ref data.maxBurningTime, toModel, Category, "maxBurningTime");
        ImportExportUtils.ApplyValueNoNull(ref model.minBurningTimeToRequestFuel, ref data.minBurningTimeToRequestFuel, toModel, Category, "minBurningTimeToRequestFuel");
        ImportExportUtils.ApplyValueNoNull(ref model.minTimeToShowNoFuel, ref data.minTimeToShowNoFuel, toModel, Category, "minTimeToShowNoFuel");
        ImportExportUtils.ApplyValueNoNull(ref model.initialBurningTime, ref data.initialBurningTime, toModel, Category, "initialBurningTime");
        ImportExportUtils.ApplyValueNoNull(ref model.fuelPriority, ref data.fuelPriority, toModel, Category, "fuelPriority");
        ImportExportUtils.ApplyValueNoNull(ref model.hubRange, ref data.hubRange, toModel, Category, "hubRange");
        ImportExportUtils.ApplyValueNoNull(ref model.extraRecipesUnlockPrice, ref data.extraRecipesUnlockPrice, toModel, Category, "extraRecipesUnlockPrice");
    }
}

[GenerateSchema("HearthBuilding", "Buildings that are used to gather fish from Gatherer resources in the world", HearthBuildingLoader.fileExtension)]
public class HearthBuildingData : ABuildingData
{
    [SchemaEnum<ProfessionTypes>(ProfessionTypes.FireKeeper, "Which races are good in this building.")]
    public string profession;
    
    [SchemaField(null)]
    public HelperMethods.WorkPlaceData[] workplaces;
    
    [SchemaField(null)]
    public float? maxBurningTime;
    
    [SchemaField(null)]
    public float? minBurningTimeToRequestFuel;
    
    [SchemaField(null)]
    public float? minTimeToShowNoFuel;
    
    [SchemaField(null)]
    public float? initialBurningTime;
    
    [SchemaField(null)]
    public int? fuelPriority;
    
    [SchemaField(null)]
    public float? hubRange;
    
    [SchemaField(null)]
    public HelperMethods.GoodRefData extraRecipesUnlockPrice;
}

public class HearthBuildingBuilder : BuildingBuilder<HearthModel, HearthData>
{
    public HearthBuildingBuilder(HearthModel model) : base(model)
    {
    }

    public HearthBuildingBuilder(string guid, string name, string iconPath) : base(guid, name, iconPath)
    {
    }

    protected override HearthData GetNewData(HearthModel model)
    {
        return new HearthData("", model.name, BuildingTypes.Small_Hearth, model, BuildingBehaviourTypes.Hearth);
    }

    protected override HearthData CreateNewData(string guid, string name)
    {
        return new HearthData(guid, name, BuildingTypes.Small_Hearth, null, BuildingBehaviourTypes.Hearth);
    }
}

public class HearthData : GenericBuildingData<HearthModel>
{
    public HearthData(string guid, string name, BuildingTypes id, HearthModel model, BuildingBehaviourTypes behaviour) : base(guid, name, id, model, behaviour)
    {
    }
}