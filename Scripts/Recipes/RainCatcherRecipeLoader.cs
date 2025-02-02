using System.Collections.Generic;
using System.Linq;
using ATS_API.Helpers;
using ATS_API.Recipes;
using ATS_JSONLoader;
using Eremite;
using Eremite.Buildings;

public class RainCatcherRecipeLoader : ARecipeLoader<RainCatcherRecipeModel, RainCatcherRecipeData>
{
    public const string fileExtension = "_rainCatcherRecipe.json";
    public override string FileExtension => fileExtension;
    public override string Category => "RainCatcherRecipes";
    public override IEnumerable<RainCatcherRecipeModel> AllModels => MB.Settings.rainCatchersRecipes;
    public override RainCatcherRecipeModel CreateNewModel(string guid, string name)
    {
        return (RainCatcherRecipeModel)RecipeManager.CreateRecipe<RainCatcherRecipeModel>(guid, name).RecipeModel;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        NewRainCatcherRecipeData newData = RecipeManager.NewRainCatcherRecipes.FirstOrDefault(a => a.Name == name);
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

    public override void Apply(RainCatcherRecipeModel model, RainCatcherRecipeData data, bool toModel, string modelName)
    {
        base.Apply(model, data, toModel, modelName);

        Logging.VerboseLog($"Applying JSON (RainCatcherRecipes) {modelName}");
        ImportExportUtils.ApplyValueNoNull(ref model.water, ref data.producedWater, toModel, "RainCatcherRecipes", "water");
        ImportExportUtils.ApplyValueNoNull(ref model.amount, ref data.producedAmount, toModel, "RainCatcherRecipes", "amount");
        ImportExportUtils.ApplyValueNoNull(ref model.productionTime, ref data.productionTime, toModel, "RainCatcherRecipes", "productionTime");
        
        if (toModel)
        {
            if (data.buildings != null)
            {
                foreach (RainCatcherModel buildingModel in SO.Settings.Buildings.Where(a=>a is RainCatcherModel).Cast<RainCatcherModel>())
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
            data.buildings = SO.Settings.Buildings.Where(a=>a is RainCatcherModel r && r.recipes.Contains(model)).Select(a=>a.name).ToArray();
        }
    }
}

[GenerateSchema("RainCatcher Recipe", "Recipes added to RainCatcher buildings to collect water. (ie: Clearance, drizzle, storm)", RainCatcherRecipeLoader.fileExtension)]
public class RainCatcherRecipeData : ARecipeData
{
    [SchemaEnum<WaterTypes>(WaterTypes.Clearance_Water, "The produced water when crafted")]
    public string producedWater;
    
    [SchemaField(1, "The amount of the good produced")]
    public int? producedAmount;
    
    [SchemaField(10f, "The time it takes to produce the water in seconds")] 
    public float? productionTime;

    [SchemaEnum<RainCatcherTypes>(RainCatcherTypes.Rain_Catcher, "Which raincatchers can use this recipe")] 
    public string[] buildings;
}