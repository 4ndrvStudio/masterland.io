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


    public class ContractRespone {
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
                    contractRespone.IsSucess =true;
                    contractRespone.Message= "Success";
                    contractRespone.Data = masterData;
                    tcs_MintMaster.TrySetResult(contractRespone);
                }
                else
                {
                    contractRespone.IsSucess =false;
                    contractRespone.Message= "Mint Fail";
                    tcs_MintMaster.TrySetResult(contractRespone);

                }
            }
            catch (Exception ex)
            {

                Debug.LogError($"Failed to parse result: {ex.Message}");
                contractRespone.IsSucess =false;
                contractRespone.Message= "Mint Fail";
                tcs_MintMaster.TrySetResult(contractRespone);
            }

        }
        #endregion

        #region Get Masters
        [DllImport("__Internal")]
        private static extern void getMasters();

        public async UniTask<List<MasterData>> GetMasters() {
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

        public async UniTask<bool> CheckMasterExist(string masterId) {
            tcs_CheckMasterExist = new UniTaskCompletionSource<bool>();
            if(string.IsNullOrEmpty(masterId))
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


    }
}
