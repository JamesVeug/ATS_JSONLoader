using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATS_API.Helpers;
using ATS_API.Recipes;
using ATS_JSONLoader;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;
using TinyJson;
using PluginInfo = ATS_JSONLoader.PluginInfo;

public class WorkshopRecipeLoader
{
    public static void LoadAll(List<string> files)
    {
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            if (!file.EndsWith("_workshopRecipe.json"))
            {
                continue;
            }
            
            ImportExportUtils.SetDebugPath(file);
            files.RemoveAt(i--);
            
            try
            {
                Logging.VerboseLog($"Loading JSON (workshopRecipes) {file}");
                WorkshopRecipeData data = JSONParser.FromFilePath<WorkshopRecipeData>(file);
                if (data == null)
                {
                    Plugin.Log.LogError($"Failed to load JSON (workshopRecipes) {file}");
                    continue;
                }
                Logging.VerboseLog($"Loaded JSON (workshopRecipes) {file}");

                // If we did not specify a GUID and this is NOT a vanilla item then use a default guid.
                data.guid ??= MB.Settings.workshopsRecipes.Any(a=>a.name == data.name) ? "" : PluginInfo.PLUGIN_GUID;
                
                string guidPrefix = !string.IsNullOrEmpty(data.guid) ? data.guid + "_" : "";
                string fullName = guidPrefix + data.name;

                bool isNewRecipe = false;
                WorkshopRecipeModel model = null;
                if (MB.Settings.workshopsRecipes.Any(a=>a.name == fullName))
                {
                    Logging.VerboseLog($"Found existing workshopRecipe {fullName}");
                    model = MB.Settings.workshopsRecipes.First(a=>a.name == fullName);
                }
                else
                {
                    Logging.VerboseLog($"Creating new workshopRecipe {fullName}");
                    model = (WorkshopRecipeModel)RecipeManager.CreateRecipe<WorkshopRecipeModel>(data.guid, data.name).RecipeModel;
                    isNewRecipe = true;
                }
                
                Logging.VerboseLog($"Applying JSON (workshopRecipes) {file} to workshopRecipe {fullName}");
                Apply(model, data, true, fullName);

                Logging.VerboseLog($"Loaded JSON workshopRecipe {fullName}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Error loading JSON (workshopRecipes) {file}\n{e}");
            }
        }
    }

    public static void Apply(WorkshopRecipeModel model, WorkshopRecipeData data, bool toModel, string modelName)
    {
        ImportExportUtils.SetID(modelName);

        Logging.VerboseLog($"Applying JSON (workshopRecipes) {modelName}");
        ImportExportUtils.ApplyValueNoNull(ref model.grade, ref data.grade, toModel, "workshopRecipes", "grade");
        ImportExportUtils.ApplyValueNoNull(ref model.producedGood.good, ref data.producedGood, toModel, "workshopRecipes", "producedGood");
        ImportExportUtils.ApplyValueNoNull(ref model.producedGood.amount, ref data.producedAmount, toModel, "workshopRecipes", "producedAmount");
        ImportExportUtils.ApplyValueNoNull(ref model.productionTime, ref data.productionTime, toModel, "workshopRecipes", "productionTime");
        ImportExportUtils.ApplyValueNoNull(ref model.tags, ref data.tags, toModel, "workshopRecipes", "tags");

        Logging.VerboseLog($"Required Goods: {data.requiredGoods}");
        if (toModel)
        {
            if (data.requiredGoods != null)
            {
                model.requiredGoods = new GoodsSet[data.requiredGoods.Length];
                for (var i = 0; i < data.requiredGoods.Length; i++)
                {
                    Logging.VerboseLog($"Required Goods Set: {i}");
                    var set = data.requiredGoods[i];
                    var requiredGoodSet = new GoodsSet();
                    requiredGoodSet.goods = new GoodRef[set.goods.Length];
                    for (var j = 0; j < set.goods.Length; j++)
                    {
                        Logging.VerboseLog($"\tRequired Good: {j}");
                        var good = set.goods[j];
                        var requiredGood = new GoodRef();
                        requiredGood.good = good.good.ToGoodModel();
                        requiredGood.amount = good.amount;
                        requiredGoodSet.goods[j] = requiredGood;
                    }
                    model.requiredGoods[i] = requiredGoodSet;
                }
            }
        }
        else
        {
            data.requiredGoods = new GoodSet[model.requiredGoods.Length];
            for (var i = 0; i < model.requiredGoods.Length; i++)
            {
                var set = model.requiredGoods[i];
                var goodSet = new GoodSet();
                goodSet.goods = new RequiredGood[set.goods.Length];
                for (var j = 0; j < set.goods.Length; j++)
                {
                    var good = set.goods[j];
                    var requiredGood = new RequiredGood();
                    requiredGood.good = good.good.name;
                    requiredGood.amount = good.amount;
                    goodSet.goods[j] = requiredGood;
                }
                data.requiredGoods[i] = goodSet;
            }
        }
        
        Logging.VerboseLog($"Workshops");
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
    
    
    public static void ExportAll()
    {
        foreach (WorkshopRecipeModel model in MB.Settings.workshopsRecipes)
        {
            WorkshopRecipeData data = new WorkshopRecipeData();
            data.Initialize();

            NewWorkshopRecipeData newModel = RecipeManager.NewWorkshopRecipes.FirstOrDefault(a=>a.RecipeModel == model);
            if (newModel != null)
            {
                data.guid = newModel.Guid;
                data.name = newModel.Name;
            }
            else
            {
                data.name = model.name;
            }
            
            Apply(model, data, false, model.name);
            
            string file = Path.Combine(Plugin.ExportDirectory, "workshopRecipes", model.name + "_workshopRecipe.json");
            if(Directory.Exists(Path.GetDirectoryName(file)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }
            
            string json = JSONParser.ToJSON(data);
            File.WriteAllText(file, json);
        }
    }
}

[GenerateSchema("Workshop Recipe", "Recipes added to workshop buildings to produce goods")]
public class WorkshopRecipeData : IInitializable
{
    [SchemaGuid] 
    public string guid;
    
    [SchemaName] 
    public string name;
    
    [SchemaEnum<Grade>(Grade.One, "The grade of the recipe")] 
    public string grade;
    
    [SchemaEnum<GoodsTypes>(GoodsTypes.Crafting_Oil, "The produced good when crafted")]
    public string producedGood;
    
    [SchemaField(1, "The amount of the good produced")]
    public int producedAmount;
    
    [SchemaField(10f, "The time it takes to produce the good in seconds")] 
    public float productionTime;
    
    [SchemaField(null, "Rewards the player can choose when starting a new settlement. If not included then will not affect existing meta rewards.")] 
    public GoodSet[] requiredGoods;
    
    [SchemaEnum<TagTypes>(TagTypes.Building_Material_Tag, "Tags of which some effects can affect this recipe by")]
    public string[] tags;

    [SchemaEnum<WorkshopTypes>(WorkshopTypes.Apothecary, "Which workshops can use this recipe")] 
    public string[] buildings;
    
    public void Initialize()
    {
        
    }
}

public class GoodSet : IInitializable
{
    [SchemaField(null, "Rewards the player can choose when starting a new settlement. If not included then will not affect existing meta rewards.")] 
    public RequiredGood[] goods;

    public void Initialize()
    {
        
    }
}

public class RequiredGood : IInitializable
{
    [SchemaEnum<GoodsTypes>(GoodsTypes.Food_Raw_Herbs, "The good required")]
    public string good;
    
    [SchemaField(0, "The amount of the good required")]
    public int amount;

    public void Initialize()
    {
        
    }
}
