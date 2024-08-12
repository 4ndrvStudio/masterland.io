using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace masterland.Wallet
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Data;
    using masterland.UI;


    public class ContractRespone
    {
        public bool IsSuccess;
        public string Message;
        public object Data;
    }

    public class WalletInteractor : Singleton<WalletInteractor>
    {
        public static bool IsWebgl = 
    #if UNITY_WEBGL && !UNITY_EDITOR
        true;
    #else
        false;
    #endif

        private UniTaskCompletionSource<string> tcs_GetAddress;
        private UniTaskCompletionSource<List<MasterData>> tcs_GetOwnedMasters;
        private UniTaskCompletionSource<ContractRespone> tcs_MintMaster;
        private UniTaskCompletionSource<bool> tcs_CheckMasterExist;

        private UniTaskCompletionSource<List<LandData>> tcs_GetLands;
        private UniTaskCompletionSource<LandData> tcs_GetLand;
        private UniTaskCompletionSource<ResidentLicense> tcs_GetResidentLicense;
        private UniTaskCompletionSource<ContractRespone> tcs_RegisterResidentLicense;
        private UniTaskCompletionSource<ContractRespone> tcs_UnregisterResidentLicense;

        private UniTaskCompletionSource<ContractRespone> tcs_MintMineral;

        #region Get Address
        [DllImport("__Internal")]
        private static extern void getAddress();

        //Login
        public async UniTask<string> GetAddress()
        {
            tcs_GetAddress = new UniTaskCompletionSource<string>();
            #if UNITY_WEBGL && !UNITY_EDITOR
                getAddress();
                return await tcs_GetAddress.Task;
            #endif

            Receive_GetAddress(SuiWallet.GetActiveAddress());
            return await tcs_GetAddress.Task;
        }

        public void Receive_GetAddress(string address)
        {
            tcs_GetAddress.TrySetResult(address);
        }
        #endregion

        #region Mint Master
        [DllImport("__Internal")]
        private static extern void mintMaster();

        public async UniTask<ContractRespone> MintMaster()
        {
            tcs_MintMaster = new UniTaskCompletionSource<ContractRespone>();
            #if UNITY_WEBGL && !UNITY_EDITOR
                mintMaster();
                return await tcs_MintMaster.Task;
            #endif
            
            Receive_MintMaster(await WalletInGame.MintMaster());


            return await tcs_MintMaster.Task;
        }

        public void Receive_MintMaster(string result)
        {
            ContractRespone contractRespone = new ContractRespone();
            try
            {
                Debug.Log(result);
                JObject jsonObj = JObject.Parse(result);
                bool isSuccess = jsonObj["isSuccess"]?.ToObject<bool>() ?? false;

                if (isSuccess && jsonObj["data"] is JObject data)
                {
                    var masterData = new MasterData
                    {
                        Id = data["objectId"]?.ToString(),
                        Name = data["display"]?["data"]?["name"]?.ToString(),
                        Description = data["display"]?["data"]?["description"]?.ToString(),
                        ImgUrl = data["display"]?["data"]?["image_url"]?.ToString(),
                        Link = data["display"]?["data"]?["link"]?.ToString(),
                        Hp = data["content"]?["fields"]?["hp"]?.ToObject<int>() ?? 0,
                        Mp = data["content"]?["fields"]?["mana"]?.ToObject<int>() ?? 0
                    };
                    contractRespone.IsSuccess = true;
                    contractRespone.Message = "Success";
                    contractRespone.Data = masterData;
                    tcs_MintMaster.TrySetResult(contractRespone);
                }
                else
                {
                    contractRespone.IsSuccess = false;
                    contractRespone.Message = jsonObj["message"]?.ToString();
                    tcs_MintMaster.TrySetResult(contractRespone);

                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse result: {ex.Message}");
                contractRespone.IsSuccess = false;
                contractRespone.Message = "Mint Fail";
                tcs_MintMaster.TrySetResult(contractRespone);
            }

        }
        #endregion

        #region Get Masters
        [DllImport("__Internal")]
        private static extern void getMasters();

        public async UniTask<List<MasterData>> GetMasters()
        {
            tcs_GetOwnedMasters = new UniTaskCompletionSource<List<MasterData>>();
#if UNITY_WEBGL && !UNITY_EDITOR
            getMasters();
            return await tcs_GetOwnedMasters.Task;
#endif
            Receive_GetMasters(await WalletInGame.GetMasters());
            return await tcs_GetOwnedMasters.Task;
        }

        public void Receive_GetMasters(string result)
        {
            JArray jsonArrayObj = JArray.Parse(result);

            List<MasterData> ownedMasters = new List<MasterData>();

            foreach (var jsonObj in jsonArrayObj)
            {
                var masterData = new MasterData
                {
                    Id = jsonObj["objectId"]?.ToString(),
                    Name = jsonObj["display"]?["data"]?["name"]?.ToString(),
                    Description = jsonObj["display"]?["data"]?["description"]?.ToString(),
                    ImgUrl = jsonObj["display"]?["data"]?["image_url"]?.ToString(),
                    Link = jsonObj["display"]?["data"]?["link"]?.ToString(),
                    Hp = jsonObj["content"]["fields"]["hp"].ToObject<int>(),
                    Mp = jsonObj["content"]["fields"]["mana"].ToObject<int>()
                };
                ownedMasters.Add(masterData);
            }
            tcs_GetOwnedMasters.TrySetResult(ownedMasters);
        }
        #endregion

        #region Check master Exits
        [DllImport("__Internal")]
        private static extern void checkMasterExist(string masterId);

        public async UniTask<bool> CheckMasterExist(string masterId)
        {
            tcs_CheckMasterExist = new UniTaskCompletionSource<bool>();
            if (string.IsNullOrEmpty(masterId))
                Receive_CheckMasterExist("false");

#if UNITY_WEBGL && !UNITY_EDITOR
            checkMasterExist(masterId);
            return await tcs_CheckMasterExist.Task;
#endif
            Receive_CheckMasterExist(await WalletInGame.CheckMasterExist(masterId));

            return await tcs_CheckMasterExist.Task;
        }

        public void Receive_CheckMasterExist(string result)
        {
            tcs_CheckMasterExist.TrySetResult(bool.Parse(result));
        }
        #endregion

        #region Get Lands
        [DllImport("__Internal")]
        private static extern void getLands();

        public async UniTask<List<LandData>> GetLands()
        {
            tcs_GetLands = new UniTaskCompletionSource<List<LandData>>();
#if UNITY_WEBGL && !UNITY_EDITOR
            getLands();
             return await tcs_GetLands.Task;
#endif
            Receive_GetLands(await WalletInGame.GetLands()); 
            return await tcs_GetLands.Task;
        }

        public void Receive_GetLands(string result)
        {
            JArray jsonArrayObj = JArray.Parse(result);

            List<LandData> lands = new List<LandData>();

            foreach (var jsonObj in jsonArrayObj)
            {
                var landData = new LandData
                {
                    Id = jsonObj["id"]["id"].ToString(),
                    Name = jsonObj["name"].ToString(),
                    HasLeased = jsonObj["has_leased"].ToObject<bool>(),
                    LandTenant = jsonObj["land_tenant"].ToString(),
                    LeasingTime = jsonObj["leasing_time"].ToString(),
                    NextPayTaxTime = jsonObj["next_pay_tax_time"].ToString(),
                    ResidentLicenseProvided = jsonObj["resident_license_provided"].ToObject<List<string>>(),
                    StoneBalance = jsonObj["stone_balance"].ToString(),
                    WoodBalance = jsonObj["wood_balance"].ToString(),
                    BuildingStoreAddress = jsonObj["building_store_address"].ToString(),
                    CraftingStoreAddress = jsonObj["crafting_store_address"].ToString()
                };
                lands.Add(landData);
            }
            tcs_GetLands.TrySetResult(lands);
        }
        #endregion

        #region Get Land
        [DllImport("__Internal")]
        private static extern void getLand(string landAddress);

        public async UniTask<LandData> GetLand(string landAddress)
        {
            tcs_GetLand = new UniTaskCompletionSource<LandData>();
#if UNITY_WEBGL && !UNITY_EDITOR
            getLand(landAddress);
            return await tcs_GetLand.Task;
#endif
            Receive_GetLand(await WalletInGame.GetLand(landAddress));
            return await tcs_GetLand.Task;
        }

        public void Receive_GetLand(string result)
        {
            if(string.IsNullOrEmpty(result) || result.Length <=2) {
                tcs_GetLand.TrySetResult(null);
                return; 
            }
            JObject jsonObj = JObject.Parse(result);
            var landData = new LandData
            {
                    Id = jsonObj["id"]["id"].ToString(),
                    Name = jsonObj["name"].ToString(),
                    HasLeased = jsonObj["has_leased"].ToObject<bool>(),
                    LandTenant = jsonObj["land_tenant"].ToString(),
                    LeasingTime = jsonObj["leasing_time"].ToString(),
                    NextPayTaxTime = jsonObj["next_pay_tax_time"].ToString(),
                    ResidentLicenseProvided = jsonObj["resident_license_provided"].ToObject<List<string>>(),
                    StoneBalance = jsonObj["stone_balance"].ToString(),
                    WoodBalance = jsonObj["wood_balance"].ToString(),
                    BuildingStoreAddress = jsonObj["building_store_address"].ToString(),
                    CraftingStoreAddress = jsonObj["crafting_store_address"].ToString()
            };
            tcs_GetLand.TrySetResult(landData);
        }
        #endregion

        #region Get Resident License
        [DllImport("__Internal")]
        private static extern void getResidentLicense(string master);

        public async UniTask<ResidentLicense> GetResidentLicense(string master)
        {
            tcs_GetResidentLicense = new UniTaskCompletionSource<ResidentLicense>();
#if UNITY_WEBGL && !UNITY_EDITOR
            getResidentLicense(master);
            return await tcs_GetResidentLicense.Task;
#endif
            Receive_GetResidentLicense(await WalletInGame.GetResidentLicense(master));
            return await tcs_GetResidentLicense.Task;
        }

        public void Receive_GetResidentLicense(string result)
        {
     
            if (string.IsNullOrEmpty(result) || result.Length <= 2)
            {
                tcs_GetResidentLicense.TrySetResult(null);
            }
            else
            {
                Debug.Log(result);
                JObject jsonObj = JObject.Parse(result);
                var residentLicense = new ResidentLicense
                {
                    Id = jsonObj["id"]?["id"]?.ToString(),
                    LandId = jsonObj["land_id"]?.ToString(),
                    NextTimeMintStone = jsonObj["next_time_mint_stone"]?.ToString(),
                    NextTimeMintWood = jsonObj["next_time_mint_wood"]?.ToString(),
                    StoneBalance = jsonObj["stone_balance"]?.ToString(),
                    WoodBalance = jsonObj["wood_balance"]?.ToString(),
                    StoneMintedPerDay = jsonObj["stone_minted_per_day"]?.ToString(),
                    WoodMintedPerDay = jsonObj["wood_minted_per_day"]?.ToString(),
                    StoneLimitedPerDay = jsonObj["stone_limited_per_day"]?.ToString(),
                    WoodLimitedPerDay = jsonObj["wood_limited_per_day"]?.ToString(),
                };
                Debug.Log(residentLicense.Id);
                tcs_GetResidentLicense.TrySetResult(residentLicense);
            }


        }
        #endregion

        #region Register Resident
        [DllImport("__Internal")]
        private static extern void registerResidentLicense(string master, string land_id);

        public async UniTask<ContractRespone> RegisterResidentLicense(string master, string land_id)
        {
            tcs_RegisterResidentLicense = new UniTaskCompletionSource<ContractRespone>();
#if UNITY_WEBGL && !UNITY_EDITOR
            registerResidentLicense(master,land_id);
            return await tcs_RegisterResidentLicense.Task;
#endif
            Receive_RegisterResidentLicense(await WalletInGame.RegisterResidentLicense(master,land_id));
            return await tcs_RegisterResidentLicense.Task;
        }

        public void Receive_RegisterResidentLicense(string result)
        {
            Debug.Log(result);
            ContractRespone contractRespone = new ContractRespone();
            try
            {
                JObject jsonObj = JObject.Parse(result);
                bool isSuccess = jsonObj["isSuccess"]?.ToObject<bool>() ?? false;

                if (isSuccess && jsonObj["data"] is JObject data)
                {
                    var residentLicense = new ResidentLicense
                    {
                        Id = data["id"]?["id"]?.ToString(),
                        LandId = data["land_id"]?.ToString(),
                        NextTimeMintStone = data["next_time_mint_stone"]?.ToString(),
                        NextTimeMintWood = data["next_time_mint_wood"]?.ToString(),
                        StoneBalance = data["stone_balance"]?.ToString(),
                        WoodBalance = data["wood_balance"].ToString(),
                        StoneMintedPerDay = data["stone_minted_per_day"]?.ToString(),
                        WoodMintedPerDay = data["wood_minted_per_day"]?.ToString(),
                        StoneLimitedPerDay = data["stone_limited_per_day"]?.ToString(),
                        WoodLimitedPerDay = data["wood_limited_per_day"]?.ToString(),
                    };
                    contractRespone.IsSuccess = true;
                    contractRespone.Message = "Success";
                    contractRespone.Data = residentLicense;
                    tcs_RegisterResidentLicense.TrySetResult(contractRespone);
                }
                else
                {
                    contractRespone.IsSuccess = false;
                    contractRespone.Message = jsonObj["message"].ToString();
                    tcs_RegisterResidentLicense.TrySetResult(contractRespone);

                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse result: {ex.Message}");
                contractRespone.IsSuccess = false;
                contractRespone.Message = "Mint Fail";
                tcs_RegisterResidentLicense.TrySetResult(contractRespone);
            }
        }
        #endregion
        

        #region Unregister Resident
        [DllImport("__Internal")]
        private static extern void unregisterResidentLicense(string master);

        public async UniTask<ContractRespone> UnregisterResidentLicense(string master)
        {
            tcs_UnregisterResidentLicense = new UniTaskCompletionSource<ContractRespone>();
#if UNITY_WEBGL && !UNITY_EDITOR
            unregisterResidentLicense(master);
            return await tcs_UnregisterResidentLicense.Task;
#endif
            Receive_UnregisterResidentLicense(await WalletInGame.UnresidentLicense(master));
            return await tcs_UnregisterResidentLicense.Task;
        }

        public void Receive_UnregisterResidentLicense(string result)
        {
            ContractRespone contractRespone = new ContractRespone();
            try
            {
                JObject jsonObj = JObject.Parse(result);
                bool isSuccess = jsonObj["isSuccess"]?.ToObject<bool>() ?? false;

                if (isSuccess)
                {
                    contractRespone.IsSuccess = true;
                    contractRespone.Message = "";
                }
                else
                {
                    contractRespone.IsSuccess = false;
                    contractRespone.Message = jsonObj["message"].ToString();;
                }
                tcs_UnregisterResidentLicense.TrySetResult(contractRespone);

            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse result: {ex.Message}");
                contractRespone.IsSuccess = false;
                contractRespone.Message = "";
                tcs_UnregisterResidentLicense.TrySetResult(contractRespone);
            }
        }
        #endregion


        #region Mint mineral
        [DllImport("__Internal")]
        private static extern void mintMineral(string master,string type);

        public async UniTask<ContractRespone> MintMineral(string master, string type)
        {
            tcs_MintMineral = new UniTaskCompletionSource<ContractRespone>();
#if UNITY_WEBGL && !UNITY_EDITOR
            mintMineral(master,type);
            return await tcs_MintMineral.Task;
#endif
            Receive_MintMineral(await WalletInGame.MintMineral(master,type));
            return await tcs_MintMineral.Task;
        }

        public void Receive_MintMineral(string result)
        {
            ContractRespone contractRespone = new ContractRespone();
            try
            {
                JObject jsonObj = JObject.Parse(result);
                bool isSuccess = jsonObj["isSuccess"]?.ToObject<bool>() ?? false;
                if (isSuccess)
                {
                    contractRespone.IsSuccess = true;
                    contractRespone.Message = jsonObj["message"].ToString();
                }
                else
                {
                    contractRespone.IsSuccess = false;
                    contractRespone.Message = jsonObj["message"].ToString();
                }
                tcs_MintMineral.TrySetResult(contractRespone);

            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse result: {ex.Message}");
                contractRespone.IsSuccess = false;
                contractRespone.Message = "";
                tcs_MintMineral.TrySetResult(contractRespone);
            }
        }
        #endregion

    }
}
