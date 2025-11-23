using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATS_API.Helpers;
using ATS_API.MetaRewards;
using ATS_JSONLoader;
using Eremite;
using Eremite.Model;
using Eremite.Model.Meta;
using TinyJson;
using PluginInfo = ATS_JSONLoader.PluginInfo;

public class MetaRewardLoader
{
    public const string fileExtension = "_metaReward.json";
    
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
                Logging.VerboseLog($"Loading JSON (MetaReward) {file}");
                MetaRewardData data = JSONParser.FromFilePath<MetaRewardData>(file);
                if (data == null)
                {
                    Plugin.Log.LogError($"Failed to load JSON (MetaReward) {file}");
                    continue;
                }
                Logging.VerboseLog($"Loaded JSON (MetaReward) {file}");

                // If we did not specify a GUID and this is NOT a vanilla item then use a default guid.
                data.guid ??= MB.Settings.metaRewards.Any(a=>a.name == data.name) ? "" : PluginInfo.PLUGIN_GUID;
                
                string guidPrefix = !string.IsNullOrEmpty(data.guid) ? data.guid + "_" : "";
                string fullName = guidPrefix + data.name;

                bool isNewMetaReward = false;
                
                object builder = null;
                if (MB.Settings.metaRewards.Any(a=>a.name == fullName))
                {
                    Logging.VerboseLog($"Found existing MetaReward {fullName}");
                    var model = MB.Settings.metaRewards.First(a=>a.name == fullName);
                    builder = CreateBuilder(data.type, model);
                }
                else
                {
                    Logging.VerboseLog($"Creating new MetaReward {fullName}");
                    builder = CreateBuilder(data.type, data.guid, data.name);
                    isNewMetaReward = true;
                }
                
                Logging.VerboseLog($"Applying JSON (MetaReward) {file} to MetaReward {fullName}");
                Apply(data, true, fullName, isNewMetaReward, builder);

