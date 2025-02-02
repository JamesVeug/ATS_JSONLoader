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
    public override string Category => "WorkshopBuilding";
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
        base.Apply(model, data, toModel, modelName, isNewGood);

        WorkshopBuildingBuilder builder = new WorkshopBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.profession, ref data.profession, toModel, Category, "profession");
        ImportExportUtils.ApplyValueNoNull(ref model.workplaces, ref data.workplaces, toModel, Category, "workplaces");
        ImportExportUtils.ApplyValueNoNull(ref model.recipes, ref data.workshopRecipes, toModel, Category, "recipes");
    }
}

[GenerateSchema("WorkshopBuilding", "Buildings that turn goods into products. (ie: Bakery, Toolshop)", WorkshopBuildingLoader.fileExtension)]
public class WorkshopBuildingData : ABuildingData
{
    [SchemaEnum<ProfessionTypes>(ProfessionTypes.Alchemist, "Which races are good in this building.")]
    public string profession;
    
    [SchemaField(null)]
    public HelperMethods.WorkPlaceData[] workplaces;
    
    [SchemaField(null)]
    public string[] workshopRecipes;
}
