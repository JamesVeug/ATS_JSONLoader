using System.Collections.Generic;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;

public class InstitutionBuildingLoader : ABuildingLoader<InstitutionModel, InstitutionBuildingData>
{
    public const string kFileExtension = "_institutionBuilding.json";
    public override string Category => "InstitutionBuilding";
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
        base.Apply(model, data, toModel, modelName, isNewGood);

        InstitutionBuildingBuilder builder = new InstitutionBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.workplaces, ref data.workplaces, toModel, Category, "workplaces");
        ImportExportUtils.ApplyValueNoNull(ref model.recipes, ref data.institutionRecipes, toModel, Category, "institutionRecipes");
        ImportExportUtils.ApplyValueNoNull(ref model.profession, ref data.profession, toModel, Category, "profession");
        ImportExportUtils.ApplyValueNoNull(ref model.activeEffects, ref data.institutionActiveEffects, toModel, Category, "institutionActiveEffects");
    }
}

[GenerateSchema("InstitutionBuilding", "A building that requires goods to satisfy needs.", InstitutionBuildingLoader.kFileExtension)]
public class InstitutionBuildingData : ABuildingData
{
    [SchemaField(null)]
    public HelperMethods.WorkPlaceData[] workplaces;
    
    [SchemaField(null)]
    public string[] institutionRecipes;
    
    [SchemaEnum<ProfessionTypes>(ProfessionTypes.Alchemist, "Which races are good in this building.")]
    public string profession;

    [SchemaField(null)]
    public HelperMethods.InstitutionActiveEffectData[] institutionActiveEffects;
}
