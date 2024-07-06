
using BepInEx.Configuration;

namespace JLPlugin
{
    internal static class Configs
    {
        internal static bool VerboseLogging => verboseLogging.Value;
        internal static bool Export => export.Value;

        private static ConfigEntry<bool> export;
        private static ConfigEntry<bool> verboseLogging;
        private static ConfigFile configFile;

        // private static Version oldConfigVersion;
        // private static Version currentVersion;

        public static void InitializeConfigs(ConfigFile config)
        {
            configFile = config;
            // currentVersion = new Version(PluginInfo.PLUGIN_NAME);
            // oldConfigVersion = GetOldConfigVersion();

            verboseLogging = config.Bind("Debugging", "Verbose Logging", false, "Set to true to see more logs on what JSONLoader is doing and what isn't working.");
            export = config.Bind("Exporting", "Export On Game Load", false, "When set to true JSONLoader will export as much data as it can to the plugins/JSONLoader/Examples/Export folder.");

            // MigrateConfigs();
            // ModdedSaveManager.SaveData.SetValue(Plugin.PluginGuid, "LastLoadedVersion", currentVersion.ToString());
        }

        // private static Version GetOldConfigVersion()
        // {
        //     string oldVersion = ModdedSaveManager.SaveData.GetValue(Plugin.PluginGuid, "LastLoadedVersion");
        //     if (string.IsNullOrEmpty(oldVersion))
        //     {
        //         // Configs exist but they deleted their save data!
        //         // This means their configs need verifying
        //         return new Version("2.5.2"); // 2.5.3 we added the save data so this is our earliest migration version
        //     }
        //
        //     // Configs and save up to date. Migrate if the versions do not match!
        //     return new Version(oldVersion); // Everything up to date!
        // }
        //
        // private static void MigrateConfigs()
        // {
        //     if (oldConfigVersion == currentVersion)
        //     {
        //         // Nothing to migrate!
        //         return;
        //     }
        //
        //     if (oldConfigVersion <= new Version("2.5.3"))
        //     {
        //         Plugin.Log.LogInfo($"Migrating from {oldConfigVersion} to {currentVersion}!");
        //         if (ReloadHotkey == (string)exportHotkey.DefaultValue &&
        //             ExportHotkey == (string)reloadHotkey.DefaultValue)
        //         {
        //             Plugin.Log.LogInfo($"\tMigrating hotkeys to new defaults!");
        //             exportHotkey.Value = (string)exportHotkey.DefaultValue;
        //             reloadHotkey.Value = (string)reloadHotkey.DefaultValue;
        //             configFile.Save();
        //         }
        //     }
        // }
    }
}