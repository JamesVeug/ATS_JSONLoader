using System.Collections.Generic;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;

public class DecorationBuildingLoader : ABuildingLoader<DecorationModel, DecorationBuildingData>
{
    public const string kFileExtension = "_decorationBuilding.json";
    public override string FileExtension => kFileExtension;
    public override string Category => "DecorationBuilding";
    public override IEnumerable<DecorationModel> AllModels => MB.Settings.Buildings.Where(a=>a is DecorationModel).Cast<DecorationModel>();
    public override bool SupportsNewModels => true;
    
    public override DecorationModel CreateNewModelModel(string guid, string name)
    {
        return BuildingManager.CreateDecoration(guid, name).BuildingModel;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        NewHouseBuildingData data = BuildingManager.NewHouses.FirstOrDefault(a => a.BuildingModel.name == name);
        if (data == null)
        {
            rawName = null;
            guid = null;
            return false;
        }
        
        rawName = data.Name;
        guid = data.Guid;
        return true;
    }

    public override void Apply(DecorationModel model, DecorationBuildingData data, bool toModel, string modelName, bool isNewGood)
    {
        base.Apply(model, data, toModel, modelName, isNewGood);
        ImportExportUtils.SetID(modelName);

        DecorationBuildingBuilder builder = new DecorationBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.tier, ref data.tier, toModel, Category, "tier");
    }
}

[GenerateSchema("DecorationBuilding", "A decoration building that buffs the hearth. (ie: Anvil, Fox Fence, Harmony Spirit Altar)", HouseBuildingLoader.kFileExtension)]
public class DecorationBuildingData : ABuildingData
{
    [SchemaEnum<DecorationTierTypes>(DecorationTierTypes.Aesthetics, "Used to upgrade the Hearth.")] 
    public string tier;
}
