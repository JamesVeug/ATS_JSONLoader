using System.Collections.Generic;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;

public class HouseBuildingLoader : ABuildingLoader<HouseModel, HouseBuildingData>
{
    public const string kFileExtension = "_houseBuilding.json";
    public override string FileExtension => kFileExtension;
    public override IEnumerable<HouseModel> AllModels => MB.Settings.Buildings.Where(a=>a is HouseModel).Cast<HouseModel>();
    
    public override HouseModel CreateNewModelModel(string guid, string name)
    {
        return BuildingManager.CreateHouse(guid, name).BuildingModel;
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

    public override void Apply(HouseModel model, HouseBuildingData data, bool toModel, string modelName, bool isNewGood)
    {
        ImportExportUtils.SetID(modelName);

        HouseBuildingBuilder builder = new HouseBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.icon, ref data.icon, toModel, "HouseBuildings", "icon");
        ImportExportUtils.ApplyValueNoNull(ref model.order, ref data.order, toModel, "HouseBuildings", "order");
        ImportExportUtils.ApplyValueNoNull(ref model.movingCost, ref data.movingCost, toModel, "HouseBuildings", "movingCost");
        ImportExportUtils.ApplyValueNoNull(ref model.movable, ref data.movable, toModel, "WorkshopBuildings", "movable");
        ImportExportUtils.ApplyValueNoNull(ref model.category, ref data.category, toModel, "HouseBuildings", "category");
        ImportExportUtils.ApplyValueNoNull(ref model.tags, ref data.tags, toModel, "HouseBuildings", "tags");
        ImportExportUtils.ApplyValueNoNull(ref model.usabilityTags, ref data.usabilityTags, toModel, "HouseBuildings", "usabilityTags");
        ImportExportUtils.ApplyValueNoNull(ref model.requiredGoods, ref data.requiredGoods, toModel, "HouseBuildings", "requiredGoods");
        ImportExportUtils.ApplyValueNoNull(ref model.housingRaces, ref data.housingRaces, toModel, "HouseBuildings", "housingRaces");
        ImportExportUtils.ApplyValueNoNull(ref model.housingPlaces, ref data.housingPlaces, toModel, "HouseBuildings", "housingPlaces");
        ImportExportUtils.ApplyValueNoNull(ref model.servedNeeds, ref data.servedNeeds, toModel, "HouseBuildings", "servedNeeds");
    }
}

[GenerateSchema("HouseBuilding", "Buildings that turn basic HouseBuildings into products.", HouseBuildingLoader.kFileExtension)]
public class HouseBuildingData : ABuildingData
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
    public string[] housingRaces;

    [SchemaField(null)]
    public int housingPlaces;

    [SchemaField(null)]
    public string[] servedNeeds;

    [SchemaField(null)]
    public string[] usabilityTags;
}
