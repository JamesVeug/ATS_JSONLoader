using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using ATS_JSONLoader;
using Eremite;
using Eremite.Buildings;
using TinyJson;

public abstract class ABuildingData : IInitializable
{
    [SchemaGuid]
    public string guid;
    
    [SchemaName]
    public string name;
    
    [SchemaIcon(TextureHelper.SpriteType.BuildingIcon)] 
    public string icon;
    
    [SchemaDisplayName] 
    public LocalizableField displayName;
    
    [SchemaDescription] 
    public LocalizableField description;

    public virtual void Initialize()
    {
        displayName = new LocalizableField("displayName");
        description = new LocalizableField("description");
    }
}

public abstract class ABuildingLoader<ModelType,DataType> 
    where ModelType : BuildingModel
    where DataType : ABuildingData
{
    public abstract string FileExtension { get; }
    public abstract IEnumerable<ModelType> AllModels { get; }
    public abstract ModelType CreateNewModelModel(string guid, string name);
    public abstract bool GetNewData(string name, out string rawName, out string guid);
    
    
    public virtual void LoadAll(List<string> files)
    {
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            if (!file.EndsWith(FileExtension))
            {
                continue;
            }
            
            ImportExportUtils.SetDebugPath(file);
            files.RemoveAt(i--);
            
            try
            {
                Logging.VerboseLog($"Loading JSON ({typeof(ModelType).Name}) {file}");
                DataType data = JSONParser.FromFilePath<DataType>(file);
                if (data == null)
                {
                    Plugin.Log.LogError($"Failed to load JSON ({typeof(ModelType).Name}) {file}");
                    continue;
                }
                Logging.VerboseLog($"Loaded JSON ({typeof(ModelType).Name}) {file}");

                // If we did not specify a GUID and this is NOT a vanilla item then use a default guid.
                data.guid ??= AllModels.Any(a=>a.name == data.name) ? "" : PluginInfo.PLUGIN_GUID;
                
                string guidPrefix = !string.IsNullOrEmpty(data.guid) ? data.guid + "_" : "";
                string fullName = guidPrefix + data.name;

                bool isNewGood = false;
                ModelType model = null;
                if (AllModels.Any(a=>a.name == fullName))
                {
                    Logging.VerboseLog($"Found existing {typeof(DataType).Name} {fullName}");
                    model = AllModels.First(a=>a.name == fullName);
                }
                else
                {
                    Logging.VerboseLog($"Creating new {typeof(DataType).Name} {fullName}");
                    model = CreateNewModelModel(data.guid, data.name);
                    isNewGood = true;
                }
                
                Logging.VerboseLog($"Applying JSON ({typeof(ModelType).Name}) {file} to {typeof(DataType).Name} {fullName}");
                Apply(model, data, true, fullName, isNewGood);

                Logging.VerboseLog($"Loaded JSON {typeof(DataType).Name} {fullName}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Error loading JSON ({typeof(ModelType).Name}) {file}\n{e}");
            }
        }
    }
    
    public abstract void Apply(ModelType model, DataType data, bool toModel, string modelName, bool isNewGood);

    
    public virtual void ExportAll()
    {
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
            
            Apply(model, data, false, model.name, false);
            
            string folder = char.ToUpper(FileExtension[1]) + FileExtension.Substring(2, FileExtension.LastIndexOf(".") - 2) + "s";
            string file = Path.Combine(Plugin.ExportDirectory, folder, model.name + FileExtension);
            if (Directory.Exists(Path.GetDirectoryName(file)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }
            
            string json = JSONParser.ToJSON(data);
            File.WriteAllText(file, json);
        }
    }
}
