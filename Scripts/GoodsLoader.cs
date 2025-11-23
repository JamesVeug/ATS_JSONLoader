using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATS_API;
using ATS_API.Goods;
using ATS_API.Helpers;
using ATS_API.MetaRewards;
using ATS_JSONLoader;
using Eremite;
using Eremite.Model;
using Eremite.Model.Meta;
using Newtonsoft.Json;
using TinyJson;
using PluginInfo = ATS_JSONLoader.PluginInfo;

public class GoodsLoader
{
    public const string fileExtension = "_good.json";
    
    public static void LoadAll(List<string> files)
    {
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            if (!file.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
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

                bool isNewGood = false;
                GoodModel model = null;
                if (MB.Settings.ContainsGood(fullName))
                {
                    Logging.VerboseLog($"Found existing good {fullName}");
                    model = MB.Settings.GetGood(fullName);
                }
                else
                {
                    Logging.VerboseLog($"Creating new good {fullName}");
                    model = GoodsManager.New(data.guid, data.name, data.icon).goodModel;
                    isNewGood = true;
                }
                
                Logging.VerboseLog($"Applying JSON (goods) {file} to good {fullName}");
                Apply(model, data, true, fullName, isNewGood);

                Logging.VerboseLog($"Loaded JSON good {fullName}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Error loading JSON (goods) {file}\n{e}");
            }
        }
    }

    public static void Apply(GoodModel model, GoodsData data, bool toModel, string modelName, bool isNewGood)
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


        if (toModel)
        {
            // Data -> Model
            if (!isNewGood)
            {
                if (!string.IsNullOrEmpty(data.icon))
                {
                    ImportExportUtils.ApplyProperty(() => { return model.icon; }, (a) => { builder.SetIcon(a); },
                        ref data.icon,
                        toModel, "goods", "icon");
                }
            }

            if (data.embarkGoodMetaRewards != null)
            {
                var models = SO.Settings.metaRewards
                    .Where(a=>a is EmbarkGoodMetaRewardModel goodReward && goodReward.good.good == model)
                    .Select(a=>a as EmbarkGoodMetaRewardModel).ToList();
                
                // Edit existing meta rewards
                foreach (GoodMetaRewardData rewardData in data.embarkGoodMetaRewards)
                {
                    string expectedName = !string.IsNullOrEmpty(rewardData.guid) ? rewardData.guid + "_" + rewardData.name : rewardData.name;
                    
                    EmbarkGoodMetaRewardModel metaRewardModel = models.FirstOrDefault(a => a.name == expectedName);
                    if (metaRewardModel == null)
                    {
                        metaRewardModel = MetaRewardManager.New<EmbarkGoodMetaRewardModel>(rewardData.guid, rewardData.name).Model as EmbarkGoodMetaRewardModel;
                        metaRewardModel.good = new GoodRef();
                        metaRewardModel.good.good = model;
                    }
                    else
                    {
                        models.Remove(metaRewardModel);
                    }

                    EmbarkGoodMetaRewardBuilder goodBuilder = new EmbarkGoodMetaRewardBuilder(metaRewardModel);
                    EmbarkGoodMetaRewardModel rewardModel = goodBuilder.Model;
                    ImportExportUtils.ApplyValue(ref rewardModel.good.amount, ref rewardData.goodAmount, toModel, "metaReward", "goodAmount");
                    ImportExportUtils.ApplyLocaText(ref rewardModel.displayName, ref rewardData.displayName, (a,b)=>goodBuilder.SetDisplayName(a,b), toModel, "displayName");
                    ImportExportUtils.ApplyLocaText(ref rewardModel.description, ref rewardData.description, (a,b)=>goodBuilder.SetDescription(a,b), toModel, "description");
                    ImportExportUtils.ApplyVector2Int(ref rewardModel.costRange, ref rewardData.minCost, ref rewardData.maxCost, toModel, "metaReward", "costRange");
                }
                
                
                // Disable meta rewards not in the array
                // TODO: This is not working yet
            }
        }
        else
        {
            // Model -> Data
            ImportExportUtils.ApplyProperty(() => { return model.icon; }, (a) => { builder.SetIcon(a); }, ref data.icon,
                toModel, "goods", "icon");

            var models = SO.Settings.metaRewards.Where(a=>a is EmbarkGoodMetaRewardModel goodReward && goodReward.good.good == model)
                .Select(a=>a as EmbarkGoodMetaRewardModel).ToArray();
            if (models.Length > 0)
            {
                data.embarkGoodMetaRewards = new GoodMetaRewardData[models.Length];
                for (var i = 0; i < models.Length; i++)
                {
                    var metaRewardModel = models[i];
                    GoodMetaRewardData rewardData = new GoodMetaRewardData();
                    rewardData.Initialize();

                    string name = "";
                    string guid = "";
                    if (MetaRewardManager.NewMetaRewardsLookup.TryGetValue(metaRewardModel.name.ToMetaRewardTypes(), out var newMetaRewardData))
                    {
                        name = newMetaRewardData.rawName;
                        guid = newMetaRewardData.guid;
                    }
                    else
                    {
                        name = metaRewardModel.name;
                    }
                    
                    ImportExportUtils.ApplyValue(ref guid, ref rewardData.guid, toModel, "metaReward", "guid");
                    ImportExportUtils.ApplyValue(ref name, ref rewardData.name, toModel, "metaReward", "name");
                    ImportExportUtils.ApplyLocaText(ref metaRewardModel.displayName, ref rewardData.displayName, (a, b) => builder.SetDisplayName(a, b), false, "displayName");
                    ImportExportUtils.ApplyLocaText(ref metaRewardModel.description, ref rewardData.description, (a, b) => builder.SetDescription(a, b), false, "description");
                    rewardData.goodAmount = metaRewardModel.good.amount;
                    rewardData.minCost = metaRewardModel.costRange.x;
                    rewardData.maxCost = metaRewardModel.costRange.y;

                    data.embarkGoodMetaRewards[i] = rewardData;
                }
            }
        }
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
        Plugin.Log.LogInfo($"Exporting {MB.Settings.Goods.Length} Goods.");
        foreach (GoodModel goodModel in MB.Settings.Goods)
        {
            GoodsTypes goodsTypes = goodModel.name.ToGoodsTypes();
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
            
            Apply(goodModel, data, false, goodModel.name, false);
            
            string file = Path.Combine(Plugin.ExportDirectory, "Goods", goodModel.name + fileExtension);
            if(Directory.Exists(Path.GetDirectoryName(file)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }
            
            string json = JSONParser.ToJSON(data);
            File.WriteAllText(file, json);
        }
    }
}

