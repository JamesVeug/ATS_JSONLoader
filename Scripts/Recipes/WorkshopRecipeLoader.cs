﻿using System.Collections.Generic;
using System.Linq;
using ATS_API.Helpers;
using ATS_API.Recipes;
using ATS_JSONLoader;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;

public class WorkshopRecipeLoader : ARecipeLoader<WorkshopRecipeModel, WorkshopRecipeData>
{
    public const string fileExtension = "_workshopRecipe.json";
    public override string FileExtension => fileExtension;
    public override string Category => "WorkshopRecipes";
    public override IEnumerable<WorkshopRecipeModel> AllModels => MB.Settings.workshopsRecipes;
    public override WorkshopRecipeModel CreateNewModel(string guid, string name)
    {
        return (WorkshopRecipeModel)RecipeManager.CreateRecipe<WorkshopRecipeModel>(guid, name).RecipeModel;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        NewWorkshopRecipeData newData = RecipeManager.NewWorkshopRecipes.FirstOrDefault(a => a.Name == name);
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

    public override void Apply(WorkshopRecipeModel model, WorkshopRecipeData data, bool toModel, string modelName)
    {
        base.Apply(model, data, toModel, modelName);

        Logging.VerboseLog($"Applying JSON (workshopRecipes) {modelName}");
        ImportExportUtils.ApplyValueNoNull(ref model.producedGood.good, ref data.producedGood, toModel, "workshopRecipes", "producedGood");
        ImportExportUtils.ApplyValueNoNull(ref model.producedGood.amount, ref data.producedAmount, toModel, "workshopRecipes", "producedAmount");
        ImportExportUtils.ApplyValueNoNull(ref model.productionTime, ref data.productionTime, toModel, "workshopRecipes", "productionTime");
        ImportExportUtils.ApplyValueNoNull(ref model.requiredGoods, ref data.requiredGoods, toModel, "workshopRecipes", "requiredGoods");
        
        if (toModel)
        {
            if (data.buildings != null)
            {
                foreach (WorkshopModel workshop in SO.Settings.workshops)
                {
                    bool shouldContainRecipe = data.buildings.Contains(workshop.name);
                    if (shouldContainRecipe && !workshop.recipes.Contains(model))
                    {
                        workshop.recipes = workshop.recipes.ForceAdd(model);
                    }
                    else if (!shouldContainRecipe && workshop.recipes.Contains(model))
                    {
                        workshop.recipes = workshop.recipes.Where(a=>a != model).ToArray();
                    }
                }
            }
        }
        else
        {
            data.buildings = SO.Settings.workshops.Where(a=>a.recipes.Contains(model)).Select(a=>a.name).ToArray();
        }
    }
}

[GenerateSchema("Workshop Recipe", "Recipes added to workshop buildings to produce goods.", WorkshopRecipeLoader.fileExtension)]
public class WorkshopRecipeData : ARecipeData
{
    [SchemaEnum<GoodsTypes>(GoodsTypes.Crafting_Oil, "The produced good when crafted")]
    public string producedGood;
    
    [SchemaField(1, "The amount of the good produced")]
    public int? producedAmount;
    
    [SchemaField(10f, "The time it takes to produce the good in seconds")] 
    public float? productionTime;
    
    [SchemaField(null, "Rewards the player can choose when starting a new settlement. If not included then will not affect existing meta rewards.")] 
    public HelperMethods.GoodSetData[] requiredGoods;

    [SchemaEnum<WorkshopTypes>(WorkshopTypes.Apothecary, "Which workshops can use this recipe")] 
    public string[] buildings;
}