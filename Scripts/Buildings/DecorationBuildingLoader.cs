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
    public override IEnumerable<DecorationModel> AllModels => MB.Settings.Buildings.Where(a=>a is DecorationModel).Cast<DecorationModel>();
    
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
        ImportExportUtils.SetID(modelName);

        DecorationBuildingBuilder builder = new DecorationBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.icon, ref data.icon, toModel, "DecorationBuilding", "icon");
        ImportExportUtils.ApplyValueNoNull(ref model.order, ref data.order, toModel, "DecorationBuilding", "order");
        ImportExportUtils.ApplyValueNoNull(ref model.movingCost, ref data.movingCost, toModel, "DecorationBuilding", "movingCost");
        ImportExportUtils.ApplyValueNoNull(ref model.movable, ref data.movable, toModel, "DecorationBuilding", "movable");
        ImportExportUtils.ApplyValueNoNull(ref model.category, ref data.category, toModel, "DecorationBuilding", "category");
        ImportExportUtils.ApplyValueNoNull(ref model.tags, ref data.tags, toModel, "DecorationBuilding", "tags");
        ImportExportUtils.ApplyValueNoNull(ref model.usabilityTags, ref data.usabilityTags, toModel, "DecorationBuilding", "usabilityTags");
        ImportExportUtils.ApplyValueNoNull(ref model.requiredGoods, ref data.requiredGoods, toModel, "DecorationBuilding", "requiredGoods");
        ImportExportUtils.ApplyValueNoNull(ref model.tier, ref data.tier, toModel, "DecorationBuilding", "tier");
    }
}

[GenerateSchema("DecorationBuilding", "A decoration building that buffs the hearth.", HouseBuildingLoader.kFileExtension)]
public class DecorationBuildingData : ABuildingData
{
    [SchemaField(0, "The order of which this will be good will be sorted in some lists.")] 
    public int? order;

    [SchemaEnum<BuildingCategoriesTypes>(BuildingCategoriesTypes.Housing, "Column it appears in to enable/disable use by villagers. If not set API will auto set it.")] 
    public string category;
    
    [HelperMethods.SchemaBuildingTagTypes(BuildingTagTypes.Alchemy, "")] 
    public string[] tags;

    [SchemaField(null)] 
    public HelperMethods.GoodRefData movingCost;

    [SchemaField(null)]
    public bool movable;
    
    [SchemaField(null)] 
    public HelperMethods.GoodRefData[] requiredGoods;

    [SchemaField(null)]
    public string[] usabilityTags;

    [SchemaEnum<DecorationTierTypes>(DecorationTierTypes.Aesthetics, "Used to upgrade the Hearth.")] 
    public string tier;
}
