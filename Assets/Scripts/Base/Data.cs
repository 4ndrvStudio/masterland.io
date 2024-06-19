using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using masterland.Wallet;
using UnityEngine;

namespace masterland.Data
{
    public class Data : Singleton<Data>
    {
        public string Address = string.Empty;
        public List<MasterData> OwnedMasters = new();
        public MasterData MasterData;

        public async UniTask InitData() {
            Address = await WalletInteractor.Instance.GetAddress();
            OwnedMasters = await WalletInteractor.Instance.GetMasters();
            await GetSelectedMaster();
        }

        public async UniTask GetSelectedMaster()
        {
            string selectedMaster = PlayerPrefs.GetString("selectedMaster");
            bool isMasterExist = await WalletInteractor.Instance.CheckMasterExist(selectedMaster);
            MasterData = isMasterExist? OwnedMasters.Find(item => item.Id == selectedMaster): null;
        }
        public async UniTask<string> SelectMaster(string masterId) {
            
            bool isMasterExist = await WalletInteractor.Instance.CheckMasterExist(masterId);
            MasterData = isMasterExist? OwnedMasters.Find(item => item.Id == masterId): null;
            if(isMasterExist)
                PlayerPrefs.SetString("selectedMaster", masterId);
            return MasterData.Id;
        }
        

    }

    [System.Serializable]
    public class MasterData
    {
        public string Id;
        public string Name;
        public string Description;
        public string ImgUrl;
        public string Link;
        public int Hp;
        public int Mp;
    }

}
