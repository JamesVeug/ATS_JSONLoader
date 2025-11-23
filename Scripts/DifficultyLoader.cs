using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATS_API.Ascension;
using ATS_API.Difficulties;
using ATS_API.Helpers;
using ATS_JSONLoader;
using Eremite;
using Eremite.Model;
using TinyJson;
using PluginInfo = ATS_JSONLoader.PluginInfo;

public class DifficultyLoader
{
    public const string fileExtension = "_difficulty.json";
    
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
                Logging.VerboseLog($"Loading JSON (difficulties) {file}");
                DifficultyData data = JSONParser.FromFilePath<DifficultyData>(file);
                if (data == null)
                {
                    Plugin.Log.LogError($"Failed to load JSON (difficulties) {file}");
                    continue;
                }
                Logging.VerboseLog($"Loaded JSON (difficulties) {file}");

                // If we did not specify a GUID and this is NOT a vanilla item then use a default guid.
                data.guid ??= MB.Settings.difficulties.Any(a=>a.name == data.name) ? "" : PluginInfo.PLUGIN_GUID;
                
                string guidPrefix = !string.IsNullOrEmpty(data.guid) ? data.guid + "_" : "";
                string fullName = guidPrefix + data.name;

                bool isNewDifficulty = false;
                DifficultyModel model = null;
                if (MB.Settings.difficulties.Any(a=>a.name == fullName))
                {
                    Logging.VerboseLog($"Found existing difficulty {fullName}");
                    model = MB.Settings.difficulties.First(a=>a.name == fullName);
                }
                else
                {
                    Logging.VerboseLog($"Creating new difficulty {fullName}");
                    DifficultyBuilder difficultiesBuilder = new DifficultyBuilder(data.guid, data.name);
                    model = difficultiesBuilder.Model;
                    isNewDifficulty = true;
                }
                
                Logging.VerboseLog($"Applying JSON (difficulties) {file} to difficulty {fullName}");
                Apply(model, data, true, fullName, isNewDifficulty);

