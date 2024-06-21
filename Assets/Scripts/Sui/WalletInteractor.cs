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
        public bool IsSucess;
        public string Message;
        public object Data;
    }

    public class WalletInteractor : Singleton<WalletInteractor>
    {

        private UniTaskCompletionSource<string> tcs_GetAddress;
        private UniTaskCompletionSource<List<MasterData>> tcs_GetOwnedMasters;
        private UniTaskCompletionSource<ContractRespone> tcs_MintMaster;
        private UniTaskCompletionSource<bool> tcs_CheckMasterExist;

        private UniTaskCompletionSource<List<LandData>> tcs_GetLands;
        private UniTaskCompletionSource<LandData> tcs_GetLand;
        private UniTaskCompletionSource<ResidentLicense> tcs_GetResidentLicense;
        private UniTaskCompletionSource<ContractRespone> tcs_RegisterResidentLicense;
        private UniTaskCompletionSource<ContractRespone> tcs_UnregisterResidentLicense;

        #region fake data
        string m_address = "";
        string m_masters = @"[
    {
        ""objectId"": ""0x07b54e72b543c62fd1af316f336ff790466d6e52a2a2c964d4a150d163a583b5"",
        ""version"": ""50330514"",
        ""digest"": ""76y8Pq7kErqs2f9fbxHqamHTUhcvVrStV18fghYChyex"",
        ""type"": ""0xca1c7449e88c3507b851218c4845f10e3f5c101d3a7fd081a4046331376e2908::master::MasterNFT"",
        ""display"": {
            ""data"": {
                ""creator"": ""4ndrv Studio"",
                ""description"": ""A character of the MasterLand.io game!"",
                ""image_url"": ""ipfs://masterland.io"",
                ""link"": ""https://masterland.io/master/0x07b54e72b543c62fd1af316f336ff790466d6e52a2a2c964d4a150d163a583b5"",
                ""name"": ""Master #7"",
                ""project_url"": ""https://masterland.io""
            },
            ""error"": null
        },
        ""content"": {
            ""dataType"": ""moveObject"",
            ""type"": ""0xca1c7449e88c3507b851218c4845f10e3f5c101d3a7fd081a4046331376e2908::master::MasterNFT"",
            ""hasPublicTransfer"": true,
            ""fields"": {
                ""hp"": 3,
                ""id"": {
                    ""id"": ""0x07b54e72b543c62fd1af316f336ff790466d6e52a2a2c964d4a150d163a583b5""
                },
                ""image_url"": ""masterland.io"",
                ""mana"": 100,
                ""name"": ""Master #7"",
                ""resident_license"": null
            }
        }
    },
    {
        ""objectId"": ""0x1580b275f9adfd2a9606c3d69a03c1bf857c63a9d9e20d05bb32923ba5579438"",
        ""version"": ""50330513"",
        ""digest"": ""AKxYD2jX52AeTVwLC5n9cq9KqUHgW7G8BjsAKXpqAphA"",
        ""type"": ""0xca1c7449e88c3507b851218c4845f10e3f5c101d3a7fd081a4046331376e2908::master::MasterNFT"",
        ""display"": {
            ""data"": {
                ""creator"": ""4ndrv Studio"",
                ""description"": ""A character of the MasterLand.io game!"",
                ""image_url"": ""ipfs://masterland.io"",
                ""link"": ""https://masterland.io/master/0x1580b275f9adfd2a9606c3d69a03c1bf857c63a9d9e20d05bb32923ba5579438"",
                ""name"": ""Master #6"",
                ""project_url"": ""https://masterland.io""
            },
            ""error"": null
        },
        ""content"": {
            ""dataType"": ""moveObject"",
            ""type"": ""0xca1c7449e88c3507b851218c4845f10e3f5c101d3a7fd081a4046331376e2908::master::MasterNFT"",
            ""hasPublicTransfer"": true,
            ""fields"": {
                ""hp"": 3,
                ""id"": {
                    ""id"": ""0x1580b275f9adfd2a9606c3d69a03c1bf857c63a9d9e20d05bb32923ba5579438""
                },
                ""image_url"": ""masterland.io"",
                ""mana"": 100,
                ""name"": ""Master #6"",
                ""resident_license"": null
            }
        }
    },
    {
        ""objectId"": ""0x4114559a233fc46233e48e2940bf82c544134f17149f0a84ad890798e0c4feef"",
        ""version"": ""50330508"",
        ""digest"": ""2X3b1K679Kvw5AMUm6gJka94Yo7YbhnmdNpQsBQoL8P1"",
        ""type"": ""0xca1c7449e88c3507b851218c4845f10e3f5c101d3a7fd081a4046331376e2908::master::MasterNFT"",
        ""display"": {
            ""data"": {
                ""creator"": ""4ndrv Studio"",
                ""description"": ""A character of the MasterLand.io game!"",
                ""image_url"": ""ipfs://masterland.io"",
                ""link"": ""https://masterland.io/master/0x4114559a233fc46233e48e2940bf82c544134f17149f0a84ad890798e0c4feef"",
                ""name"": ""Master #1"",
                ""project_url"": ""https://masterland.io""
            },
            ""error"": null
        },
        ""content"": {
            ""dataType"": ""moveObject"",
            ""type"": ""0xca1c7449e88c3507b851218c4845f10e3f5c101d3a7fd081a4046331376e2908::master::MasterNFT"",
            ""hasPublicTransfer"": true,
            ""fields"": {
                ""hp"": 3,
                ""id"": {
                    ""id"": ""0x4114559a233fc46233e48e2940bf82c544134f17149f0a84ad890798e0c4feef""
                },
                ""image_url"": ""masterland.io"",
                ""mana"": 100,
                ""name"": ""Master #1"",
                ""resident_license"": {
                    ""type"": ""0xca1c7449e88c3507b851218c4845f10e3f5c101d3a7fd081a4046331376e2908::master::ResidentLicense"",
                    ""fields"": {
                        ""id"": {
                            ""id"": ""0xf89a482933fada1a0823e598679627609ad3c65f433b9d34ad4e734d1244608d""
                        },
                        ""land_id"": ""2"",
                        ""next_time_mint_stone"": ""1718261825471"",
                        ""next_time_mint_wood"": ""1718261825471"",
                        ""stone_balance"": ""10"",
                        ""stone_limited_per_day"": ""3"",
                        ""stone_minted_per_day"": ""1"",
                        ""wood_balance"": ""30"",
                        ""wood_limited_per_day"": ""3"",
                        ""wood_minted_per_day"": ""1""
                    }
                }
            }
        }
    }]";
        string m_lands = @"
