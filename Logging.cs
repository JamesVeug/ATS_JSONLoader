using ATS_API;
using JLPlugin;

namespace ATS_JSONLoader;

public class Logging
{
    internal static void VerboseLog(string s)
    {
        if (Configs.VerboseLogging)
            Plugin.Log.LogInfo(s);
    }

    internal static void VerboseWarning(string s)
    {
        if (Configs.VerboseLogging)
            Plugin.Log.LogWarning(s);
    }

    internal static void VerboseError(string s)
    {
        if (Configs.VerboseLogging)
            Plugin.Log.LogError(s);
    }
}