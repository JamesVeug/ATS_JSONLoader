
using ATS_JSONLoader;
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

        public static void InitializeConfigs(ConfigFile config)
        {
            configFile = config;

            verboseLogging = config.Bind("Debugging", "Verbose Logging", false, 
                "When set to true JSONLoader will produce a lot more logs to assist with debugging why something isn't working.");
            
            export = config.Bind("Exporting", "Export On Game Load", false, 
                $"When set to true JSONLoader will export as much data as it can to '{Plugin.ExportDirectory}'.");
        }
    }
}