[
    {
        ""building_store_address"": ""0x0e87c3eb3ec7ab199bdce0bfedddc3feb1d5f5ee65f5ad7546fae298f30ec4c5"",
        ""crafting_store_address"": ""0xc7ee4aa26e9acda2e2d0f66a51ea00d5912535459012a510bf8eb3e9feccaf80"",
        ""has_leased"": false,
        ""id"": {
            ""id"": ""0x447768027b3b66af6266589a3aa7b69c3a27447b231c4efd922152b36f42f9e8""
        },
        ""land_tenant"": ""0x4a546acc0fcf4462cb1db23a3731a1eec1d24db2124c0e924a8027447b3ff4b7"",
        ""leasing_time"": ""0"",
        ""name"": ""Land #2"",
        ""next_pay_tax_time"": ""0"",
        ""resident_license_num_provided"": ""1"",
        ""resident_license_provided"": [
            ""0xf89a482933fada1a0823e598679627609ad3c65f433b9d34ad4e734d1244608d""
        ],
        ""stone_balance"": ""990"",
        ""wood_balance"": ""970""
    },
    {
        ""building_store_address"": ""0xd5617c54c13a7182a45d5058022495db7a447ce814673b212b1a43edb6fd5ee3"",
        ""crafting_store_address"": ""0x3e0a2755cfbe7cba8ff08671eb4da280845f63e18b59776bcb76c8f35c9e2d7b"",
        ""has_leased"": false,
        ""id"": {
            ""id"": ""0xc8841056b0ee7fd7e677d4266760df6e119fba321b19aa6a50dd1a58b472ee94""
        },
        ""land_tenant"": ""0x4a546acc0fcf4462cb1db23a3731a1eec1d24db2124c0e924a8027447b3ff4b7"",
        ""leasing_time"": ""0"",
        ""name"": ""Land #3"",
        ""next_pay_tax_time"": ""0"",
        ""resident_license_num_provided"": ""0"",
        ""resident_license_provided"": [],
        ""stone_balance"": ""1000"",
        ""wood_balance"": ""1000""
    },
    {
        ""building_store_address"": ""0x82b1eb89e9f6bed35d8aaa2babd80335c6d46d5ea22f223d8a0a36b63f4c5fe2"",
        ""crafting_store_address"": ""0xaa65670a2d89a00a73c2471482a8489eef4fe20078f90a7f5a1dda192da5546a"",
        ""has_leased"": false,
        ""id"": {
            ""id"": ""0x3093d72dd1e55f56a13e9a2a370c808af819d864e2064dc0f687a5bb534774b0""
        },
        ""land_tenant"": ""0x4a546acc0fcf4462cb1db23a3731a1eec1d24db2124c0e924a8027447b3ff4b7"",
        ""leasing_time"": ""0"",
        ""name"": ""Land #1"",
        ""next_pay_tax_time"": ""0"",
        ""resident_license_num_provided"": ""0"",
        ""resident_license_provided"": [],
        ""stone_balance"": ""1000"",
        ""wood_balance"": ""1000""
    }
]";
        #endregion

        #region Get Address
        [DllImport("__Internal")]
        private static extern void getAddress();

        //Login
        public async UniTask<string> GetAddress()
        {
            tcs_GetAddress = new UniTaskCompletionSource<string>();
#if UNITY_EDITOR
            Receive_GetAddress(m_address);
#else
                getAddress();
#endif
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
#if UNITY_EDITOR
            Receive_MintMaster(m_masters);
#else
                mintMaster();
#endif

            return await tcs_MintMaster.Task;
        }

        public void Receive_MintMaster(string result)
        {
            ContractRespone contractRespone = new ContractRespone();
            try
            {
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
                    contractRespone.IsSucess = true;
                    contractRespone.Message = "Success";
                    contractRespone.Data = masterData;
                    tcs_MintMaster.TrySetResult(contractRespone);
                }
                else
                {
                    contractRespone.IsSucess = false;
                    contractRespone.Message = "Mint Fail";
                    tcs_MintMaster.TrySetResult(contractRespone);

                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse result: {ex.Message}");
                contractRespone.IsSucess = false;
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
#if UNITY_EDITOR
            Receive_GetMasters(m_masters);
#else
                getMasters();
#endif
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
                    Id = jsonObj["objectId"].ToString(),
                    Name = jsonObj["display"]["data"]["name"].ToString(),
                    Description = jsonObj["display"]["data"]["description"].ToString(),
                    ImgUrl = jsonObj["display"]["data"]["image_url"].ToString(),
                    Link = jsonObj["display"]["data"]["link"].ToString(),
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
#if UNITY_EDITOR
            Receive_CheckMasterExist("true");
#else
                checkMasterExist(masterId);
#endif
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
#if UNITY_EDITOR
            Receive_GetLands(m_lands);
#else
                getLands();
#endif
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
#if UNITY_EDITOR
            Receive_GetLand(m_lands);
#else
            getLand(landAddress);
#endif
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
#if UNITY_EDITOR
            Receive_GetResidentLicense(string.Empty);
#else
                getResidentLicense(master);
#endif
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
#if UNITY_EDITOR
            Receive_RegisterResidentLicense(m_lands);
#else
               registerResidentLicense(master,land_id);
#endif
            return await tcs_RegisterResidentLicense.Task;
        }

        public void Receive_RegisterResidentLicense(string result)
        {
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
                    contractRespone.IsSucess = true;
                    contractRespone.Message = "Success";
                    contractRespone.Data = residentLicense;
                    tcs_RegisterResidentLicense.TrySetResult(contractRespone);
                }
                else
                {
                    contractRespone.IsSucess = false;
                    contractRespone.Message = "Mint Fail";
                    tcs_RegisterResidentLicense.TrySetResult(contractRespone);

                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse result: {ex.Message}");
                contractRespone.IsSucess = false;
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
#if UNITY_EDITOR
            Receive_UnregisterResidentLicense(m_lands);
#else
               unregisterResidentLicense(master);
#endif
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
                    contractRespone.IsSucess = true;
                    contractRespone.Message = "";
                }
                else
                {
                    contractRespone.IsSucess = false;
                    contractRespone.Message = "";
                }
                tcs_UnregisterResidentLicense.TrySetResult(contractRespone);

            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse result: {ex.Message}");
                contractRespone.IsSucess = false;
                contractRespone.Message = "";
                tcs_UnregisterResidentLicense.TrySetResult(contractRespone);
            }
        }
        #endregion

    }
}
