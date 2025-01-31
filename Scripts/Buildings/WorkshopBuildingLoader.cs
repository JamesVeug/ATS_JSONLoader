using System.Collections.Generic;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;

public class WorkshopBuildingLoader : ABuildingLoader<WorkshopModel, WorkshopBuildingData>
{
    public const string fileExtension = "_workshopBuilding.json";
    public override string FileExtension => fileExtension;
    public override IEnumerable<WorkshopModel> AllModels => MB.Settings.workshops;
    public override WorkshopModel CreateNewModelModel(string guid, string name)
    {
        return BuildingManager.CreateWorkshop(guid, name).BuildingModel;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        NewWorkshopBuildingData data = BuildingManager.NewWorkshops.FirstOrDefault(a => a.BuildingModel.name == name);
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

    public override void Apply(WorkshopModel model, WorkshopBuildingData data, bool toModel, string modelName, bool isNewGood)
    {
        ImportExportUtils.SetID(modelName);

        WorkshopBuildingBuilder builder = new WorkshopBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.icon, ref data.icon, toModel, "WorkshopBuildings", "icon");
        ImportExportUtils.ApplyValueNoNull(ref model.order, ref data.order, toModel, "WorkshopBuildings", "order");
        ImportExportUtils.ApplyValueNoNull(ref model.movingCost, ref data.movingCost, toModel, "WorkshopBuildings", "movingCost");
        ImportExportUtils.ApplyValueNoNull(ref model.movable, ref data.movable, toModel, "WorkshopBuildings", "movable");
        ImportExportUtils.ApplyValueNoNull(ref model.profession, ref data.profession, toModel, "WorkshopBuildings", "profession");
        ImportExportUtils.ApplyValueNoNull(ref model.category, ref data.category, toModel, "WorkshopBuildings", "category");
        ImportExportUtils.ApplyValueNoNull(ref model.tags, ref data.tags, toModel, "WorkshopBuildings", "tags");
        ImportExportUtils.ApplyValueNoNull(ref model.usabilityTags, ref data.usabilityTags, toModel, "WorkshopBuildings", "usabilityTags");
        ImportExportUtils.ApplyValueNoNull(ref model.requiredGoods, ref data.requiredGoods, toModel, "WorkshopBuildings", "requiredGoods");
        ImportExportUtils.ApplyValueNoNull(ref model.workplaces, ref data.workplaces, toModel, "WorkshopBuildings", "workplaces");
        ImportExportUtils.ApplyValueNoNull(ref model.recipes, ref data.workshopRecipes, toModel, "WorkshopBuildings", "recipes");
    }
}

[GenerateSchema("WorkshopBuilding", "Buildings that turn basic WorkshopBuildings into products.", WorkshopBuildingLoader.fileExtension)]
public class WorkshopBuildingData : ABuildingData
{
    [SchemaField(0, "The order of which this will be good will be sorted in some lists.")] 
    public int? order;

    [SchemaEnum<BuildingCategoriesTypes>(BuildingCategoriesTypes.Industry, "Column it appears in to enable/disable use by villagers. If not set API will auto set it.")] 
    public string category;
    
    [HelperMethods.SchemaBuildingTagTypes(BuildingTagTypes.Alchemy, "")] 
    public string[] tags;

    [SchemaField(null)] 
    public HelperMethods.GoodRefData movingCost;

    [SchemaField(null)] 
    public bool movable;
    
    [SchemaEnum<ProfessionTypes>(ProfessionTypes.Alchemist, "Which races are good in this building.")]
    public string profession;
    
    [SchemaField(null)] 
    public HelperMethods.GoodRefData[] requiredGoods;
    
    [SchemaField(null)]
    public HelperMethods.WorkPlaceData[] workplaces;
    
    [SchemaField(null)]
    public string[] workshopRecipes;

    [SchemaField(null)]
    public string[] usabilityTags;
}
