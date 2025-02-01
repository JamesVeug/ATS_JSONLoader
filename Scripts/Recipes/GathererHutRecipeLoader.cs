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

public class GathererHutRecipeLoader : ARecipeLoader<GathererHutRecipeModel, GathererHutRecipeData>
{
    public const string fileExtension = "_GathererHutRecipe.json";
    public override string FileExtension => fileExtension;
    public override string Category => "GathererHutRecipes";
    public override IEnumerable<GathererHutRecipeModel> AllModels => MB.Settings.gatherersHutsRecipes;
    public override GathererHutRecipeModel CreateNewModel(string guid, string name)
    {
        return (GathererHutRecipeModel)RecipeManager.CreateRecipe<GathererHutRecipeModel>(guid, name).RecipeModel;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        NewGathererHutRecipeData newData = RecipeManager.NewGathererHutRecipes.FirstOrDefault(a => a.Name == name);
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

    public override void Apply(GathererHutRecipeModel model, GathererHutRecipeData data, bool toModel, string modelName)
    {
        base.Apply(model, data, toModel, modelName);
        
        GathererHutRecipeBuilder builder = new GathererHutRecipeBuilder(model);

        Logging.VerboseLog($"Applying JSON (GathererHutRecipes) {modelName}");
        ImportExportUtils.ApplyValueNoNull(ref model.refGood, ref data.refGood, toModel, Category, "refGood");
        ImportExportUtils.ApplyValueNoNull(ref model.productionTime, ref data.productionTime, toModel, Category, "productionTime");
        ImportExportUtils.ApplyLocaText(ref model.gradeDesc, ref data.gradeDesc, (a, b) => builder.SetGradeDesc(a, b), false, "gradeDesc");
    }
}

[GenerateSchema("GathererHut Recipe", "Recipes added to GathererHut buildings to produce goods.", GathererHutRecipeLoader.fileExtension)]
public class GathererHutRecipeData : ARecipeData
{
    [SchemaField(null, "The good that is produced by the recipe.")]
    public HelperMethods.GoodRefData refGood;
    
    [SchemaField(10f, "The time it takes to produce the good in seconds")] 
    public float? productionTime;
    
    [SchemaLocalized("Description of the recipes grade")] 
    public LocalizableField gradeDesc;
}