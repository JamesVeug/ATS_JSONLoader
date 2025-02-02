﻿using System.Collections.Generic;
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
    public const string fileExtension = "_gathererHutRecipe.json";
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
        
        if (toModel)
        {
            if (data.buildings != null)
            {
                foreach (var buildingModel in SO.Settings.Buildings.Where(a=>a is GathererHutModel).Cast<GathererHutModel>())
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
            data.buildings = SO.Settings.Buildings.Where(a=>a is GathererHutModel r && r.recipes.Contains(model)).Select(a=>a.name).ToArray();
        }
    }
}

[GenerateSchema("GathererHut Recipe", "Recipes added to GathererHut buildings to collect goods from nodes on maps. (ie: Clay, Eggs, Berry's... etc).", GathererHutRecipeLoader.fileExtension)]
public class GathererHutRecipeData : ARecipeData
{
    [SchemaField(null, "The good that is produced by the recipe.")]
    public HelperMethods.GoodRefData refGood;
    
    [SchemaField(10f, "The time it takes to produce the good in seconds")] 
    public float? productionTime;
    
    [SchemaLocalized("Description of the recipes grade")] 
    public LocalizableField gradeDesc;

    [SchemaEnum<GathererHutTypes>(GathererHutTypes.Foragers_Camp, "Which gatherer buildings can use this recipe")] 
    public string[] buildings;
}