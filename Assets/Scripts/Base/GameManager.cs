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

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private GameObject _uiManager;
        [SerializeField] private string _serverIpAddress = "171.244.143.28";
        [SerializeField] private ushort _port = 7777;
        [SerializeField] private bool _isDev;
        [SerializeField] private bool _isServer;
        [SerializeField] private Transform _spawnPos;
        private NetworkManager _networkManager;
        private Multipass _mp;
        private Tugboat _tugboat;
        private Bayou _bayou;
        private LocalConnectionState _clientState;
        private LocalConnectionState _serverState;
        public string PlayerName = "Player @";

        // Start is called before the first frame update
        void Awake()
        {
            if (Instance == null)
                Instance = this;

            LoadGame();
            
            _networkManager = _networkManager = FindFirstObjectByType<NetworkManager>();
            _mp = FindFirstObjectByType<Multipass>();       
            _tugboat = FindFirstObjectByType<Tugboat>();   
            _bayou = FindFirstObjectByType<Bayou>();

            #if UNITY_WEBGL && !UNITY_EDITOR
                Debug.Log("Set Port to Bayou");
                _mp.SetClientTransport<Bayou>();
            #else
                Debug.Log("Set Port to Tugboat");
                _mp.SetClientTransport<Tugboat>();
            #endif

            _uiManager.SetActive(!_isServer);
         
            if (_isDev)
            {
                _networkManager.TransportManager.Transport.SetClientAddress("192.168.1.5");
                if (_isServer)
                {
                    StartServer();
                }
            }
            else
            {
                _networkManager.TransportManager.Transport.SetClientAddress(Sanitize(_serverIpAddress));
                if (_isServer)
                {
                    StartServer();
                }
            }

            if (_networkManager == null)
            {
                Debug.LogError("NetworkManager not found, HUD will not function.");
                return;
            }
            else
            {
                _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
                _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
                _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
            }

        }

        public async void LoadGame() {
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




        private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
        {
            _clientState = obj.ConnectionState;
        }


        private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
        {
            _serverState = obj.ConnectionState;
            if (_serverState == LocalConnectionState.Started)
            { 
                Debug.Log("Server Started ....");
            }
        }


          private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
        {
            if (!asServer)
                return;
            if (_playerPrefab == null)
            {
                Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
                return;
            }
         
            Vector3 randomDirection = UnityEngine.Random.insideUnitCircle.normalized * 20f;
            Vector3 newPosition = Map.Instance.MapData.SpawnPos.position + new Vector3(randomDirection.x, 5, randomDirection.z);
            Quaternion randomRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
            _playerPrefab.transform.position = newPosition;
            _playerPrefab.transform.rotation = randomRotation;

            NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, _playerPrefab.transform.position, _playerPrefab.transform.rotation, true);
            _networkManager.ServerManager.Spawn(nob, conn);
            _networkManager.SceneManager.AddOwnerToDefaultScene(nob);
        }

        public void StartServer()
        {
            _tugboat.SetPort(_port);
            _bayou.SetPort(_port);
            if (_serverState != LocalConnectionState.Stopped)
                _networkManager.ServerManager.StopConnection(true);
            StartCoroutine(IEStartServer());
        }

        IEnumerator IEStartServer()
        {
            yield return new WaitUntil(() => _serverState == LocalConnectionState.Stopped);
            _networkManager.ServerManager.StartConnection();
        
        }

        private void StartClient(ushort port)
        {
            _tugboat.SetPort(port);
            _bayou.SetPort(port);
        
            if (_clientState != LocalConnectionState.Stopped)
                _networkManager.ClientManager.StopConnection();

            StartCoroutine(IEStartClient());
        }

        IEnumerator IEStartClient()
        {
            UIManager.Instance.ShowPopup(PopupName.Waiting);
            yield return new WaitUntil(() => _clientState == LocalConnectionState.Stopped);
            _networkManager.ClientManager.StartConnection();
            yield return new WaitUntil(() => _clientState == LocalConnectionState.Started);
            yield return new WaitForSeconds(2f);
            UIManager.Instance.ToggleView(ViewName.Gameplay);
            UIManager.Instance.HidePopup(PopupName.Waiting);
        }

        public void Play() => StartClient(_port);


        static string Sanitize(string dirtyString)
        {
            return Regex.Replace(dirtyString, "[^A-Za-z0-9.]", "");
        }





    }
}
