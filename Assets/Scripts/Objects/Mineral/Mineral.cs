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
            Debug.Log("mint Call");
            if(State.Value == MineralState.Minting)
              return;
            _currentMiner = conn;
            State.Value = MineralState.Minting;
            Client_AllowMint(conn);
        }

        [ServerRpc(RequireOwnership = false)]
        private void Server_MintResult(NetworkConnection conn, bool isComplete) 
        {
            if(_currentMiner != conn)
                return;

            if(isComplete) 
            {
                NetworkManager.ServerManager.Despawn(this.gameObject);

            } else {
                _currentMiner = null;
                State.Value = MineralState.None;
            }
        }

        [ObserversRpc][TargetRpc]
        private void Client_AllowMint(NetworkConnection target)
        {
            ExecuteMint();
        }

        public async void ExecuteMint() 
        {
            ContractRespone contractRespone = await WalletInteractor.Instance.MintMineral(Data.Instance.MasterData.Id,"mint_wood");

        }

        public void Interact(NetworkConnection connection)  
        {
            Debug.Log("Mintt");
            if(State.Value == MineralState.Minting) 
            {
                Debug.Log("Someone Minting This Tree");
                return;
            }

            Server_RequireMint(connection);
        }

    }
}
