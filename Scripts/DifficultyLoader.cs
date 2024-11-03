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
    public static void LoadAll(List<string> files)
    {
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            if (!file.EndsWith("_difficulty.json"))
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
            
            string file = Path.Combine(Plugin.ExportDirectory, "difficulties", model.name + "_difficulty.json");
            if(Directory.Exists(Path.GetDirectoryName(file)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }
            
            string json = JSONParser.ToJSON(data);
            File.WriteAllText(file, json);
        }
    }
}

public class DifficultyData : IInitializable
{
    public string guid;
    public string name;
    public string icon;
    public LocalizableField displayName;
    public LocalizableField description;
    public string shortName;
    public int? index;
    public int? ascensionIndex;
    public int? sealFragmentsForWin;
    public bool? isAscension;
    public bool? canBePicked;
    public bool? isInCustomGame;
    public float? blightFootprintRate;
    public float? blightCorruptionRate;
    public float? rewardsMultiplier;
    public float? expMultiplier;
    public float? scoreMultiplier;
    public int? difficultyBudget;
    public int? positiveEffects;
    public int? negativeEffects;
    public int? minEffectCost;
    public int? maxEffectCost;
    public int? preparationPointsPenalty;
    public float? portRequirementsRatio;
    public int? maxWildcards;
    public string inGameSeal;
    public string[] modifiers;

    public int? prestigeLevel;
    public bool? copyModifiersFromPreviousDifficulties;
    public NewModifier[] newModifiers;

    public class NewModifier : IInitializable
    {
        public LocalizableField shortDescription;
        public string effect;
        public bool? isEarlyEffect;
        public bool? isShown;
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