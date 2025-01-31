using System.Collections.Generic;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;

public class InstitutionBuildingLoader : ABuildingLoader<InstitutionModel, InstitutionBuildingData>
{
    public const string kFileExtension = "_institutionBuilding.json";
    public override string FileExtension => kFileExtension;
    public override IEnumerable<InstitutionModel> AllModels => MB.Settings.Buildings.Where(a=>a is InstitutionModel).Cast<InstitutionModel>();
    
    public override InstitutionModel CreateNewModelModel(string guid, string name)
    {
        return BuildingManager.CreateInstitution(guid, name).BuildingModel;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        NewInstitutionBuildingData data = BuildingManager.NewInstitutions.FirstOrDefault(a => a.BuildingModel.name == name);
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

    public override void Apply(InstitutionModel model, InstitutionBuildingData data, bool toModel, string modelName, bool isNewGood)
    {
        ImportExportUtils.SetID(modelName);

        InstitutionBuildingBuilder builder = new InstitutionBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.icon, ref data.icon, toModel, "InstitutionBuilding", "icon");
        ImportExportUtils.ApplyValueNoNull(ref model.order, ref data.order, toModel, "InstitutionBuilding", "order");
        ImportExportUtils.ApplyValueNoNull(ref model.movingCost, ref data.movingCost, toModel, "InstitutionBuilding", "movingCost");
        ImportExportUtils.ApplyValueNoNull(ref model.movable, ref data.movable, toModel, "InstitutionBuilding", "movable");
        ImportExportUtils.ApplyValueNoNull(ref model.category, ref data.category, toModel, "InstitutionBuilding", "category");
        ImportExportUtils.ApplyValueNoNull(ref model.tags, ref data.tags, toModel, "InstitutionBuilding", "tags");
        ImportExportUtils.ApplyValueNoNull(ref model.usabilityTags, ref data.usabilityTags, toModel, "InstitutionBuilding", "usabilityTags");
        ImportExportUtils.ApplyValueNoNull(ref model.requiredGoods, ref data.requiredGoods, toModel, "InstitutionBuilding", "requiredGoods");
        ImportExportUtils.ApplyValueNoNull(ref model.profession, ref data.profession, toModel, "InstitutionBuilding", "profession");
        ImportExportUtils.ApplyValueNoNull(ref model.workplaces, ref data.workplaces, toModel, "InstitutionBuilding", "workplaces");
        ImportExportUtils.ApplyValueNoNull(ref model.recipes, ref data.institutionRecipes, toModel, "InstitutionBuilding", "institutionRecipes");
        ImportExportUtils.ApplyValueNoNull(ref model.activeEffects, ref data.institutionActiveEffects, toModel, "InstitutionBuilding", "institutionActiveEffects");
    }
}

[GenerateSchema("InstitutionBuilding", "A building that requires goods to satisfy needs.", InstitutionBuildingLoader.kFileExtension)]
public class InstitutionBuildingData : ABuildingData
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
    
    [SchemaField(null)]
    public HelperMethods.WorkPlaceData[] workplaces;
    
    [SchemaField(null)]
    public string[] institutionRecipes;
    
    [SchemaEnum<ProfessionTypes>(ProfessionTypes.Alchemist, "Which races are good in this building.")]
    public string profession;

    [SchemaField(null)]
    public HelperMethods.InstitutionActiveEffectData[] institutionActiveEffects;
}
