using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATS_API;
using BepInEx;
using BepInEx.Logging;
using Eremite;
using Eremite.Controller;
using HarmonyLib;
using JLPlugin;
using UnityEngine;

namespace ATS_JSONLoader;

[HarmonyPatch]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("API", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
    public static string PluginDirectory;
        
    public static Plugin Instance;
    public static ManualLogSource Log;
    
    public static string JSONLoaderDirectory = "";
    public static string BepInExDirectory = "";
    public static string ExportDirectory => Path.Combine(Application.persistentDataPath, "JSONLoader", "Exported");

    private Harmony harmony;
    
    private static List<string> GetAllJLDRFiles()
    {
        string exportedFolder = Path.Combine(JSONLoaderDirectory, "Exported");
        string examplesFolder = Path.Combine(JSONLoaderDirectory, "Examples");
        return Directory.GetFiles(Paths.PluginPath, "*.json", SearchOption.AllDirectories)
            .Where(a=> !a.Contains(exportedFolder) && !a.Contains(examplesFolder))
            .ToList();
    }

    private void Awake()
    {
        Logger.LogInfo($"Loading JSONLoader!");
        Instance = this;
        Log = Logger;
        JSONLoaderDirectory = Path.GetDirectoryName(Info.Location);

        // Stops Unity from destroying it for some reason. Same as Setting the BepInEx config HideManagerGameObject to true.
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        harmony = Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, PluginInfo.PLUGIN_GUID);
        
        int bepInExIndex = Info.Location.LastIndexOf("BepInEx");
        if (bepInExIndex > 0)
        {
            BepInExDirectory = Info.Location.Substring(0, bepInExIndex);
        }
        else
        {
            BepInExDirectory = Directory.GetParent(JSONLoaderDirectory)?.FullName ?? "";
        }
        
        Configs.InitializeConfigs(Config);
        
        Hotkeys.RegisterKey(PluginInfo.PLUGIN_NAME, "reload", "Reload all JSON Files", [KeyCode.F5], () =>
        {
            Logger.LogInfo($"Reloading JSONLoader!");
            LoadAllFiles();
        });

        Logger.LogInfo($"Loaded JSONLoader!");
    }

    [HarmonyPatch(typeof(MainController), nameof(MainController.OnServicesReady))]
    [HarmonyPostfix]
    private static void HookMainControllerSetup()
    {
        // Too difficult to predict when GameController will exist and I can hook observers to it
        // So just use Harmony and save us all some time. This method will run after every game start
        var isNewGame = MB.GameSaveService.IsNewGame();
        Instance.Logger.LogInfo($"Entered a game. Is this a new game: {isNewGame}.");

        // Wait 1 frame because most mods add content in this method
        // And some JSON files will modify these
        Instance.StartCoroutine(GameLoaded());
    }

    private static IEnumerator GameLoaded()
    {
        yield return new WaitForEndOfFrame();
        
        LoadAllFiles();
        
        if (Configs.Export)
        {
            ExportAllFiles();
        }
    }

    private static void LoadAllFiles()
    {
        List<string> files = GetAllJLDRFiles();
        if (Configs.VerboseLogging)
        {
            Log.LogInfo("Loading all JSON files:" + string.Join(", ", files.Select(Path.GetFileName)));
        }

        GoodsLoader.LoadAll(files);
        RaceLoader.LoadAll(files);
        DifficultyLoader.LoadAll(files);
        MetaRewardLoader.LoadAll(files);
        
        // Recipes
        new WorkshopRecipeLoader().LoadAll(files);
        new InstitutionRecipeLoader().LoadAll(files);
        new CampRecipeLoader().LoadAll(files);
        new FishingHutRecipeLoader().LoadAll(files);
        new FarmRecipeLoader().LoadAll(files);
        new GathererHutRecipeLoader().LoadAll(files);
        new MineRecipeLoader().LoadAll(files);
        new RainCatcherRecipeLoader().LoadAll(files);
        new CollectorRecipeLoader().LoadAll(files);
        
        
        // Buildings
        new WorkshopBuildingLoader().LoadAll(files);
        new HouseBuildingLoader().LoadAll(files);
        new DecorationBuildingLoader().LoadAll(files);
        new InstitutionBuildingLoader().LoadAll(files);
        new FishingHutBuildingLoader().LoadAll(files);
        new GathererHutBuildingLoader().LoadAll(files);
        new CampBuildingLoader().LoadAll(files);
        new HearthBuildingLoader().LoadAll(files);
        
    }

    private static void ExportAllFiles()
    {
        ImportExportUtils.SetDebugPath("");
        if (!Directory.Exists(ExportDirectory))
        {
            Directory.CreateDirectory(ExportDirectory);
        }

        Log.LogInfo($"Exporting all files to {ExportDirectory}... Grab a coffee... this will take a long time.");
        JSONSchemaGenerator.GenerateAndExport();
        GoodsLoader.ExportAll();
        RaceLoader.ExportAll();
        DifficultyLoader.ExportAll();
        MetaRewardLoader.ExportAll();
        
        // Recipe
        new WorkshopRecipeLoader().ExportAll();
        new InstitutionRecipeLoader().ExportAll();
        new CampRecipeLoader().ExportAll();
        new FishingHutRecipeLoader().ExportAll();
        new FarmRecipeLoader().ExportAll();
        new GathererHutRecipeLoader().ExportAll();
        new MineRecipeLoader().ExportAll();
        new RainCatcherRecipeLoader().ExportAll();
        new CollectorRecipeLoader().ExportAll();
        
        // Buildings
        new WorkshopBuildingLoader().ExportAll();
        new HouseBuildingLoader().ExportAll();
        new DecorationBuildingLoader().ExportAll();
        new InstitutionBuildingLoader().ExportAll();
        new FishingHutBuildingLoader().ExportAll();
        new GathererHutBuildingLoader().ExportAll();
        new CampBuildingLoader().ExportAll();
        new HearthBuildingLoader().ExportAll();
    }
}