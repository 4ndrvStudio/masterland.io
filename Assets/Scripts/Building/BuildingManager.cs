using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using FishNet.Object;
using FishNet.Managing;
using masterland.Manager;
namespace masterland.Building
{
    public class BuildingManager : SingletonNetwork<BuildingManager>
    {
        [SerializeField] private NetworkObject _buildingPrefab;
        public string JsonData = string.Empty;
        public List<BuildingComponent> BuildingComponents = new List<BuildingComponent>();

        private JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
        };

        [ServerRpc(RequireOwnership =false)]
        public void RequireBuildHouse(string jsonData) 
        {
            Build(jsonData);
        }

        public void Build(string jsonData) 
        {
            BuildingData buildingData = ConvertJsonToBuildingData(jsonData);
            NetworkObject nob = Network.Instance._networkManager.GetPooledInstantiated(_buildingPrefab, buildingData.Position.ToVector3(),Quaternion.Euler(buildingData.EulerAngles.ToVector3()), true);
            nob.GetComponent<Building>().BuildingDataJson = jsonData;            
            Network.Instance._networkManager.ServerManager.Spawn(nob);

        }

        public string ConvertBuildingToJsonData(Vector3 selectedHousePosition, Vector3 eulerAngles, List<ElementConnections> elementConnectionsList) 
        {
            BuildingData buildingData= new BuildingData();
            buildingData.Position =  new SerializedVector3(selectedHousePosition);
            buildingData.EulerAngles = new SerializedVector3(eulerAngles);
            buildingData.Components = new();
            foreach(var element in elementConnectionsList){
                buildingData.Components.Add(new BuildingComponentData 
                {
                    Type = element.BuildingComponentConfig.Type,
                    Position = new SerializedVector3(element.Element.transform.localPosition),
                    EulerAngles = new SerializedVector3(element.Element.transform.localEulerAngles)
                });
            }
            JsonData = JsonConvert.SerializeObject(buildingData,jsonSettings);
            return JsonData;
        }

        public BuildingData ConvertJsonToBuildingData(string Json) =>
                JsonConvert.DeserializeObject<BuildingData>(Json,jsonSettings);
        


    }
}
