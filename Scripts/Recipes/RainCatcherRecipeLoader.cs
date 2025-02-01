using System.Collections.Generic;
using System.Linq;
using ATS_API.Helpers;
using ATS_API.Recipes;
using ATS_JSONLoader;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;

public class RainCatcherRecipeLoader : ARecipeLoader<RainCatcherRecipeModel, RainCatcherRecipeData>
{
    public const string fileExtension = "_RainCatcherRecipe.json";
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
    }
}

[GenerateSchema("RainCatcher Recipe", "Recipes added to RainCatcher buildings to produce goods.", RainCatcherRecipeLoader.fileExtension)]
public class RainCatcherRecipeData : ARecipeData
{
    [SchemaEnum<WaterTypes>(WaterTypes.Clearance_Water, "The produced water when crafted")]
    public string producedWater;
    
    [SchemaField(1, "The amount of the good produced")]
    public int? producedAmount;
    
    [SchemaField(10f, "The time it takes to produce the water in seconds")] 
    public float? productionTime;
}