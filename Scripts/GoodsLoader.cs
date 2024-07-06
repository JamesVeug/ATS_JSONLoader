using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATS_API.Goods;
using ATS_API.Helpers;
using ATS_JSONLoader;
using Eremite;
using Eremite.Model;
using Eremite.Model.Effects;
using TinyJson;

public class GoodsLoader
{
    public static void LoadAll(List<string> files)
    {
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            if (!file.EndsWith("_good.json"))
            {
                continue;
            }
            
            ImportExportUtils.SetDebugPath(file);
            files.RemoveAt(i--);
            
            try
            {
                Logging.VerboseLog($"Loading JSON (goods) {file}");
                GoodsData data = JSONParser.FromFilePath<GoodsData>(file);
                if (data == null)
                {
                    Plugin.Log.LogError($"Failed to load JSON (goods) {file}");
                    continue;
                }
                Logging.VerboseLog($"Loaded JSON (goods) {file}");

                // If we did not specify a GUID and this is NOT a vanilla item then use a default guid.
                data.guid ??= MB.Settings.ContainsGood(data.name) ? "" : PluginInfo.PLUGIN_GUID;
                
                string guidPrefix = !string.IsNullOrEmpty(data.guid) ? data.guid + "_" : "";
                string fullName = guidPrefix + data.name;
                
                GoodModel model = null;
                if (MB.Settings.ContainsGood(fullName))
                {
                    Logging.VerboseLog($"Found existing good {fullName}");
                    model = MB.Settings.GetGood(fullName);
                }
                else
                {
                    Logging.VerboseLog($"Creating good {fullName}");
                    model = GoodsManager.New(data.guid, data.name, data.icon).goodModel;
                }
                
                Logging.VerboseLog($"Applying JSON (goods) {file} to good {fullName}");
                Apply(model, data, true, fullName);

                Logging.VerboseLog($"Loaded JSON good {fullName}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Error loading JSON (goods) {file}\n{e}");
            }
        }
    }

    public static void Apply(GoodModel model, GoodsData data, bool toModel, string modelName)
    {
        ImportExportUtils.SetID(modelName);

        GoodsBuilder builder = new GoodsBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        ImportExportUtils.ApplyLocaText(ref model.shortDescription, ref data.shortDescription, (a,b)=>builder.SetShortDescription(a,b), toModel, "shortDescription");
        
        ImportExportUtils.ApplyProperty(() =>
        {
            return model.category.name;
        }, (a) =>
        {
            builder.SetCategory(a);
        }, ref data.category, toModel, "goods", "category");

        ApplyTraderSellingFields(builder, data, toModel, modelName);
        ApplyTraderBuyingFields(builder, data, toModel, modelName);


        ImportExportUtils.ApplyValueNoNull(ref model.eatable, ref data.eatable, toModel, "goods", "eatable");
        ImportExportUtils.ApplyValueNoNull(ref model.icon, ref data.icon, toModel, "goods", "icon");
        ImportExportUtils.ApplyValueNoNull(ref model.order, ref data.order, toModel, "goods", "order");
        ImportExportUtils.ApplyValueNoNull(ref model.burningTime, ref data.burningTime, toModel, "goods", "burningTime");
        ImportExportUtils.ApplyValueNoNull(ref model.eatingFullness, ref data.eatingFullness, toModel, "goods", "eatingFullness");
        ImportExportUtils.ApplyValueNoNull(ref model.canBeBurned, ref data.canBeBurned, toModel, "goods", "canBeBurned");
        ImportExportUtils.ApplyValueNoNull(ref model.showStorageAmount, ref data.showStorageAmount, toModel, "goods", "showStorageAmount");
        ImportExportUtils.ApplyValueNoNull(ref model.tradingBuyValue, ref data.tradingBuyValue, toModel, "goods", "tradingBuyValue");
        ImportExportUtils.ApplyValueNoNull(ref model.tradingSellValue, ref data.tradingSellValue, toModel, "goods", "tradingSellValue");
        ImportExportUtils.ApplyValueNoNull(ref model.isOnHUD, ref data.isOnHUD, toModel, "goods", "isOnHUD");
        ImportExportUtils.ApplyValueNoNull(ref model.consoleId, ref data.consoleId, toModel, "goods", "consoleId");
        ImportExportUtils.ApplyValueNoNull(ref model.tags, ref data.tags, toModel, "goods", "tags");
    }

    private static void ApplyTraderBuyingFields(GoodsBuilder builder, GoodsData data, bool toModel, string modelName)
    {
        var allTraders = MB.Settings.traders;
        var tradersBuyingThisGood = MB.Settings.traders.Where(a => a.desiredGoods.Any(b =>
        {
            return b.name == modelName;
        })).Select(a=>a.name);
        
        // allTradersBuyThisGood
        ImportExportUtils.ApplyProperty(() =>
        {
            // get bool from model
            return allTraders.Length == tradersBuyingThisGood.Count();
        }, (a) =>
        {
            // Set model bool
            if (a)
            {
                builder.CanBeSoldToAllTraders();
            }
        }, ref data.allTradersBuyThisGood, !toModel, "goods", "allTradersBuyThisGood");
        
        
        // tradersBuyingThisGood
        ImportExportUtils.ApplyProperty(() =>
        {
            // get traders from model
            return tradersBuyingThisGood.ToArray();
        }, (a) =>
        {
            // Set model traders
            if (a != null)
            {
                foreach (string trader in a)
                {
                    builder.SetCanBeSoldToTrader(trader);
                }
            }
        }, ref data.tradersBuyingThisGood, toModel, "goods", "tradersBuyingThisGood");
    }

    private static void ApplyTraderSellingFields(GoodsBuilder model, GoodsData data, bool toModel, string modelName)
    {
        var allTraders = MB.Settings.traders;
        var tradersSellingThisGood = MB.Settings.traders
            .Where(a =>
            {
                return a.guaranteedOfferedGoods.Any(b => b.good.name == modelName) || a.offeredGoods.Any(b=>b.Value.name == modelName);
            })
            .Select(a=>a.name);
        
        // allTradersSellThisGood
        ImportExportUtils.ApplyProperty(() =>
        {
            // get bool from model
            return allTraders.Length == tradersSellingThisGood.Count();
        }, (a) =>
        {
            // Set model bool
            if (a)
            {
                model.SetSoldByAllTraders();
            }
        }, ref data.allTradersSellThisGood, toModel, "goods", "allTradersSellThisGood");
        
        
        // tradersSellingThisGood
        ImportExportUtils.ApplyProperty(() =>
        {
            // get traders from model
            return tradersSellingThisGood.ToArray();
        }, (a) =>
        {
            // Set model traders
            if (a != null)
            {
                foreach (string trader in a)
                {
                    model.AddTraderSellingGood(trader);
                }
            }
        }, ref data.tradersSellingThisGood, toModel, "goods", "tradersSellingThisGood");
    }
    
    

    public static void ExportAll()
    {
        foreach (GoodModel goodModel in MB.Settings.Goods)
        {
            GoodsTypes goodsTypes = goodModel.name.ToGoodsType();
            GoodsData data = new GoodsData();
            data.Initialize();
            
            if (GoodsManager.NewGoodsLookup.TryGetValue(goodsTypes, out var newModel))
            {
                data.guid = newModel.guid;
                data.name = newModel.rawName;
            }
            else
            {
                data.name = goodModel.name;
            }
            
            Apply(goodModel, data, false, goodModel.name);
            
            string file = Path.Combine(Plugin.ExportDirectory, "goods", goodModel.name + "_good.json");
            string json = JSONParser.ToJSON(data);
            File.WriteAllText(file, json);
        }
    }
}

public class GoodsData : IInitializable
{
    public string guid;
    public string name;
    public string icon;
    public LocalizableField displayName;
    public LocalizableField description;
    public LocalizableField shortDescription;
    public bool? eatable;
    public int? order;
    public float? burningTime;
    public float? eatingFullness;
    public bool? canBeBurned;
    public bool? showStorageAmount;
    public bool? isOnHUD;
    public string category;
    public string[] tags;
    public string consoleId;
    public float? tradingBuyValue;
    public bool? allTradersSellThisGood;
    public string[] tradersSellingThisGood;
    public float? tradingSellValue;
    public bool? allTradersBuyThisGood;
    public string[] tradersBuyingThisGood;

    public void Initialize()
    {
        displayName = new LocalizableField("displayName");
        description = new LocalizableField("description");
        shortDescription = new LocalizableField("shortDescription");
    }
}