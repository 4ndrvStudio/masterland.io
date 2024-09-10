using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace masterland.Building
{
    public class Building : NetworkBehaviour
    {
        public string BuildingDataJson;

        public override void OnSpawnServer(NetworkConnection connection)
        {
            base.OnSpawnServer(connection);
            SetupHouse(BuildingDataJson);
        }


        [ObserversRpc]
        public void Rpc_SetupHouse(string buildingDataJson) 
        {
            SetupHouse(buildingDataJson);
        }

        public void SetupHouse(string buildingDataJson) 
        {
               BuildingData buildingData = BuildingManager.Instance.ConvertJsonToBuildingData(buildingDataJson);
               foreach(var component in buildingData.Components) 
               {    

                    GameObject componentPrefab = BuildingManager.Instance.BuildingComponents.Find(item => item.Type == component.Type).BuildingElementPrefab;
                    componentPrefab.GetComponentInChildren<Rigidbody>().isKinematic = true;
                    GameObject componentOb = Instantiate(componentPrefab, this.transform);
                    componentOb.transform.localPosition = component.Position.ToVector3();
                    componentOb.transform.localEulerAngles= component.EulerAngles.ToVector3();
               }
        }

       
    }
}
