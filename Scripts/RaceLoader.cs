using System;
using System.Collections.Generic;
using System.IO;
using ATS_JSONLoader;
using ATS_JSONLoader.Sounds;
using Eremite;
using Eremite.Model;
using TinyJson;

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

    public class RaceData : IInitializable
    {
        public string guid;
        public string name;
        public string icon;
        public string roundIcon;

        public RacialSounds avatarClickSounds = new RacialSounds();
        public RacialSounds femalePickSounds = new RacialSounds();
        public RacialSounds malePickSounds = new RacialSounds();
        public RacialSounds femaleChangeProfessionSounds = new RacialSounds();
        public RacialSounds maleChangeProfessionSounds = new RacialSounds();

        public void Initialize()
        {
            avatarClickSounds = new RacialSounds();
            femalePickSounds = new RacialSounds();
            malePickSounds = new RacialSounds();
            femaleChangeProfessionSounds = new RacialSounds();
            maleChangeProfessionSounds = new RacialSounds();
        }
    }
}