                Logging.VerboseLog($"Loaded JSON MetaReward {fullName}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Error loading JSON (MetaReward) {file}\n{e}");
            }
        }
    }

    private static object CreateBuilder(string typeString, params object[] args)
    {
        MetaRewardData.MetaRewardTypes type;
        if(!Enum.TryParse(typeString, out type))
        {
            type = MetaRewardData.MetaRewardTypes.Unknown;
        }
        
        Type genericType = null;
        switch (type)
        {
            case MetaRewardData.MetaRewardTypes.EmbarkGoodMetaReward:
                genericType = typeof(EmbarkGoodMetaRewardBuilder);
                break;
            case MetaRewardData.MetaRewardTypes.EmbarkEffectMetaReward:
                genericType = typeof(EmbarkEffectMetaRewardBuilder);
                break;
            default:
                throw new NotSupportedException("Unknown MetaRewardType: " + typeString);
        }
        
        return (object)Activator.CreateInstance(genericType, args);
    }

    public static void Apply(MetaRewardData data, bool toModel, string modelName, bool isNewMetaReward, object builder)
    {
        ImportExportUtils.SetID(modelName);
        Plugin.Log.LogInfo("Applying MetaReward " + modelName);

        if (builder != null)
        {
            if(builder is EmbarkGoodMetaRewardBuilder goodBuilder)
            {
                EmbarkGoodMetaRewardModel rewardModel = goodBuilder.Model;
                ImportExportUtils.ApplyLocaText(ref rewardModel.displayName, ref data.displayName, (a,b)=>goodBuilder.SetDisplayName(a,b), toModel, "displayName");
                ImportExportUtils.ApplyLocaText(ref rewardModel.description, ref data.description, (a,b)=>goodBuilder.SetDescription(a,b), toModel, "description");
                ImportExportUtils.ApplyVector2Int(ref rewardModel.costRange, ref data.minCost, ref data.maxCost, toModel, "metaReward", "costRange");
                ImportExportUtils.ApplyProperty(ref rewardModel.good, 
                    ()=>
                    {
                        Plugin.Log.LogInfo($"Getting good {modelName} with good {data.good} and name {data.good.ToGoodModel()?.name}");
                        return new GoodRef()
                        {
                            good = data.good.ToGoodModel(),
                            amount = data.goodAmount
                        };
                    }, (a)=>
                    {
                        Plugin.Log.LogInfo($"Setting good {modelName} with good {a} and name {a?.good?.name} from {rewardModel.good}");
                        data.good = a?.good?.name ?? "";
                        data.goodAmount = a?.amount ?? 0;
                    },
                    toModel, "metaReward", "good");
            }
            else if(builder is EmbarkEffectMetaRewardBuilder effectBuilder)
            {
                EmbarkEffectMetaRewardModel rewardModel = effectBuilder.Model;
                ImportExportUtils.ApplyLocaText(ref rewardModel.displayName, ref data.displayName, (a,b)=>effectBuilder.SetDisplayName(a,b), toModel, "displayName");
                ImportExportUtils.ApplyLocaText(ref rewardModel.description, ref data.description, (a,b)=>effectBuilder.SetDescription(a,b), toModel, "description");
                ImportExportUtils.ApplyProperty(()=>rewardModel.effect?.name.ToEffectTypes() ?? EffectTypes.None, (a)=>effectBuilder.SetEffect(a), ref data.effect, toModel, "metaReward", "effect");
                ImportExportUtils.ApplyVector2Int(ref rewardModel.costRange, ref data.minCost, ref data.maxCost, toModel, "metaReward", "costRange");
            }
        }
     
        // MetaRewardBuilder<MetaRewardModel> metaRewardBuilder = builder as MetaRewardBuilder<MetaRewardModel>;
        // ImportExportUtils.ApplyValueNoNull(ref model.shortName, ref data.shortName, toModel, "MetaReward", "shortName");
    }
    
    public static void ExportAll()
    {
        Plugin.Log.LogInfo($"Exporting {MB.Settings.metaRewards.Length} MetaRewards.");
        foreach (MetaRewardModel model in MB.Settings.metaRewards)
        {
            MetaRewardData.MetaRewardTypes dataType = MetaRewardData.MetaRewardTypes.Unknown;
            if (model is EmbarkGoodMetaRewardModel goodReward)
            {
                dataType = MetaRewardData.MetaRewardTypes.EmbarkGoodMetaReward;
            }
            else if(model is EmbarkEffectMetaRewardModel)
            {
                dataType = MetaRewardData.MetaRewardTypes.EmbarkEffectMetaReward;
            }
            else
            {
                Plugin.Log.LogError($"Unknown MetaReward type {model.GetType()}");
                continue;
            }
            
            MetaRewardTypes type = model.name.ToMetaRewardTypes();
            MetaRewardData data = new MetaRewardData();
            data.Initialize();
            data.type = dataType.ToString();

            bool isNewMetaReward = MetaRewardManager.NewMetaRewardsLookup.TryGetValue(type, out var newModel);
            if (isNewMetaReward)
            {
                data.guid = newModel.guid;
                data.name = newModel.rawName;
            }
            else
            {
                data.name = model.name;
            }

            var builder = CreateBuilder(dataType.ToString(), model);
            if (model is EmbarkGoodMetaRewardModel goodReward2)
            {
                Plugin.Log.LogInfo("Good model good " + goodReward2.good);
                
                EmbarkGoodMetaRewardBuilder goodBuilder = builder as EmbarkGoodMetaRewardBuilder;
                Plugin.Log.LogInfo("Good builder model " + goodBuilder.Model);
                Plugin.Log.LogInfo("Good builder model good " + goodBuilder.Model.good);
            }
            Apply(data, false, model.name, isNewMetaReward, builder);
            
            string file = Path.Combine(Plugin.ExportDirectory, "MetaRewards", model.name + fileExtension);
            if(Directory.Exists(Path.GetDirectoryName(file)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }
            
            string json = JSONParser.ToJSON(data);
            File.WriteAllText(file, json);
        }
    }
}

[GenerateSchema("MetaReward", "Rewards given to the player when embarking.", MetaRewardLoader.fileExtension)]
public class MetaRewardData : IInitializable
{
    public enum MetaRewardTypes
    {
        Unknown,
        EmbarkGoodMetaReward,
        EmbarkEffectMetaReward,
    }

    [SchemaGuid]
    public string guid;
    
    [SchemaName]
    public string name;
    
    [SchemaEnumAttribute<MetaRewardTypes>(MetaRewardTypes.EmbarkGoodMetaReward, true, "Type of meta reward")]
    public string type;
    
    [SchemaDisplayName]
    public LocalizableField displayName;
    
    [SchemaDescription]
    public LocalizableField description;
    
    [SchemaEffectType(EffectTypes.Ale_3pm, "Effect of the reward if type is EmbarkEffectMetaReward")]
    public string effect;
    
    [SchemaGoodType("Good given when the game starts if type is EmbarkGoodMetaReward")]
    public string good;
    
    [SchemaField(1, false, "Amount of good given if type is EmbarkGoodMetaReward")]
    public int goodAmount;
    
    [SchemaField(0, false, "Minimum cost of the reward")]
    public int minCost;
    
    [SchemaField(0, false, "Maximum cost of the reward")]
    public int maxCost;


    public void Initialize()
    {
        displayName = new LocalizableField("displayName");
        description = new LocalizableField("description");
    }
}