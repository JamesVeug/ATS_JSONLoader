using System.Collections.Generic;
using System.Linq;
using ATS_API.Helpers;
using ATS_API.Recipes;
using ATS_JSONLoader;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;

public class CollectorRecipeLoader : ARecipeLoader<CollectorRecipeModel, CollectorRecipeData>
{
    public const string fileExtension = "_CollectorRecipe.json";
    public override string FileExtension => fileExtension;
    public override string Category => "CollectorRecipes";
    public override IEnumerable<CollectorRecipeModel> AllModels => MB.Settings.collectorsRecipes;
    public override CollectorRecipeModel CreateNewModel(string guid, string name)
    {
        return (CollectorRecipeModel)RecipeManager.CreateRecipe<CollectorRecipeModel>(guid, name).RecipeModel;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        NewCollectorRecipeData newData = RecipeManager.NewCollectorRecipes.FirstOrDefault(a => a.Name == name);
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

    public override void Apply(CollectorRecipeModel model, CollectorRecipeData data, bool toModel, string modelName)
    {
        base.Apply(model, data, toModel, modelName);

        Logging.VerboseLog($"Applying JSON (CollectorRecipes) {modelName}");
        ImportExportUtils.ApplyValueNoNull(ref model.producedGood.good, ref data.producedGood, toModel, "CollectorRecipes", "producedGood");
        ImportExportUtils.ApplyValueNoNull(ref model.producedGood.amount, ref data.producedAmount, toModel, "CollectorRecipes", "producedAmount");
        ImportExportUtils.ApplyValueNoNull(ref model.productionTime, ref data.productionTime, toModel, "CollectorRecipes", "productionTime");
        
        if (toModel)
        {
            if (data.buildings != null)
            {
                foreach (var buildingModel in SO.Settings.Buildings.Where(a=>a is CollectorModel).Cast<CollectorModel>())
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
            data.buildings = SO.Settings.Buildings.Where(a=>a is CollectorModel r && r.recipes.Contains(model)).Select(a=>a.name).ToArray();
        }
    }
}

[GenerateSchema("Collector Recipe", "Recipes added to Collector buildings to produce goods.", CollectorRecipeLoader.fileExtension)]
public class CollectorRecipeData : ARecipeData
{
    [SchemaEnum<GoodsTypes>(GoodsTypes.Crafting_Oil, "The produced good when crafted")]
    public string producedGood;
    
    [SchemaField(1, "The amount of the good produced")]
    public int? producedAmount;
    
    [SchemaField(10f, "The time it takes to produce the good in seconds")] 
    public float? productionTime;

    [SchemaEnum<CollectorTypes>(CollectorTypes.None, "Which collector buildings can use this recipe")] 
    public string[] buildings;
}