using System.Collections.Generic;
using System.Linq;
using ATS_API.Helpers;
using ATS_API.Recipes;
using ATS_API.Recipes.Builders;
using ATS_JSONLoader;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;
using TinyJson;

public class FishingHutRecipeLoader : ARecipeLoader<FishingHutRecipeModel, FishingHutRecipeData>
{
    public const string fileExtension = "_FishingHutRecipe.json";
    public override string FileExtension => fileExtension;
    public override string Category => "FishingHutRecipes";
    public override IEnumerable<FishingHutRecipeModel> AllModels => MB.Settings.fishingHutsRecipes;
    public override FishingHutRecipeModel CreateNewModel(string guid, string name)
    {
        return (FishingHutRecipeModel)RecipeManager.CreateRecipe<FishingHutRecipeModel>(guid, name).RecipeModel;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        NewFishingHutRecipeData newData = RecipeManager.NewFishingHutRecipes.FirstOrDefault(a => a.Name == name);
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

    public override void Apply(FishingHutRecipeModel model, FishingHutRecipeData data, bool toModel, string modelName)
    {
        base.Apply(model, data, toModel, modelName);
        
        FishingHutRecipeBuilder builder = new FishingHutRecipeBuilder(model);

        Logging.VerboseLog($"Applying JSON (FishingHutRecipes) {modelName}");
        ImportExportUtils.ApplyValueNoNull(ref model.refGood, ref data.refGood, toModel, Category, "refGood");
        ImportExportUtils.ApplyValueNoNull(ref model.productionTime, ref data.productionTime, toModel, Category, "productionTime");
        ImportExportUtils.ApplyLocaText(ref model.gradeDesc, ref data.gradeDesc, (a, b) => builder.SetGradeDesc(a, b), false, "gradeDesc");
    }
}

[GenerateSchema("FishingHut Recipe", "Recipes added to FishingHuts", FishingHutRecipeLoader.fileExtension)]
public class FishingHutRecipeData : ARecipeData
{
    [SchemaField(null, "The good that is produced by the recipe.")]
    public HelperMethods.GoodRefData refGood;
    
    [SchemaField(0.0f, "The time it takes to produce the good.")]
    public float? productionTime;
    
    [SchemaLocalized("The grade description of the recipe.")] 
    public LocalizableField gradeDesc;
}