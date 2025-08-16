using System.Collections.Generic;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;

public class CampBuildingLoader : ABuildingLoader<CampModel, CampBuildingData>
{
    public const string fileExtension = "_CampBuilding.json";
    public override string FileExtension => fileExtension;
    public override string Category => "CampBuilding";
    public override IEnumerable<CampModel> AllModels => MB.Settings.Buildings.Where(a=>a is CampModel).Cast<CampModel>();
    public override bool SupportsNewModels => false;
    
    public override CampModel CreateNewModelModel(string guid, string name)
    {
        return null;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        rawName = null;
        guid = null;
        return false;
    }

    public override void Apply(CampModel model, CampBuildingData data, bool toModel, string modelName, bool isNewGood)
    {
        base.Apply(model, data, toModel, modelName, isNewGood);

        CampBuildingBuilder builder = new CampBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.profession, ref data.profession, toModel, Category, "profession");
        ImportExportUtils.ApplyValueNoNull(ref model.workplaces, ref data.workplaces, toModel, Category, "workplaces");
        ImportExportUtils.ApplyValueNoNull(ref model.recipes, ref data.recipes, toModel, Category, "recipes");
        
        ImportExportUtils.ApplyValueNoNull(ref model.maxDistance, ref data.maxDistance, toModel, Category, "maxDistance");
        ImportExportUtils.ApplyValueNoNull(ref model.maxStorage, ref data.maxStorage, toModel, Category, "maxStorage");
    }
}

[GenerateSchema("CampBuilding", "Buildings that are used to gather fish from Gatherer resources in the world", CampBuildingLoader.fileExtension)]
public class CampBuildingData : ABuildingData
{
    [SchemaEnum<ProfessionTypes>(ProfessionTypes.Alchemist, "Which races are good in this building.")]
    public string profession;
    
    [SchemaField(null)]
    public HelperMethods.WorkPlaceData[] workplaces;
    
    [SchemaField(null)]
    public string[] recipes;

    [SchemaField(null)]
    public float maxStorage;

    [SchemaField(null)]
    public float maxDistance;
}

public class CampBuildingBuilder : BuildingBuilder<CampModel, CampData>
{
    public CampBuildingBuilder(CampModel model) : base(model)
    {
    }

    public CampBuildingBuilder(string guid, string name, string iconPath) : base(guid, name, iconPath)
    {
    }

    protected override CampData GetNewData(CampModel model)
    {
        return new CampData("", model.name, BuildingTypes.Ruined_Fishing_Hut, model, BuildingBehaviourTypes.Hearth);
    }

    protected override CampData CreateNewData(string guid, string name)
    {
        return new CampData(guid, name, BuildingTypes.Ruined_Fishing_Hut, null, BuildingBehaviourTypes.Hearth);
    }
}

public class CampData : GenericBuildingData<CampModel>
{
    public CampData(string guid, string name, BuildingTypes id, CampModel model, BuildingBehaviourTypes behaviour) : base(guid, name, id, model, behaviour)
    {
    }
}