                Logging.VerboseLog($"Loaded JSON difficulty {fullName}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Error loading JSON (difficulties) {file}\n{e}");
            }
        }
    }

    public static void Apply(DifficultyModel model, DifficultyData data, bool toModel, string modelName, bool isNewDifficulty)
    {
        ImportExportUtils.SetID(modelName);

        DifficultyBuilder builder = new DifficultyBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.shortName, ref data.shortName, toModel, "difficulties", "shortName");
        ImportExportUtils.ApplyValueNoNull(ref model.index, ref data.index, toModel, "difficulties", "index");
        ImportExportUtils.ApplyValueNoNull(ref model.ascensionIndex, ref data.ascensionIndex, toModel, "difficulties", "ascensionIndex");
        ImportExportUtils.ApplyValueNoNull(ref model.sealFramentsForWin, ref data.sealFragmentsForWin, toModel, "difficulties", "sealFragmentsForWin");
        ImportExportUtils.ApplyValueNoNull(ref model.isAscension, ref data.isAscension, toModel, "difficulties", "eatable");
        ImportExportUtils.ApplyValueNoNull(ref model.canBePicked, ref data.canBePicked, toModel, "difficulties", "canBePicked");
        ImportExportUtils.ApplyValueNoNull(ref model.isInCustomGame, ref data.isInCustomGame, toModel, "difficulties", "isInCustomGame");
        ImportExportUtils.ApplyValueNoNull(ref model.blightFootprintRate, ref data.blightFootprintRate, toModel, "difficulties", "blightFootprintRate");
        ImportExportUtils.ApplyValueNoNull(ref model.blightCorruptionRate, ref data.blightCorruptionRate, toModel, "difficulties", "blightCorruptionRate");
        ImportExportUtils.ApplyValueNoNull(ref model.rewardsMultiplier, ref data.rewardsMultiplier, toModel, "difficulties", "rewardsMultiplier");
        ImportExportUtils.ApplyValueNoNull(ref model.expMultiplier, ref data.expMultiplier, toModel, "difficulties", "expMultiplier");
        ImportExportUtils.ApplyValueNoNull(ref model.scoreMultiplier, ref data.scoreMultiplier, toModel, "difficulties", "scoreMultiplier");
        ImportExportUtils.ApplyValueNoNull(ref model.difficultyBudget, ref data.difficultyBudget, toModel, "difficulties", "difficultyBudget");
        ImportExportUtils.ApplyValueNoNull(ref model.positiveEffects, ref data.positiveEffects, toModel, "difficulties", "positiveEffects");
        ImportExportUtils.ApplyValueNoNull(ref model.negativeEffects, ref data.negativeEffects, toModel, "difficulties", "negativeEffects");
        ImportExportUtils.ApplyValueNoNull(ref model.minEffectCost, ref data.minEffectCost, toModel, "difficulties", "minEffectCost");
        ImportExportUtils.ApplyValueNoNull(ref model.maxEffectCost, ref data.maxEffectCost, toModel, "difficulties", "maxEffectCost");
        ImportExportUtils.ApplyValueNoNull(ref model.preparationPointsPenalty, ref data.preparationPointsPenalty, toModel, "difficulties", "preparationPointsPenalty");
        ImportExportUtils.ApplyValueNoNull(ref model.portRequirementsRatio, ref data.portRequirementsRatio, toModel, "difficulties", "portRequirementsRatio");
        ImportExportUtils.ApplyValueNoNull(ref model.maxWildcards, ref data.maxWildcards, toModel, "difficulties", "maxWildcards");
        ImportExportUtils.ApplyValueNoNull(ref model.inGameSeal, ref data.inGameSeal, toModel, "difficulties", "inGameSeal");
        ImportExportUtils.ApplyValueNoNull(ref model.modifiers, ref data.modifiers, toModel, "difficulties", "modifiers");

        if (toModel)
        {
            if (data.prestigeLevel.HasValue)
            {
                builder.SetPrestigeLevel(data.prestigeLevel.Value);
            }
            if (data.copyModifiersFromPreviousDifficulties.HasValue)
            {
                builder.CopyModifiersFromPreviousDifficulties(data.copyModifiersFromPreviousDifficulties.Value);
            }
            if (data.newModifiers != null)
            {
                for (var i = 0; i < data.newModifiers.Length; i++)
                {
                    var newModifier = data.newModifiers[i];
                    bool isShown = newModifier.isShown ?? true;
                    bool isEarlyEffect = newModifier.isEarlyEffect ?? false;
                    bool inCustomMode = newModifier.inCustomMode ?? true;
                    NewAscensionModifierModel modifier = builder.AddModifier(newModifier.effect.ToEffectTypes(), isShown, isEarlyEffect, inCustomMode);
                    ImportExportUtils.ApplyLocaText(ref modifier.ShortDescription, ref newModifier.shortDescription, (a, b) => modifier.SetShortDescription(a, b), toModel, $"newModifiers[{i}].shortDescription");
                }
            }
        }
        else
        {
            data.prestigeLevel = model.ascensionIndex + 1;
            data.copyModifiersFromPreviousDifficulties = builder.NewDifficulty.copyModifiersFromPreviousDifficulty;
            // data.newModifiers
        }
        

        if (toModel)
        {
            // Data -> Model
            if (!isNewDifficulty)
            {
                if (!string.IsNullOrEmpty(data.icon))
                {
                    ImportExportUtils.ApplyProperty(() => { return model.icon; }, (a) => { builder.SetIcon(a); },
                        ref data.icon,
                        toModel, "difficulties", "icon");
                }
            }
        }
        else
        {
            // Model -> Data
            ImportExportUtils.ApplyProperty(() => { return model.icon; }, (a) => { builder.SetIcon(a); }, ref data.icon,
                toModel, "difficulties", "icon");
        }
    }
    
    public static void ExportAll()
    {
        Plugin.Log.LogInfo($"Exporting {MB.Settings.difficulties.Length} Difficulties.");
        foreach (DifficultyModel model in MB.Settings.difficulties)
        {
            DifficultyTypes type = model.name.ToDifficultyTypes();
            DifficultyData data = new DifficultyData();
            data.Initialize();

            bool isNewDifficulty = DifficultyManager.NewDifficultiesLookup.TryGetValue(type, out var newModel);
            if (isNewDifficulty)
            {
                data.guid = newModel.guid;
                data.name = newModel.rawName;
            }
            else
            {
                data.name = model.name;
            }
            
            Apply(model, data, false, model.name, isNewDifficulty);
            
            string file = Path.Combine(Plugin.ExportDirectory, "Difficulties", model.name + fileExtension);
            if(Directory.Exists(Path.GetDirectoryName(file)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }
            
            string json = JSONParser.ToJSON(data);
            File.WriteAllText(file, json);
        }
    }
}

