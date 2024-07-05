using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using FishNet.Transporting.Bayou;
using UnityEngine;

namespace masterland.Manager
{
    using FishNet.Transporting.Multipass;
    using Map;
    using masterland.UI;
    using masterland.Wallet;
    using Data;

    public class GameManager : Singleton<GameManager>
    {
        public bool IsDev;
        public bool IsServer;
        
        void Awake()
        {
            Initialize();
        }

        public void Initialize() 
        {
            Network.Instance.SetupNetwork();
            if(!IsServer) 
            {
                LoadGame();
            }
        }

        public async void LoadGame() 
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
                await Data.Instance.InitData();
                UIManager.Instance.ToggleView(ViewName.Menu);
            #else
                //load data 
                if(SuiWallet.GetActiveAddress() == "0x")
                    UIManager.Instance.ToggleView(ViewName.Login);
                else 
                {
                    WalletInGame.Setup();
                    await Data.Instance.InitData();
                    UIManager.Instance.ToggleView(ViewName.Menu);
                }   
            #endif
        }

        public void EnterMap() => Network.Instance.StartClient();

    }
}
