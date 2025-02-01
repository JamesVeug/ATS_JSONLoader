using System.Collections.Generic;
using System.Linq;
using ATS_API.Helpers;
using ATS_API.Recipes;
using ATS_JSONLoader;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;

public class CampRecipeLoader : ARecipeLoader<CampRecipeModel, CampRecipeData>
{
    public const string fileExtension = "_CampRecipe.json";
    public override string FileExtension => fileExtension;
    public override string Category => "CampRecipes";
    public override IEnumerable<CampRecipeModel> AllModels => MB.Settings.campsRecipes;
    public override CampRecipeModel CreateNewModel(string guid, string name)
    {
        return (CampRecipeModel)RecipeManager.CreateRecipe<CampRecipeModel>(guid, name).RecipeModel;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        NewCampRecipeData newData = RecipeManager.NewCampRecipes.FirstOrDefault(a => a.Name == name);
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

    public override void Apply(CampRecipeModel model, CampRecipeData data, bool toModel, string modelName)
    {
        base.Apply(model, data, toModel, modelName);

        Logging.VerboseLog($"Applying JSON (CampRecipes) {modelName}");
        ImportExportUtils.ApplyValueNoNull(ref model.refGood, ref data.refGood, toModel, Category, "refGood");
        ImportExportUtils.ApplyValueNoNull(ref model.productionTime, ref data.productionTime, toModel, Category, "productionTime");
    }
}

[GenerateSchema("Camp Recipe", "Recipes added to Camps (aka: Service buildings) to needs.", CampRecipeLoader.fileExtension)]
public class CampRecipeData : ARecipeData
{
    [SchemaField(null, "The good that is produced by the recipe.")]
    public HelperMethods.GoodRefData refGood;
    
    [SchemaField(0.0f, "The time it takes to produce the good.")]
    public float? productionTime;

}