using System.Collections.Generic;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;

public class GathererHutBuildingLoader : ABuildingLoader<GathererHutModel, GathererHutBuildingData>
{
    public const string fileExtension = "_GathererHutBuilding.json";
    public override string FileExtension => fileExtension;
    public override string Category => "GathererHutBuilding";
    public override IEnumerable<GathererHutModel> AllModels => MB.Settings.Buildings.Where(a=>a is GathererHutModel).Cast<GathererHutModel>();
    public override bool SupportsNewModels => false;
    
    public override GathererHutModel CreateNewModelModel(string guid, string name)
    {
        return null;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        rawName = null;
        guid = null;
        return false;
    }

    public override void Apply(GathererHutModel model, GathererHutBuildingData data, bool toModel, string modelName, bool isNewGood)
    {
        base.Apply(model, data, toModel, modelName, isNewGood);

        GathererHubBuildingBuilder builder = new GathererHubBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.profession, ref data.profession, toModel, Category, "profession");
        ImportExportUtils.ApplyValueNoNull(ref model.workplaces, ref data.workplaces, toModel, Category, "workplaces");
        ImportExportUtils.ApplyValueNoNull(ref model.recipes, ref data.recipes, toModel, Category, "recipes");
        
        ImportExportUtils.ApplyValueNoNull(ref model.maxDistance, ref data.maxDistance, toModel, Category, "maxDistance");
        ImportExportUtils.ApplyValueNoNull(ref model.maxStorage, ref data.maxStorage, toModel, Category, "maxStorage");
    }
}

[GenerateSchema("GathererHutBuilding", "Buildings that are used to gather fish from Gatherer resources in the world", GathererHutBuildingLoader.fileExtension)]
public class GathererHutBuildingData : ABuildingData
{
    [SchemaEnum<ProfessionTypes>(ProfessionTypes.Alchemist, "Which races are good in this building.")]
    public string profession;
    
    [SchemaField(null)]
    public HelperMethods.WorkPlaceData[] workplaces;
    
    [SchemaField(null)]
    public string[] recipes;

    [SchemaField(null)]
    public float? maxStorage;

    [SchemaField(null)]
    public float? maxDistance;
}

public class GathererHubBuildingBuilder : BuildingBuilder<GathererHutModel, GathererHutData>
{
    public GathererHubBuildingBuilder(GathererHutModel model) : base(model)
    {
    }

    public GathererHubBuildingBuilder(string guid, string name, string iconPath) : base(guid, name, iconPath)
    {
    }

    protected override GathererHutData GetNewData(GathererHutModel model)
    {
        return new GathererHutData("", model.name, BuildingTypes.Ruined_Fishing_Hut, model, BuildingBehaviourTypes.Hearth);
    }

    protected override GathererHutData CreateNewData(string guid, string name)
    {
        return new GathererHutData(guid, name, BuildingTypes.Ruined_Fishing_Hut, null, BuildingBehaviourTypes.Hearth);
    }
}

public class GathererHutData : GenericBuildingData<GathererHutModel>
{
    public GathererHutData(string guid, string name, BuildingTypes id, GathererHutModel model, BuildingBehaviourTypes behaviour) : base(guid, name, id, model, behaviour)
    {
    }
}