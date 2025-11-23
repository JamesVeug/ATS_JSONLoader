using System.Collections.Generic;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;

public class ExtractorBuildingLoader : ABuildingLoader<ExtractorModel, ExtractorBuildingData>
{
    public const string fileExtension = "_ExtractorBuilding.json";
    public override string FileExtension => fileExtension;
    public override string Category => "ExtractorBuilding";
    public override IEnumerable<ExtractorModel> AllModels => MB.Settings.Buildings.Where(a=>a is ExtractorModel).Cast<ExtractorModel>();
    public override bool SupportsNewModels => false;
    
    public override ExtractorModel CreateNewModelModel(string guid, string name)
    {
        return null;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        rawName = null;
        guid = null;
        return false;
    }

    public override void Apply(ExtractorModel model, ExtractorBuildingData data, bool toModel, string modelName, bool isNewGood)
    {
        base.Apply(model, data, toModel, modelName, isNewGood);

        ExtractorBuildingBuilder builder = new ExtractorBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.producedAmount, ref data.producedAmount, toModel, Category, "producedAmount");
        ImportExportUtils.ApplyValueNoNull(ref model.productionTime, ref data.productionTime, toModel, Category, "productionTime");
        ImportExportUtils.ApplyValueNoNull(ref model.baseTankCapacity, ref data.baseTankCapacity, toModel, Category, "baseTankCapacity");
        ImportExportUtils.ApplyValueNoNull(ref model.profession, ref data.profession, toModel, Category, "profession");
        // ImportExportUtils.ApplyValueNoNull(ref model.prefab, ref data.prefab, toModel, Category, "prefab");
    }
}

[GenerateSchema("ExtractorBuilding", "Buildings that are placed on top of Springs/Geysers to extract resources", ExtractorBuildingLoader.fileExtension)]
public class ExtractorBuildingData : ABuildingData
{
    [SchemaField(null)]
    public int producedAmount;
    
    [SchemaField(null)]
    public float productionTime;
    
    [SchemaField(null)]
    public int baseTankCapacity;
    
    [SchemaEnum<ProfessionTypes>(ProfessionTypes.Alchemist, "Which races are good in this building.")]
    public string profession;
    
    [SchemaField(null)]
    public string[] recipes;
    
    [SchemaField(null)]
    public HelperMethods.WorkPlaceData[] workplaces;
}

public class ExtractorBuildingBuilder : BuildingBuilder<ExtractorModel, ExtractorData>
{
    public ExtractorBuildingBuilder(ExtractorModel model) : base(model)
    {
    }

    public ExtractorBuildingBuilder(string guid, string name, string iconPath) : base(guid, name, iconPath)
    {
    }

    protected override ExtractorData GetNewData(ExtractorModel model)
    {
        return new ExtractorData("", model.name, BuildingTypes.Ruined_Fishing_Hut, model, BuildingBehaviourTypes.Hearth);
    }

    protected override ExtractorData CreateNewData(string guid, string name)
    {
        return new ExtractorData(guid, name, BuildingTypes.Ruined_Fishing_Hut, null, BuildingBehaviourTypes.Hearth);
    }
}

public class ExtractorData : GenericBuildingData<ExtractorModel>
{
    public ExtractorData(string guid, string name, BuildingTypes id, ExtractorModel model, BuildingBehaviourTypes behaviour) : base(guid, name, id, model, behaviour)
    {
    }
}