[GenerateSchema("Goods", "Fuel/Eatable/Tradeable items/Amber/etc.", GoodsLoader.fileExtension)]
public class GoodsData : IInitializable
{
    [SchemaGuid] 
    public string guid;
    
    [SchemaName] 
    public string name;
    
    [SchemaIcon(TextureHelper.SpriteType.GoodIcon)] 
    public string icon;
    
    [SchemaDisplayName] 
    public LocalizableField displayName;
    
    [SchemaDescription] 
    public LocalizableField description;
    
    [SchemaShortDescription] 
    public LocalizableField shortDescription;
    
    [SchemaField(false, "If set to true will allow villagers to eat it. Use eatingFullness to set how full villagers get.")] 
    public bool? eatable;
    
    [SchemaField(0, "The order of which this will be good will be sorted in some lists.")] 
    public int? order;
    
    [SchemaField(40f, "How long this good will burn at the hearth if canBeBurned is set to true.")] 
    public float? burningTime;
    
    [SchemaField(1f, "How full villagers get when they eat this good if eatable is set to true.")] 
    public float? eatingFullness;
    
    [SchemaField(false, "If set to true then this good can be burnt at the hearth. Use burningTime to set how long it will burn.")] 
    public bool? canBeBurned;
    
    [SchemaField(true)] 
    public bool? showStorageAmount;
    
    [SchemaField(true)] 
    public bool? isOnHUD;
    
    [SchemaEnum<GoodsCategoriesTypes>(GoodsCategoriesTypes.Food, "Column it appears in to enable/disable use by villagers. If not set API will auto set it.")] 
    public string category;
    
    [SchemaField(null)] 
    public string[] tags;
    
    [SchemaField("", "Identifier to be used by the console??")] 
    public string consoleId;
    
    [SchemaField(4.5f, "How much the player will receive for selling this.")] 
    public float? tradingBuyValue;
    
    [SchemaField(true, "If set to true then this good will be sold by all traders.")] 
    public bool? allTradersSellThisGood;
    
    [SchemaTraderType(TraderTypes.Trader_0_General, "Name of traders that can sell this good.")] 
    public string[] tradersSellingThisGood;
    
    [SchemaField(2.25f, "How much traders will sell this good for.")] 
    public float? tradingSellValue;
    
    [SchemaField(true, "If set to true then the player can sell this good to all traders.")] 
    public bool? allTradersBuyThisGood;
    
    [SchemaTraderType(TraderTypes.Trader_0_General, "Name of traders that can buy this off the player..")] 
    public string[] tradersBuyingThisGood;
    
    [SchemaField(null, "Rewards the player can choose when starting a new settlement. If not included then will not affect existing meta rewards.")] 
    public GoodMetaRewardData[] embarkGoodMetaRewards;

    public void Initialize()
    {
        displayName = new LocalizableField("displayName");
        description = new LocalizableField("description");
        shortDescription = new LocalizableField("shortDescription");
    }
}

public class GoodMetaRewardData : IInitializable
{
    [SchemaGuid] 
    public string guid;
    
    [SchemaGuid] 
    public string name;
    
    [SchemaDisplayName] 
    public LocalizableField displayName;
    
    [SchemaDescription] 
    public LocalizableField description;
    
    [SchemaField("How much of this good you will get when selecting this reward.")]
    public int goodAmount;
    
    [SchemaField("Minimum cost required to get this meta reward.")]
    public int minCost;
    
    [SchemaField("Maximum cost required to get this meta reward.")]
    public int maxCost;


    public void Initialize()
    {
        displayName = new LocalizableField("displayName");
        description = new LocalizableField("description");
    }
}