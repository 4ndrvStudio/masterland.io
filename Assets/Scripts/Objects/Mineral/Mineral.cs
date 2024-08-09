using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace masterland.Mineral
{
    using InteractObject;
    using masterland.Wallet;
    using Data;
    using masterland.UI;
    using Master;
    using System.Collections;

    public enum MineralType 
    {
        Stone,
        Wood
    }

    public enum MineralState 
    {
        None,
        Minting,
    }

    public class Mineral : NetworkBehaviour, IInteractObject
    {
        public MineralType Type;

        [AllowMutableSyncType]
        public SyncVar<MineralState> State = new SyncVar<MineralState>(MineralState.None, new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));

        private NetworkConnection _currentMiner = null;
        
        [ServerRpc(RequireOwnership = false)]
        private void Server_RequireMint(NetworkConnection conn) 
        {
   
            if(State.Value == MineralState.Minting)
              return;
            _currentMiner = conn;
            State.Value = MineralState.Minting;
            Client_AllowMint(conn);
            StartCoroutine(ResetTree());
        }

        [ServerRpc(RequireOwnership = false)]
        private void Server_MintResult(NetworkConnection conn, bool isComplete) 
        {
            if(_currentMiner != conn)
                return;
            if(isComplete) 
            {
                NetworkManager.ServerManager.Despawn(this.gameObject);
            }
            _currentMiner = null;
            State.Value = MineralState.None;
        }

        [ObserversRpc][TargetRpc]
        private void Client_AllowMint(NetworkConnection target)
        {
            ExecuteMint(target);
        }

        public async void ExecuteMint(NetworkConnection conn) 
        {
            ContractRespone contractRespone = await WalletInteractor.Instance.MintMineral(Data.Instance.MasterData.Id, Type == MineralType.Wood ?"mint_wood" : "mint_stone");
            if(contractRespone.IsSuccess) 
            {   
                Server_MintResult(conn, true);
                var toastModel = new ToastModel 
                {
                    IsSuccess = true,
                    Title = Type  == MineralType.Wood ? "Mint Wood Complete" : "Mint Stone Complete",
                    Description = Type == MineralType.Wood ?  "+10 Wood" : "+10 Stone"
                };

                UIToast.Instance.Show(toastModel);
            } 
            else 
            {
                Server_MintResult(conn, false);
                var toastModel = new ToastModel 
                {
                    IsSuccess = false,
                    Title = "Mint Fail!",
                    Description = contractRespone.Message
                };
                UIToast.Instance.Show(toastModel, 3500);
            }
        }

        public void Interact(NetworkConnection connection)  
        {
            if(connection != Master.Local.Owner)
                return;

            if(State.Value == MineralState.Minting) 
            {
               var toastModel = new ToastModel 
                {
                    IsSuccess = false,
                    Title = "Mint Fail!",
                    Description = "Someone minting this tree"
                };
                UIToast.Instance.Show(toastModel);
                return;
            }

            Server_RequireMint(connection);
        }


        IEnumerator ResetTree() 
        {
            yield return new WaitForSeconds(10f);
            _currentMiner = null;
            State.Value = MineralState.None;
        }

    }
}
