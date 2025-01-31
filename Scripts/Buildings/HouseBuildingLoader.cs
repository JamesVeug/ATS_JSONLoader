using System.Collections.Generic;
using System.Linq;
using ATS_API.Buildings;
using ATS_API.Helpers;
using Eremite;
using Eremite.Buildings;

public class HouseBuildingLoader : ABuildingLoader<HouseModel, HouseBuildingData>
{
    public const string kFileExtension = "_houseBuilding.json";
    public override string FileExtension => kFileExtension;
    public override string Category => "HouseBuilding";
    public override IEnumerable<HouseModel> AllModels => MB.Settings.Buildings.Where(a=>a is HouseModel).Cast<HouseModel>();
    
    public override HouseModel CreateNewModelModel(string guid, string name)
    {
        return BuildingManager.CreateHouse(guid, name).BuildingModel;
    }

    public override bool GetNewData(string name, out string rawName, out string guid)
    {
        NewHouseBuildingData data = BuildingManager.NewHouses.FirstOrDefault(a => a.BuildingModel.name == name);
        if (data == null)
        {
            rawName = null;
            guid = null;
            return false;
        }
        
        rawName = data.Name;
        guid = data.Guid;
        return true;
    }

    public override void Apply(HouseModel model, HouseBuildingData data, bool toModel, string modelName, bool isNewGood)
    {
        base.Apply(model, data, toModel, modelName, isNewGood);

        HouseBuildingBuilder builder = new HouseBuildingBuilder(model);

        ImportExportUtils.ApplyLocaText(ref model.displayName, ref data.displayName, (a,b)=>builder.SetDisplayName(a,b), toModel, "displayName");
        ImportExportUtils.ApplyLocaText(ref model.description, ref data.description, (a,b)=>builder.SetDescription(a,b), toModel, "description");
        
        ImportExportUtils.ApplyValueNoNull(ref model.housingRaces, ref data.housingRaces, toModel, Category, "housingRaces");
        ImportExportUtils.ApplyValueNoNull(ref model.housingPlaces, ref data.housingPlaces, toModel, Category, "housingPlaces");
        ImportExportUtils.ApplyValueNoNull(ref model.servedNeeds, ref data.servedNeeds, toModel, Category, "servedNeeds");
    }
}

[GenerateSchema("HouseBuilding", "Buildings that turn basic HouseBuildings into products.", HouseBuildingLoader.kFileExtension)]
public class HouseBuildingData : ABuildingData
{
    [SchemaField(null)]
    public string[] housingRaces;

    [SchemaField(null)]
    public int? housingPlaces;

    [SchemaField(null)]
    public string[] servedNeeds;
}
