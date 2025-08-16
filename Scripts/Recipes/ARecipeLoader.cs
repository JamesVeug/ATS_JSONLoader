using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATS_API.Helpers;
using ATS_JSONLoader;
using Eremite.Buildings;
using TinyJson;
using PluginInfo = ATS_JSONLoader.PluginInfo;

public abstract class ARecipeData : IInitializable
{
    [SchemaGuid]
    public string guid;
    
    [SchemaName]
    public string name;
    
    [SchemaEnum<Grade>(Grade.One, "The grade of the recipe")] 
    public string grade;
    
    [SchemaEnum<TagTypes>(TagTypes.Building_Material_Tag, "Tags of which some effects can affect this recipe by")]
    public string[] tags;

    public virtual void Initialize()
    {
        
    }
}

public abstract class ARecipeLoader<ModelType, DataType> 
    where ModelType : RecipeModel
    where DataType : ARecipeData
{
    
    public abstract string FileExtension { get; }
    public abstract string Category { get; }
    public abstract IEnumerable<ModelType> AllModels { get; }
    
    public abstract ModelType CreateNewModel(string guid, string name);
    public abstract bool GetNewData(string name, out string rawName, out string guid);
    
    public virtual void LoadAll(List<string> files)
    {
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            if (!file.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            ImportExportUtils.SetDebugPath(file);
            files.RemoveAt(i--);
            
            try
            {
                Logging.VerboseLog($"Loading JSON ({Category}) {file}");
                DataType data = JSONParser.FromFilePath<DataType>(file);
                if (data == null)
                {
                    Plugin.Log.LogError($"Failed to load JSON ({Category}) {file}");
                    continue;
                }
                Logging.VerboseLog($"Loaded JSON ({Category}) {file}");

                // If we did not specify a GUID and this is NOT a vanilla item then use a default guid.
                data.guid ??= AllModels.Any(a=>a.name == data.name) ? "" : PluginInfo.PLUGIN_GUID;
                
                string guidPrefix = !string.IsNullOrEmpty(data.guid) ? data.guid + "_" : "";
                string fullName = guidPrefix + data.name;

                ModelType model = null;
                if (AllModels.Any(a=>a.name == fullName))
                {
                    Logging.VerboseLog($"Found existing {Category} {fullName}");
                    model = AllModels.First(a=>a.name == fullName);
                }
                else
                {
                    Logging.VerboseLog($"Creating new {Category} {fullName}");
                    model = CreateNewModel(data.guid, data.name);
                }
                
                Logging.VerboseLog($"Applying JSON ({Category}) {file} to {Category} {fullName}");
                Apply(model, data, true, fullName);

                Logging.VerboseLog($"Loaded JSON {Category} {fullName}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Error loading JSON ({Category}) {file}\n{e}");
            }
        }
    }

    public virtual void Apply(ModelType model, DataType data, bool toModel, string modelName)
    {
        ImportExportUtils.SetID(modelName);

        Logging.VerboseLog($"Applying JSON ({Category}) {modelName}");
        ImportExportUtils.ApplyValueNoNull(ref model.grade, ref data.grade, toModel, Category, "grade");
        ImportExportUtils.ApplyValueNoNull(ref model.tags, ref data.tags, toModel, Category, "tags");
    }
    
    public virtual void ExportAll()
    {
        Plugin.Log.LogInfo($"Exporting {AllModels.Count()} {Category}s.");
        foreach (ModelType model in AllModels)
        {
            DataType data = Activator.CreateInstance<DataType>();
            data.Initialize();

            if (GetNewData(model.name, out string rawName, out string guid))
            {
                data.guid = guid;
                data.name = rawName;
            }
            else
            {
                data.name = model.name;
            }
            
            Apply(model, data, false, model.name);
            
            string file = Path.Combine(Plugin.ExportDirectory, Category, model.name + FileExtension);
            if(Directory.Exists(Path.GetDirectoryName(file)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }
            
            string json = JSONParser.ToJSON(data);
            File.WriteAllText(file, json);
        }
    }
}

