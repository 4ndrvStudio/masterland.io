using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using masterland.Wallet;
using Unity.VisualScripting;
using UnityEngine;

namespace masterland.Data
{
    public class Data : Singleton<Data>
    {
        public string Address = string.Empty;
        public List<MasterData> OwnedMasters = new();
        public List<LandData> Lands = new();
        public MasterData MasterData;
        public ResidentLicense ResidentLicense;
        public LandData Land; 

        public async UniTask InitData() {
            Address = await WalletInteractor.Instance.GetAddress();
            OwnedMasters = await WalletInteractor.Instance.GetMasters();
            Lands = await WalletInteractor.Instance.GetLands();
            await GetSelectedMaster();
        }

        public void SignOutData() {
            MasterData = null;
            Land = null;
            ResidentLicense = null;
        }

        public async UniTask GetSelectedMaster()
        {
            string selectedMaster = PlayerPrefs.GetString($"{Address}-selectedMaster");
            bool isMasterExist = await WalletInteractor.Instance.CheckMasterExist(selectedMaster);
            MasterData = isMasterExist? OwnedMasters.Find(item => item.Id == selectedMaster): null;
            if(MasterData != null && !string.IsNullOrEmpty(MasterData.Id)) {
                ResidentLicense = await WalletInteractor.Instance.GetResidentLicense(MasterData.Id);
                if(ResidentLicense != null && !string.IsNullOrEmpty(ResidentLicense.Id)) {
                    var landAddress = Lands.Find(item => item.Name.Split("#")[1] == ResidentLicense.LandId).Id;
                    Land = await WalletInteractor.Instance.GetLand(landAddress);
                } else  {
                    Land = null;
                }
            }
        }
        public async UniTask<string> SelectMaster(string masterId) {
            
            bool isMasterExist = await WalletInteractor.Instance.CheckMasterExist(masterId);
            MasterData = isMasterExist? OwnedMasters.Find(item => item.Id == masterId): null;
            if(isMasterExist)
                PlayerPrefs.SetString($"{Address}-selectedMaster", masterId);
            if(MasterData != null && !string.IsNullOrEmpty(MasterData.Id)) {
                ResidentLicense = await WalletInteractor.Instance.GetResidentLicense(MasterData.Id);
                if( ResidentLicense != null && !string.IsNullOrEmpty(ResidentLicense.Id)) {
                    var landAddress = Lands.Find(item => item.Name.Split("#")[1] == ResidentLicense.LandId).Id;
                    Land = await WalletInteractor.Instance.GetLand(landAddress);
                } else  {
                    Land = null;
                }
            }
            return MasterData.Id;
        }

    }

    [System.Serializable]
    public class MasterData
    {
        public string Name;
        public string Id;
        public string Description;
        public string ImgUrl;
        public string Link;
        public int Hp;
        public int Mp;
    }

    [System.Serializable]
    public class ResidentLicense {
        public string Id;
        public string LandId;
        public string NextTimeMintStone;
        public string NextTimeMintWood;
        public string StoneBalance;
        public string WoodBalance;
        public string StoneMintedPerDay;
        public string WoodMintedPerDay;
        public string StoneLimitedPerDay;
        public string WoodLimitedPerDay;
    }

    [System.Serializable]
    public class LandData 
    {
        public string Name;
        public string Id;
        public bool HasLeased;
        public string LandTenant;
        public string LeasingTime;
        public string NextPayTaxTime;
        public List<string> ResidentLicenseProvided;
        public string StoneBalance;
        public string WoodBalance;
        public string BuildingStoreAddress;
        public string CraftingStoreAddress;
    }

}