[GenerateSchema("Difficulty", "Difficulty when starting a settlement.", DifficultyLoader.fileExtension)]
public class DifficultyData : IInitializable
{
    [SchemaGuid] 
    public string guid;
    
    [SchemaName] 
    public string name;
    
    [SchemaIcon(TextureHelper.SpriteType.DifficultyIcon)]
    public string icon;
    
    [SchemaDisplayName]
    public LocalizableField displayName;
    
    [SchemaDescription]
    public LocalizableField description;
    
    [SchemaShortName]
    public string shortName;
    
    [SchemaField(0, false, "Position in the list of difficulties. Starts at 0 for the first difficulty.")] 
    public int? index;
    
    [SchemaField(1, false, "Position in the list of ascension difficulties. 0 if not prestige difficulty or first prestige in the list.")] 
    public int? ascensionIndex;
    
    [SchemaField(5, false, "Amount of seal fragments earned when winning the game with this difficulty selected.")] 
    public int? sealFragmentsForWin;
    
    [SchemaField(false, false, "When set to true will set this difficulty to be a prestige difficulty.")] 
    public bool? isAscension;
    
    [SchemaField(true, false, "When set to false will hide this difficulty from the list of difficulties.")] 
    public bool? canBePicked;
    
    [SchemaField(true, false, "When set to false will hide this difficulty from the list of difficulties in custom games.")] 
    public bool? isInCustomGame;
    
    [SchemaField(1f)] 
    public float? blightFootprintRate;
    
    [SchemaField(1f)] 
    public float? blightCorruptionRate;
    
    [SchemaField(5.1f)] 
    public float? rewardsMultiplier;
    
    [SchemaField(5.1f)] 
    public float? expMultiplier;
    
    [SchemaField(2.1f)] 
    public float? scoreMultiplier;
    
    [SchemaField(6)] 
    public int? difficultyBudget;
    
    [SchemaField(1)] 
    public int? positiveEffects;
    
    [SchemaField(4)] 
    public int? negativeEffects;
    
    [SchemaField(-1)] 
    public int? minEffectCost;
    
    [SchemaField(3)] 
    public int? maxEffectCost;
    
    [SchemaField(-4)] 
    public int? preparationPointsPenalty;
    
    [SchemaField(1)] 
    public float? portRequirementsRatio;
    
    [SchemaField(1)] 
    public int? maxWildcards;
    
    [SchemaField("Seal")] 
    public string inGameSeal;
    
    [SchemaAscensionModifierType("List of effects that are applied to this difficulty.")] 
    public string[] modifiers;

    [SchemaField(1, false, "Prestige level of the difficulty. Overrides everything in the Difficulty to set the prestige level.")]
    public int? prestigeLevel;
    
    [SchemaField(true, false, "When set to true will copy the modifiers from the lower level difficulties.")]
    public bool? copyModifiersFromPreviousDifficulties;
    
    [SchemaField(null, false, "List of new modifiers that are created and applied to this difficulty.")]
    public NewModifier[] newModifiers;

    public class NewModifier : IInitializable
    {
        [SchemaShortDescription]
        public LocalizableField shortDescription;
        
        [SchemaEffectType(EffectTypes.Corrupted_Sacrifice, "Effect that is applied to the difficulty.")]
        public string effect;
        
        [SchemaField(true, false, "When set to true will apply the effect to the difficulty.")]
        public bool? isEarlyEffect;
        
        [SchemaField(true, false, "When set to false will hide the effect from the list of effects in custom games.")]
        public bool? isShown;
        
        [SchemaField(true, false, "When set to false will hide the effect from the list of effects in custom games.")]
        public bool? inCustomMode;
        
        public void Initialize()
        {
            shortDescription = new LocalizableField("shortDescription");
        }
    }
    

    public void Initialize()
    {
        displayName = new LocalizableField("displayName");
        description = new LocalizableField("description");
    }
}