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
        ImportExportUtils.ApplyValueNoNull(ref model.racialHousingNeed, ref data.racialHousingNeed, toModel, "races", "racialHousingNeed");
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
                    charModel.effect = charData.villagerPerkEffect.ToVillagerPerkModel();
                    charModel.globalEffect = charData.globalEffect.ToEffectModel();
                    charModel.buildingPerk = charData.buildingPerk.ToBuildingPerkModel();
                    model.characteristics[i] = charModel;
                }
            }
        }
        else
        {
            data.characteristics = new RaceCharacteristicData[model.characteristics.Length];
            Plugin.Log.LogInfo($"{modelName} Characteristics: {model.characteristics.Length}");
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
            string json = JSONParser.ToJSON(data);
            File.WriteAllText(file, json);
        }
    }

    public class RaceCharacteristicData
    {
        public string buildingTag;
        public string villagerPerkEffect;
        public string globalEffect;
        public string buildingPerk;
    }

    public class RaceData : IInitializable
    {
        public string guid;
        public string name;
        public string icon;
        public string roundIcon;
        public string widePortrait;
        public string tag;
        public bool? isEssential;
        public int? order;

        public float? baseSpeed;
        public float? initialResolve;
        public float? minResolve;
        public float? maxResolve;
        public float? resolvePositveChangePerSec;
        public float? resolveNegativeChangePerSec;
        public float? resolveNegativeChangeDiffFactor;
        public float? reputationPerSec;
        public int? minPopulationToGainReputation;
        public float? maxReputationFromResolvePerSec;
        public float? minResolveForReputationTreshold;
        public float? maxResolveForReputationTreshold;
        public float? reputationTresholdIncreasePerReputation;
        public float? resolveToReputationRatio;
        public float? populationToReputationRatio;
        public int? hungerTolerance;
        public string racialHousingNeed;
        public float? needsInterval;
        public string[] needs;
        public RaceCharacteristicData[] characteristics;

        public RacialSounds avatarClickSounds;
        public RacialSounds femalePickSounds;
        public RacialSounds malePickSounds;
        public RacialSounds femaleChangeProfessionSounds;
        public RacialSounds maleChangeProfessionSounds;

        public string[] maleNames;
        public string[] femaleNames;
        
        public void Initialize()
        {

        }
    }
}