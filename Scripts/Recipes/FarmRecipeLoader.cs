using System.Collections.Generic;
using System.Linq;
using ATS_API.Helpers;
using ATS_API.Recipes;
using ATS_JSONLoader;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;

public class FarmRecipeLoader : ARecipeLoader<FarmRecipeModel, FarmRecipeData>
{
    public const string fileExtension = "_FarmRecipe.json";
    public override string FileExtension => fileExtension;
    public override string Category => "FarmRecipes";
    public override IEnumerable<FarmRecipeModel> AllModels => MB.Settings.farmsRecipes;
    public override FarmRecipeModel CreateNewModel(string guid, string name)
    {
        return (FarmRecipeModel)RecipeManager.CreateRecipe<FarmRecipeModel>(guid, name).RecipeModel;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        NewFarmRecipeData newData = RecipeManager.NewFarmRecipes.FirstOrDefault(a => a.Name == name);
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

    public override void Apply(FarmRecipeModel model, FarmRecipeData data, bool toModel, string modelName)
    {
        base.Apply(model, data, toModel, modelName);

        Logging.VerboseLog($"Applying JSON (FarmRecipes) {modelName}");
        ImportExportUtils.ApplyValueNoNull(ref model.producedGood.good, ref data.producedGood, toModel, "FarmRecipes", "producedGood");
        ImportExportUtils.ApplyValueNoNull(ref model.producedGood.amount, ref data.producedAmount, toModel, "FarmRecipes", "producedAmount");
        ImportExportUtils.ApplyValueNoNull(ref model.plantingTime, ref data.plantingTime, toModel, "FarmRecipes", "plantingTime");
        ImportExportUtils.ApplyValueNoNull(ref model.harvestTime, ref data.harvestTime, toModel, "FarmRecipes", "harvestTime");
    }
}

[GenerateSchema("Farm Recipe", "Recipes added to Farm buildings to produce goods.", FarmRecipeLoader.fileExtension)]
public class FarmRecipeData : ARecipeData
{
    [SchemaEnum<GoodsTypes>(GoodsTypes.Crafting_Oil, "The produced good when crafted")]
    public string producedGood;
    
    [SchemaField(1, "The amount of the good produced")]
    public int? producedAmount;
    
    [SchemaField(10f, "The time it takes to produce the good in seconds")] 
    public float? plantingTime;
    
    [SchemaField(10f, "The time it takes to harvest the good in seconds")]
    public float? harvestTime;
}