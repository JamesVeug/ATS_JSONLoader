using System;
using System.Collections.Generic;
using System.IO;
using ATS_API.Helpers;
using ATS_JSONLoader;
using ATS_JSONLoader.Sounds;
using Eremite;
using Eremite.Model;
using TinyJson;
using UnityEngine;

public class RaceLoader
{
    public static void LoadAll(List<string> files)
    {
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            if (!file.EndsWith("_race.json"))
            {
                continue;
            }

            ImportExportUtils.SetDebugPath(file);
            files.RemoveAt(i--);

            try
            {
                Logging.VerboseLog($"Loading JSON (goods) {file}");
                RaceData data = JSONParser.FromFilePath<RaceData>(file);
                if (data == null)
                {
                    Plugin.Log.LogError($"Failed to load JSON (race) {file}");
                    continue;
                }

                Logging.VerboseLog($"Loaded JSON (race) {file}");

                // If we did not specify a GUID and this is NOT a vanilla item then use a default guid.
                data.guid ??= MB.Settings.ContainsRace(data.name) ? "" : PluginInfo.PLUGIN_GUID;

                string guidPrefix = !string.IsNullOrEmpty(data.guid) ? data.guid + "_" : "";
                string fullName = guidPrefix + data.name;

                RaceModel model = null;
                if (MB.Settings.ContainsRace(fullName))
                {
                    Logging.VerboseLog($"Found existing race {fullName}");
                    model = MB.Settings.GetRace(fullName);
                }
                else
                {
                    Plugin.Log.LogError($"Custom races not yet supported! for name {fullName}");
                    continue;
                }

                Logging.VerboseLog($"Applying JSON (race) {file} to race {fullName}");
                Apply(model, data, true, fullName);

                Logging.VerboseLog($"Loaded JSON race {fullName}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Error loading JSON (race) {file}\n{e}");
            }
        }
    }

    public static void Apply(RaceModel model, RaceData data, bool toModel, string modelName)
    {
        ImportExportUtils.SetID(modelName);

        ImportExportUtils.ApplyValueNoNull(ref model.icon, ref data.icon, toModel, "races", "icon");
        ImportExportUtils.ApplyValueNoNull(ref model.roundIcon, ref data.roundIcon, toModel, "races", "roundIcon");
        ImportExportUtils.ApplyValueNoNull(ref model.widePortrait, ref data.widePortrait, toModel, "races", "widePortrait");
        ImportExportUtils.ApplyValueNoNull(ref model.isEssential, ref data.isEssential, toModel, "races", "isEssential");
        ImportExportUtils.ApplyValueNoNull(ref model.maleNames, ref data.maleNames, toModel, "races", "maleNames");
        ImportExportUtils.ApplyValueNoNull(ref model.femaleNames, ref data.femaleNames, toModel, "races", "femaleNames");
        ImportExportUtils.ApplyValueNoNull(ref model.order, ref data.order, toModel, "races", "order");
        ImportExportUtils.ApplyValueNoNull(ref model.tag, ref data.tag, toModel, "races", "widePortrait");
        ImportExportUtils.ApplyValueNoNull(ref model.baseSpeed, ref data.baseSpeed, toModel, "races", "baseSpeed");
        ImportExportUtils.ApplyValueNoNull(ref model.initialResolve, ref data.initialResolve, toModel, "races", "initialResolve");
        ImportExportUtils.ApplyValueNoNull(ref model.minResolve, ref data.minResolve, toModel, "races", "minResolve");
        ImportExportUtils.ApplyValueNoNull(ref model.maxResolve, ref data.maxResolve, toModel, "races", "maxResolve");
        ImportExportUtils.ApplyValueNoNull(ref model.resolvePositveChangePerSec, ref data.resolvePositveChangePerSec, toModel, "races", "resolvePositveChangePerSec");
        ImportExportUtils.ApplyValueNoNull(ref model.resolveNegativeChangePerSec, ref data.resolveNegativeChangePerSec, toModel, "races", "resolveNegativeChangePerSec");
        ImportExportUtils.ApplyValueNoNull(ref model.resolveNegativeChangeDiffFactor, ref data.resolveNegativeChangeDiffFactor, toModel, "races", "resolveNegativeChangeDiffFactor");
        ImportExportUtils.ApplyValueNoNull(ref model.reputationPerSec, ref data.reputationPerSec, toModel, "races", "reputationPerSec");
        ImportExportUtils.ApplyValueNoNull(ref model.minPopulationToGainReputation, ref data.minPopulationToGainReputation, toModel, "races", "minPopulationToGainReputation");
        ImportExportUtils.ApplyValueNoNull(ref model.maxReputationFromResolvePerSec, ref data.maxReputationFromResolvePerSec, toModel, "races", "maxReputationFromResolvePerSec");
        ImportExportUtils.ApplyVector2(ref model.resolveForReputationTreshold, ref data.minResolveForReputationTreshold, ref data.maxResolveForReputationTreshold, toModel, "races", "resolveForReputationTreshold");
        ImportExportUtils.ApplyValueNoNull(ref model.reputationTresholdIncreasePerReputation, ref data.reputationTresholdIncreasePerReputation, toModel, "races", "reputationTresholdIncreasePerReputation");
        ImportExportUtils.ApplyValueNoNull(ref model.resolveToReputationRatio, ref data.resolveToReputationRatio, toModel, "races", "resolveToReputationRatio");
        ImportExportUtils.ApplyValueNoNull(ref model.populationToReputationRatio, ref data.populationToReputationRatio, toModel, "races", "populationToReputationRatio");
        ImportExportUtils.ApplyValueNoNull(ref model.hungerTolerance, ref data.hungerTolerance, toModel, "races", "hungerTolerance");
        ImportExportUtils.ApplyValueNoNull(ref model.needs, ref data.needs, toModel, "races", "needs");
        // ImportExportUtils.ApplyValueNoNull(ref model.racialHousingNeed, ref data.racialHousingNeed, toModel, "races", "racialHousingNeed");
        ImportExportUtils.ApplyValueNoNull(ref model.needsInterval, ref data.needsInterval, toModel, "races", "needsInterval");


        ImportExportUtils.ApplyValueNoNull(ref model.avatarClickSound, ref data.avatarClickSounds, toModel, "races", "avatarClickSounds");
        ImportExportUtils.ApplyValueNoNull(ref model.femalePrefab.view.pickSound, ref data.femalePickSounds, toModel, "races", "femalePickSounds");
        ImportExportUtils.ApplyValueNoNull(ref model.femalePrefab.view.professionChangeSound, ref data.femaleChangeProfessionSounds, toModel, "races", "femaleChangeProfessionSounds");
        ImportExportUtils.ApplyValueNoNull(ref model.malePrefab.view.pickSound, ref data.malePickSounds, toModel, "races", "malePickSounds");
        ImportExportUtils.ApplyValueNoNull(ref model.malePrefab.view.professionChangeSound,
            ref data.maleChangeProfessionSounds, toModel, "races", "maleChangeProfessionSounds");

        if (toModel)
        {
            model.avatarClickSound.race = model;
            model.femalePrefab.view.pickSound.race = model;
            model.femalePrefab.view.professionChangeSound.race = model;
            model.malePrefab.view.pickSound.race = model;
            model.malePrefab.view.professionChangeSound.race = model;

            if (data.characteristics != null)
            {
                model.characteristics = new RaceCharacteristicModel[data.characteristics.Length];
                for (int i = 0; i < data.characteristics.Length; i++)
                {
                    RaceCharacteristicData charData = data.characteristics[i];
                    RaceCharacteristicModel charModel = new RaceCharacteristicModel();
                    charModel.tag = charData.buildingTag.ToBuildingTagModel();

                    if (!string.IsNullOrEmpty(charData.villagerPerkEffect))
                    {
                        charModel.effect = charData.villagerPerkEffect.ToVillagerPerkModel();
                    }

                    if (!string.IsNullOrEmpty(charData.globalEffect))
                    {
                        charModel.globalEffect = charData.globalEffect.ToEffectModel();
                    }

                    if (!string.IsNullOrEmpty(charData.buildingPerk))
                    {
                        charModel.buildingPerk = charData.buildingPerk.ToBuildingPerkModel();
                    }

                    model.characteristics[i] = charModel;
                }
            }
        }
        else
        {
            data.characteristics = new RaceCharacteristicData[model.characteristics.Length];
            for (int i = 0; i < model.characteristics.Length; i++)
            {
                RaceCharacteristicModel charModel = model.characteristics[i];
                RaceCharacteristicData charData = new RaceCharacteristicData();
                charData.buildingTag = charModel.tag?.name;
                charData.villagerPerkEffect = charModel.effect?.name;
                charData.globalEffect = charModel.globalEffect?.name;
                charData.buildingPerk = charModel.buildingPerk?.name;
                data.characteristics[i] = charData;
            }
        }
    }

    public static void ExportAll()
    {
        foreach (RaceModel goodModel in MB.Settings.Races)
        {
            RaceData data = new RaceData();
            data.name = goodModel.name; // TODO: Handle new races name and guid similar to goods
            data.Initialize();

            Apply(goodModel, data, false, goodModel.name);

            string file = Path.Combine(Plugin.ExportDirectory, "races", goodModel.name + "_race.json");
            if(Directory.Exists(Path.GetDirectoryName(file)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }
            
            string json = JSONParser.ToJSON(data);
            File.WriteAllText(file, json);
        }
    }

    public class RaceCharacteristicData
    {
        [SchemaBuildingTagType("")]
        public string buildingTag;
        
        [SchemaVillagerPerkTypeType("")]
        public string villagerPerkEffect;
        
        [SchemaEffectType(EffectTypes.Ale_3pm, "")]
        public string globalEffect;
        
        [SchemaBuildingPerkType("")]
        public string buildingPerk;
    }

    [GenerateSchema("Races", "Also known as villagers")]
    public class RaceData : IInitializable
    {
        [SchemaGuid]
        public string guid;
        
        [SchemaName]
        public string name;
        
        [SchemaIcon(TextureHelper.SpriteType.RaceIcon)]
        public string icon;
        
        [SchemaIcon(TextureHelper.SpriteType.RaceIcon, "Same as icon but with transparent background and roudned image.")]
        public string roundIcon;
        
        [SchemaIcon(TextureHelper.SpriteType.RaceIconWide, "Large concept image of the race that will appear when selecting which villagers to join your town.")]
        public string widePortrait;
        
        [SchemaField("")]
        public string tag;
        
        [SchemaField(true, false, "If set to true the villager will always be available in the game.")]
        public bool? isEssential;
        
        [SchemaField(0, false, "Order in which the villager will appear in lists. 0 is minimum")]
        public int? order;

        [SchemaField("1.8f", false, "Base speed the villager can move at")]
        public float? baseSpeed;
        
        [SchemaField(15f, false, "Starting resolve of the villager")]
        public float? initialResolve;
        
        [SchemaField(0f, false, "Minimum resolve the villager can have")]
        public float? minResolve;
        
        [SchemaField(50f, false, "Maximum resolve the villager can have")]
        public float? maxResolve;
        
        [SchemaField(0.15f, false, "Resolve increase per second when the villager is happy")]
        public float? resolvePositveChangePerSec;
        
        [SchemaField(0.12f, false, "Resolve decrease per second when the villager is unhappy")]
        public float? resolveNegativeChangePerSec;
        
        [SchemaField(0.1f, false, "Factor that determines how much faster the resolve decreases when the villager is unhappy")]
        public float? resolveNegativeChangeDiffFactor;
        
        [SchemaField(0.00013f, false, "How much Reputation the player gains per second when the villagers resolve meets the threshold.")]
        public float? reputationPerSec;
        
        [SchemaField(0.025f, false, "Maximum amount of reputation the player gains per second when the villagers resolve meets the threshold.")]
        public float? maxReputationFromResolvePerSec;
        
        [SchemaField("1", false, "Minimum population required to gain reputation")]
        public int? minPopulationToGainReputation;
        
        [SchemaField(30f, false, "Minimum and maximum resolve values for the reputation treshold")]
        public float? minResolveForReputationTreshold;
        
        [SchemaField(50f, false, "Minimum and maximum resolve values for the reputation treshold")]
        public float? maxResolveForReputationTreshold;
        
        [SchemaField(4f)]
        public float? reputationTresholdIncreasePerReputation;
        
        [SchemaField(0.1f)]
        public float? resolveToReputationRatio;
        
        [SchemaField(0.7f)]
        public float? populationToReputationRatio;
        
        [SchemaField(6, false, "How hunger a villager gets before it wants to leave the village.")]
        public int? hungerTolerance;
        
        [SchemaField(120f, false, "How often the villager needs are checked")]
        public float? needsInterval;
        
        [SchemaNeed]
        public string[] needs;
        
        [SchemaField(null, false, "Characteristics of the villager of what they like and are good at.")]
        public RaceCharacteristicData[] characteristics;

        // NOTE: Excluding sounds because this json is already too large for a schema
        public RacialSounds avatarClickSounds;
        public RacialSounds femalePickSounds;
        public RacialSounds malePickSounds;
        public RacialSounds femaleChangeProfessionSounds;
        public RacialSounds maleChangeProfessionSounds;

        [SchemaField(null, false, "All possible names a male villager can have")]
        public string[] maleNames;
        
        [SchemaField(null, false, "All possible names a female villager can have")]
        public string[] femaleNames;
        
        // Removed in ATS v1.5
        // [SchemaField("", false, "name of the 'Housing Need' required to meet the villagers racial housing need")]
        // public string racialHousingNeed;
        
        public void Initialize()
        {

        }
    }
}