using System.Collections.Generic;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;

public class MineBuildingLoader : ABuildingLoader<MineModel, MineBuildingData>
{
    public const string fileExtension = "_MineBuilding.json";
    public override string FileExtension => fileExtension;
    public override string Category => "MineBuilding";
    public override IEnumerable<MineModel> AllModels => MB.Settings.Buildings.Where(a=>a is MineModel).Cast<MineModel>();
    public override bool SupportsNewModels => false;
    
    public override MineModel CreateNewModelModel(string guid, string name)
    {
        return null;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        rawName = null;
        guid = null;
        return false;
    }

    public override void Apply(MineModel model, MineBuildingData data, bool toModel, string modelName, bool isNewGood)
    {
        base.Apply(model, data, toModel, modelName, isNewGood);

        MineBuildingBuilder builder = new MineBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.levels, ref data.levels, toModel, Category, "levels");
        
        ImportExportUtils.ApplyValueNoNull(ref model.maxProductionStorage, ref data.maxProductionStorage, toModel, Category, "maxProductionStorage");
        ImportExportUtils.ApplyValueNoNull(ref model.profession, ref data.profession, toModel, Category, "profession");
        ImportExportUtils.ApplyValueNoNull(ref model.workplaces, ref data.workplaces, toModel, Category, "workplaces");
        ImportExportUtils.ApplyValueNoNull(ref model.recipes, ref data.recipes, toModel, Category, "recipes");
        // ImportExportUtils.ApplyValueNoNull(ref model.prefab, ref data.prefab, toModel, Category, "prefab");
    }
}

[GenerateSchema("MineBuilding", "Buildings that are used to Mine from Mine fields to get resources", MineBuildingLoader.fileExtension)]
public class MineBuildingData : ABuildingData
{
    [SchemaEnum<ProfessionTypes>(ProfessionTypes.Alchemist, "Which races are good in this building.")]
    public string profession;

    [SchemaField(null)]
    public int maxProductionStorage;
    
    [SchemaField(null)]
    public string[] recipes;
    
    [SchemaField(null)]
    public HelperMethods.WorkPlaceData[] workplaces;

    [SchemaField(null)]
    public HelperMethods.BuildingLevelData[] levels;
}

public class MineBuildingBuilder : BuildingBuilder<MineModel, MineData>
{
    public MineBuildingBuilder(MineModel model) : base(model)
    {
    }

    public MineBuildingBuilder(string guid, string name, string iconPath) : base(guid, name, iconPath)
    {
    }

    protected override MineData GetNewData(MineModel model)
    {
        return new MineData("", model.name, BuildingTypes.Ruined_Fishing_Hut, model, BuildingBehaviourTypes.Hearth);
    }

    protected override MineData CreateNewData(string guid, string name)
    {
        return new MineData(guid, name, BuildingTypes.Ruined_Fishing_Hut, null, BuildingBehaviourTypes.Hearth);
    }
}

public class MineData : GenericBuildingData<MineModel>
{
    public MineData(string guid, string name, BuildingTypes id, MineModel model, BuildingBehaviourTypes behaviour) : base(guid, name, id, model, behaviour)
    {
    }
}