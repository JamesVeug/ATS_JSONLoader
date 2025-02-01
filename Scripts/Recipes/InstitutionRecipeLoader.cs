using System.Collections.Generic;
using System.Linq;
using ATS_API.Helpers;
using ATS_API.Recipes;
using ATS_JSONLoader;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;

public class InstitutionRecipeLoader : ARecipeLoader<InstitutionRecipeModel, InstitutionRecipeData>
{
    public const string fileExtension = "_InstitutionRecipe.json";
    public override string FileExtension => fileExtension;
    public override string Category => "InstitutionRecipes";
    public override IEnumerable<InstitutionRecipeModel> AllModels => MB.Settings.institutionRecipes;
    public override InstitutionRecipeModel CreateNewModel(string guid, string name)
    {
        return (InstitutionRecipeModel)RecipeManager.CreateRecipe<InstitutionRecipeModel>(guid, name).RecipeModel;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        NewInstitutionRecipeData newData = RecipeManager.NewInstitutionRecipes.FirstOrDefault(a => a.Name == name);
        if (newData != null)
        {
            rawName = newData.Name;
            guid = newData.Guid;
            return true;
        }
        
        rawName = null;
        guid = null;
        return false;
    }

    public override void Apply(InstitutionRecipeModel model, InstitutionRecipeData data, bool toModel, string modelName)
    {
        base.Apply(model, data, toModel, modelName);

        Logging.VerboseLog($"Applying JSON (InstitutionRecipes) {modelName}");
        ImportExportUtils.ApplyValueNoNull(ref model.servedNeed, ref data.servedNeed, toModel, Category, "servedNeed");
        ImportExportUtils.ApplyValueNoNull(ref model.isGoodConsumed, ref data.isGoodConsumed, toModel, Category, "isGoodConsumed");
        ImportExportUtils.ApplyValueNoNull(ref model.requiredGoods, ref data.requiredGoods, toModel, Category, "requiredGoods");
    }
}

[GenerateSchema("Institution Recipe", "Recipes added to Institutions (aka: Service buildings) to needs.", InstitutionRecipeLoader.fileExtension)]
public class InstitutionRecipeData : ARecipeData
{
    [SchemaEnum<NeedTypes>(NeedTypes.None, "The need that is served by this recipe. If not included then will not affect existing meta rewards.")]
    public string servedNeed;
    
    [SchemaField(null, "Are the goods consumed when the need is served?")]
    public bool? isGoodConsumed;
    
    [SchemaField(null, "Rewards the player can choose when starting a new settlement. If not included then will not affect existing meta rewards.")] 
    public HelperMethods.GoodSetData requiredGoods;

}