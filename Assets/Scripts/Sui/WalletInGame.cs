using System.Numerics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using masterland.Manager;
using masterland.UI;
using Suinet.Rpc;
using Suinet.Rpc.Types;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Suinet.Wallet;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace masterland.Wallet
{
    using Data;
    using Suinet.Rpc.Types.MoveTypes;

    public static class WalletInGame
    {
        public static string PACKAGE_ID_ADDRESS = "0xca1c7449e88c3507b851218c4845f10e3f5c101d3a7fd081a4046331376e2908";
        public static string BANK_ADDRESS = "0x5014aad4e542b753d261cd43ea6ac8f517224788c3576c5de9d218698155196a";
        public static string LAND_CAP_ADDRESS = "0x8af6d37fa7c71ba0033f71b224b834c4fb85df603e584f7e42ebc42d695b2ee5";
        public static string LAND_ADDRESS = "0xfb3af20ef01ea6c3dd845674719161fa68b1e15ad6b02c7c17ffacb849fdc82d";
        public static string STONE_ADDRESS = "0xceb81f3be84d88043596b2f164df8b7c85695718b9ba4d26c73001f396c54a2e";
        public static string WOOD_ADDRESS = "0x07f5453b9c20f06e54742be1f254805919ddd6a7abced7332f4059f1d87e3541";
        public static string MASTER_ADDRESS = "0xe56fa24ce838b8f16266d56d045fc8b55cb52fff6a498cfd45c55b4d6e6cfd57";
        public static string BOOK_ADDRESS = "0xf767479f0e60f35501af69cd766aee55492128f6afac74a10d56e3e8435da65e";
        public static string ClockAddress = "0x0000000000000000000000000000000000000000000000000000000000000006";

        public static ulong MINT_MASTER_FEE = 0;


        private static MoveCallTransaction _moveCallTransaction;

        private static UniTaskCompletionSource<ContractRespone> tcs_TxResult;

        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting =Formatting.Indented
        };

        public static void Login(string mnemo = null)
        {
            if (!string.IsNullOrEmpty(mnemo))
            {
                SuiWallet.RestoreWalletFromMnemonics(mnemo);
            }
            GameManager.Instance.LoadGame();
            UIManager.Instance.ShowPopup(PopupName.SuiWallet, new Dictionary<string, object> { { "isConfirmTx", false } });
        }

        public static void Logout()
        {
            SuiWallet.Logout();
            //clear data
            Data.Instance.Address = null;
            Data.Instance.MasterData = null;
            Data.Instance.ResidentLicense = null;
            Data.Instance.Land = null;
            //stop game
            Debug.Log("Call sdf");
            //exit map

            //swich UI
            //UIManager.Instance.ToggleView(ViewName.Init);
            GameManager.Instance.LoadGame();

        }

        public static void Setup()
        {
            _moveCallTransaction = new MoveCallTransaction
            {
                Signer = SuiWallet.GetActiveAddress(),
                PackageObjectId = PACKAGE_ID_ADDRESS,
                TypeArguments = System.Array.Empty<string>(),
                GasBudget = BigInteger.Parse("10000000")
            };
        }

        #region Balance
        public static async UniTask<string> GetBalance()
        {
            string coinType = "0x2::sui::SUI";

            var balanceResult = await SuiApi.Client.GetBalanceAsync(SuiWallet.GetActiveAddress(), coinType);

            float balance = ((float)balanceResult.Result.TotalBalance) / 1000000000;

            return balance.ToString("0.##");
        }

        public static string GetGasFee(string result)
        {
            JObject dryTxObj = JObject.Parse(result);
            if (dryTxObj["result"]["effects"]["gasUsed"] is JObject gasUsed)
            {
                ulong computationCost = ulong.Parse(gasUsed["computationCost"].ToString());
                ulong storageCost = ulong.Parse(gasUsed["storageCost"].ToString());
                ulong storageRebate = ulong.Parse(gasUsed["storageRebate"].ToString());
                decimal cost = decimal.Parse((((computationCost + storageCost) - storageRebate) / 1000000000).ToString());
                return ((computationCost + storageCost) - storageRebate).ToString().ToSuiCoinFormat();
            }
            return "Can't estimate";
        }
        public static string GetBalanceChange(string result)
        {
            JObject dryTxObj = JObject.Parse(result);
            if (dryTxObj["result"]["balanceChanges"] is JArray balanceChanges)
            {
                return balanceChanges[0]["amount"].ToString().ToSuiCoinFormat();
            }
            return "0";
        }

        #endregion

        #region Execute

        public async static Task<RpcResult<TransactionBlockResponse>> Execute(string tx)
        {
            var rpcResult = new RpcResult<TransactionBlockResponse>();


            var keyPair = SuiWallet.GetActiveKeyPair();

            var txBytes = tx;
            var rawSigner = new RawSigner(keyPair);

            var signature = rawSigner.SignData(Intent.GetMessageWithIntent(txBytes));

            rpcResult = await SuiApi.Client.ExecuteTransactionBlockAsync(txBytes, new[] { signature.Value }, TransactionBlockResponseOptions.ShowAll(), ExecuteTransactionRequestType.WaitForLocalExecution);

            return rpcResult;
        }

        #endregion


        #region Master

        public static async UniTask<string> CheckMasterExist(string masterId) {
            
            var masterResult = await SuiApi.Client.GetObjectAsync(masterId, new ObjectDataOptions {
                ShowContent = true,
            }); 
            if(masterResult.IsSuccess) 
                return "true";

            return "false";
        }

        public static async UniTask<string> MintMaster()
        {
            tcs_TxResult = new UniTaskCompletionSource<ContractRespone>();
            _moveCallTransaction.Module = "master";
            _moveCallTransaction.Function = "mint";
            _moveCallTransaction.Arguments = new object[] {
                MASTER_ADDRESS
            };
            var tx_response = await SuiApi.Client.MoveCallAsync(_moveCallTransaction);
            if (tx_response.IsSuccess)
            {
                var dry_tx = await SuiApi.Client.DryRunTransactionBlockAsync(tx_response.Result.TxBytes);

                ConfirmTxData confirmTxData = new ConfirmTxData
                {
                    Title = "Mint a Master?",
                    Tx = tx_response.Result.TxBytes,
                    Tcs = tcs_TxResult,
                    Gas = GetGasFee(dry_tx.RawRpcResponse),
                    Fee = MINT_MASTER_FEE.ToString(),
                    BalanceChange = GetBalanceChange(dry_tx.RawRpcResponse)

                };

                UIManager.Instance.ShowPopup(PopupName.SuiWallet, new Dictionary<string, object>{
                    {"isConfirmTx", true},
                    {"confirmTxData", confirmTxData},
                });
            }
            else
            {
                tcs_TxResult.TrySetResult(new ContractRespone
                {
                    IsSucess = false,
                    Data = null,
                    Message = tx_response.ErrorMessage
                });
                Debug.Log(tx_response.ErrorMessage);
            }

            var tcs_result = await tcs_TxResult.Task;
           
            ContractRespone contractRespone = new ContractRespone();
            if (tcs_result.IsSucess)
            {
                contractRespone.IsSucess = true;
                var data = tcs_result.Data as RpcResult<TransactionBlockResponse>;
                string nftCreatedId = string.Empty;
                Debug.Log(data.RawRpcResponse);
                
                foreach (var objectChange in data.Result.ObjectChanges)
                {
                    if (objectChange.Type == ObjectChangeType.Created )
                        {
                            var createdObjectChange = objectChange as CreatedObjectChange;
                            if (createdObjectChange != null && createdObjectChange.ObjectType  == $"{PACKAGE_ID_ADDRESS}::master::MasterNFT")
                            {   
                                nftCreatedId = createdObjectChange.ObjectId;
                            }
                        }
                }
                var objectResut = await SuiApi.Client.GetObjectAsync(nftCreatedId, new ObjectDataOptions
                {
                    ShowContent = true,
                    ShowDisplay = true,
                    ShowType = true
                });

                contractRespone.Data = objectResut.IsSuccess? objectResut.Result.Data : null;
          
            } else {
                contractRespone.IsSucess = false;
                contractRespone.Message = tcs_result.Message;
            }

            return JsonConvert.SerializeObject(contractRespone);
        }

        public static async UniTask<string> GetMasters()
        {
            ObjectDataOptions optionsReq = new ObjectDataOptions();
            optionsReq.ShowType = true;
            optionsReq.ShowContent = true;
            optionsReq.ShowDisplay = true;
    
            string cursor = null;
            bool hasNextPage = true;
            List<ObjectData > list = new();


            while(hasNextPage) {
                var result = await SuiApi.Client.GetOwnedObjectsAsync(
                    SuiWallet.GetActiveAddress(),
                    new ObjectResponseQuery() { Options = optionsReq }, cursor, null);
                if(result.IsSuccess) 
                {
                    hasNextPage = result.Result.HasNextPage;
                    cursor = result.Result.NextCursor != null ? result.Result.NextCursor : string.Empty;
                    result.Result.Data.ForEach(item => {
                       if(item.Data.Type == $"{PACKAGE_ID_ADDRESS}::master::MasterNFT") {
                           list.Add(item.Data);
                       }
                    });

                } 
                else 
                {
                    hasNextPage = false;
                }
            }

            return JsonConvert.SerializeObject(list,jsonSettings);
        }
        #endregion

        #region Lands
        public static async UniTask<string> GetLands() 
        {
            string cursor = null;
            bool hasNextPage = true;
            List<MoveStruct> list = new();
            while (hasNextPage) {
                var landDO = await SuiApi.Client.GetDynamicFieldsAsync(LAND_ADDRESS,cursor,null);
                if(landDO.IsSuccess) 
                {
                    hasNextPage = landDO.Result.HasNextPage;
                    cursor = landDO.Result.NextCursor != null ? landDO.Result.NextCursor : string.Empty;
                    foreach(var land in landDO.Result.Data) {
                        var landNft = await SuiApi.Client.GetObjectAsync(land.ObjectId, new ObjectDataOptions {
                            ShowContent =true, ShowDisplay = true, ShowType = true
                        });
                        if(landNft.IsSuccess) {
                            var landcontent = landNft.Result.Data.Content as MoveObjectData;
                            list.Add(landcontent.Fields);
                        }
                    }
              
                } 
                else 
                {
                    hasNextPage = false;
                }
            
            }
            return JsonConvert.SerializeObject(list,jsonSettings);
        }

        public static async UniTask<string> GetLand(string landNftAddress) 
        {
            var landNftResult = await SuiApi.Client.GetObjectAsync(landNftAddress, new ObjectDataOptions {
                ShowContent =true, ShowDisplay = true
            });
            if(landNftResult.IsSuccess) {
                var landcontent = landNftResult.Result.Data.Content as MoveObjectData;
                return JsonConvert.SerializeObject(landcontent.Fields,jsonSettings);
            }

            return "[]";
        }

        public static async UniTask<string> GetResidentLicense(string masterAddress) 
        {
            var masterNftResult = await SuiApi.Client.GetObjectAsync(masterAddress, new ObjectDataOptions {
                ShowContent =true, ShowDisplay = true
            });

            if(masterNftResult.IsSuccess) {
                var masterNftContent = masterNftResult.Result.Data.Content as MoveObjectData;
                string masterNftJson = JsonConvert.SerializeObject(masterNftContent.Fields, jsonSettings);
               
                JObject masterNftJObject = JObject.Parse(masterNftJson);
                string resident_license = "";
                if(!string.IsNullOrEmpty(masterNftJObject["resident_license"].ToString())) 
                    resident_license = masterNftJObject["resident_license"]["fields"].ToString();
                return resident_license;
            }

            return "";
        }

        #endregion
        
        #region Resident License
        public static async UniTask<string> RegisterResidentLicense(string master, string landId)
        {
            tcs_TxResult = new UniTaskCompletionSource<ContractRespone>();
            _moveCallTransaction.Module = "master";
            _moveCallTransaction.Function = "register_resident_license";
            _moveCallTransaction.Arguments = new object[] {
                ulong.Parse(landId),
                LAND_ADDRESS,
                master,
                ClockAddress
            };

            var tx_response = await SuiApi.Client.MoveCallAsync(_moveCallTransaction);
            if (tx_response.IsSuccess)
            {
                var dry_tx = await SuiApi.Client.DryRunTransactionBlockAsync(tx_response.Result.TxBytes);

                ConfirmTxData confirmTxData = new ConfirmTxData
                {
                    Title = "Register Resident License?",
                    Tx = tx_response.Result.TxBytes,
                    Tcs = tcs_TxResult,
                    Gas = GetGasFee(dry_tx.RawRpcResponse),
                    Fee = "0",
                    BalanceChange = GetBalanceChange(dry_tx.RawRpcResponse)

                };

                UIManager.Instance.ShowPopup(PopupName.SuiWallet, new Dictionary<string, object>{
                    {"isConfirmTx", true},
                    {"confirmTxData", confirmTxData},
                });
            }
            else
            {
                tcs_TxResult.TrySetResult(new ContractRespone
                {
                    IsSucess = false,
                    Data = null,
                    Message = tx_response.ErrorMessage
                });
                Debug.Log(tx_response.ErrorMessage);
            }

            var tcs_result = await tcs_TxResult.Task;
           
            ContractRespone contractRespone = new ContractRespone();
            if (tcs_result.IsSucess)
            {
                contractRespone.IsSucess = true;
                var data = tcs_result.Data as RpcResult<TransactionBlockResponse>;
                string mutatedNft = string.Empty;
                Debug.Log(data.RawRpcResponse);
                
                foreach (var objectChange in data.Result.ObjectChanges)
                {
                    if (objectChange.Type == ObjectChangeType.Mutated )
                        {
                            var mutatedObjectChange = objectChange as MutatedObjectChange;
                            if (mutatedObjectChange != null && mutatedObjectChange.ObjectType  == $"{PACKAGE_ID_ADDRESS}::master::MasterNFT")
                            {   
                                mutatedNft = mutatedObjectChange.ObjectId;
                            }
                        }
                }
                var objectResut = await SuiApi.Client.GetObjectAsync(mutatedNft, new ObjectDataOptions
                {
                    ShowContent = true, ShowDisplay = true,
                });
                var masterNftContent = objectResut.Result.Data.Content as MoveObjectData;
                string masterNftJson = JsonConvert.SerializeObject(masterNftContent.Fields, jsonSettings);
                JObject masterNftJObject = JObject.Parse(masterNftJson);
                string resident_license = masterNftJObject["resident_license"].ToString();
                contractRespone.Data = objectResut.IsSuccess? resident_license : null;
          
            } else {
                contractRespone.IsSucess = false;
                contractRespone.Message = tcs_result.Message;
            }

            return JsonConvert.SerializeObject(contractRespone);
        }

        public static async UniTask<string> UnresidentLicense(string master)
        {
            tcs_TxResult = new UniTaskCompletionSource<ContractRespone>();
            _moveCallTransaction.Module = "master";
            _moveCallTransaction.Function = "unregister_resident_license";
            _moveCallTransaction.Arguments = new object[] {
                LAND_ADDRESS,
                master,
            };

            var tx_response = await SuiApi.Client.MoveCallAsync(_moveCallTransaction);
            if (tx_response.IsSuccess)
            {
                var dry_tx = await SuiApi.Client.DryRunTransactionBlockAsync(tx_response.Result.TxBytes);

                ConfirmTxData confirmTxData = new ConfirmTxData
                {
                    Title = "Unregister Resident License?",
                    Tx = tx_response.Result.TxBytes,
                    Tcs = tcs_TxResult,
                    Gas = GetGasFee(dry_tx.RawRpcResponse),
                    Fee = "0",
                    BalanceChange = GetBalanceChange(dry_tx.RawRpcResponse)

                };

                UIManager.Instance.ShowPopup(PopupName.SuiWallet, new Dictionary<string, object>{
                    {"isConfirmTx", true},
                    {"confirmTxData", confirmTxData},
                });
            }
            else
            {
                tcs_TxResult.TrySetResult(new ContractRespone
                {
                    IsSucess = false,
                    Data = null,
                    Message = tx_response.ErrorMessage
                });
                Debug.Log(tx_response.ErrorMessage);
            }

            var tcs_result = await tcs_TxResult.Task;
           
            ContractRespone contractRespone = new ContractRespone();
            if (tcs_result.IsSucess)
            {
                contractRespone.IsSucess = true;
          
            } else {
                contractRespone.IsSucess = false;
                contractRespone.Message = tcs_result.Message;
            }

            return JsonConvert.SerializeObject(contractRespone);
        }
        #endregion
   
        #region  Mint Mineral
        public static async UniTask<string> MintMineral(string master, string type)
        {
            tcs_TxResult = new UniTaskCompletionSource<ContractRespone>();
            _moveCallTransaction.Module = "master";
            _moveCallTransaction.Function = type;
            _moveCallTransaction.Arguments = new object[] {
                LAND_ADDRESS,
                master,
                ClockAddress
            };
            var tx_response = await SuiApi.Client.MoveCallAsync(_moveCallTransaction);
            Debug.Log("stat 1 " +tx_response.IsSuccess);
            if (tx_response.IsSuccess)
            {
                var dry_tx = await SuiApi.Client.DryRunTransactionBlockAsync(tx_response.Result.TxBytes);
                Debug.Log("stat 1 " +dry_tx.IsSuccess);
                ConfirmTxData confirmTxData = new ConfirmTxData
                {
                    Title = "Mint this Tree?",
                    Tx = tx_response.Result.TxBytes,
                    Tcs = tcs_TxResult,
                    Gas = GetGasFee(dry_tx.RawRpcResponse),
                    Fee = "0",
                    BalanceChange = GetBalanceChange(dry_tx.RawRpcResponse)

                };

                UIManager.Instance.ShowPopup(PopupName.SuiWallet, new Dictionary<string, object>{
                    {"isConfirmTx", true},
                    {"confirmTxData", confirmTxData},
                });
            }
            else
            {
                tcs_TxResult.TrySetResult(new ContractRespone
                {
                    IsSucess = false,
                    Data = null,
                    Message = tx_response.ErrorMessage
                });
                Debug.Log(tx_response.ErrorMessage);
            }

            var tcs_result = await tcs_TxResult.Task;
            Debug.Log(tcs_result.IsSucess);
            ContractRespone contractRespone = new ContractRespone();
           
            if (tcs_result.IsSucess)
            {
                contractRespone.IsSucess = true;
                var data = tcs_result.Data as RpcResult<TransactionBlockResponse>;
                Debug.Log(data.RawRpcResponse);
            } else {
                contractRespone.IsSucess = false;
                contractRespone.Message = tcs_result.Message;
            }

            return JsonConvert.SerializeObject(contractRespone);
        }

    }
    #endregion
}
