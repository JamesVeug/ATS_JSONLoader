using System.Collections.Generic;
using System.Linq;
using ATS_API.Helpers;
using ATS_API.Recipes;
using ATS_JSONLoader;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;

public class MineRecipeLoader : ARecipeLoader<MineRecipeModel, MineRecipeData>
{
    public const string fileExtension = "_MineRecipe.json";
    public override string FileExtension => fileExtension;
    public override string Category => "MineRecipes";
    public override IEnumerable<MineRecipeModel> AllModels => MB.Settings.minesRecipes;
    public override MineRecipeModel CreateNewModel(string guid, string name)
    {
        return (MineRecipeModel)RecipeManager.CreateRecipe<MineRecipeModel>(guid, name).RecipeModel;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        NewMineRecipeData newData = RecipeManager.NewMineRecipes.FirstOrDefault(a => a.Name == name);
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

    public override void Apply(MineRecipeModel model, MineRecipeData data, bool toModel, string modelName)
    {
        base.Apply(model, data, toModel, modelName);

        Logging.VerboseLog($"Applying JSON (MineRecipes) {modelName}");
        ImportExportUtils.ApplyValueNoNull(ref model.producedGood, ref data.producedGood, toModel, Category, "producedGood");
        ImportExportUtils.ApplyValueNoNull(ref model.productionTime, ref data.productionTime, toModel, Category, "productionTime");
        ImportExportUtils.ApplyValueNoNull(ref model.extraProduction, ref data.extraProduction, toModel, Category, "extraProduction");
        
        if (toModel)
        {
            if (data.buildings != null)
            {
                foreach (var buildingModel in SO.Settings.mines)
                {
                    bool shouldContainRecipe = data.buildings.Contains(buildingModel.name);
                    if (shouldContainRecipe && !buildingModel.recipes.Contains(model))
                    {
                        buildingModel.recipes = buildingModel.recipes.ForceAdd(model);
                    }
                    else if (!shouldContainRecipe && buildingModel.recipes.Contains(model))
                    {
                        buildingModel.recipes = buildingModel.recipes.Where(a=>a != model).ToArray();
                    }
                }
            }
        }
        else
        {
            data.buildings = SO.Settings.mines.Where(a=>a.recipes.Contains(model)).Select(a=>a.name).ToArray();
        }
    }
}

[GenerateSchema("Mine Recipe", "Recipes added to Mine buildings to produce goods.", MineRecipeLoader.fileExtension)]
public class MineRecipeData : ARecipeData
{
    [SchemaField(null, "The good that is produced by the recipe.")]
    public HelperMethods.GoodRefData producedGood;
    
    [SchemaField(10f, "The time it takes to produce the good in seconds")] 
    public float? productionTime;
    
    [SchemaField(null, "The amount of the good that is also produced by the recipe.")] 
    public HelperMethods.GoodRefChanceData[] extraProduction;

    [SchemaEnum<MineTypes>(MineTypes.Mine, "Which mines can use this recipe")] 
    public string[] buildings;
}