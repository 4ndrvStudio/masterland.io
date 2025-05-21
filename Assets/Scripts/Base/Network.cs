using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using FishNet.Transporting.Bayou;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using UnityEngine;

namespace masterland.Manager
{
    using System.Collections;
    using System.Text.RegularExpressions;
    using Cysharp.Threading.Tasks;
    using Map;
    using UI;

    public class Network : Singleton<Network>
    {
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private GameObject _uiManager;
        [SerializeField] private string _serverIpAddress = "171.244.143.28";
        [SerializeField] private ushort _port = 7777;
     
        [SerializeField] private Transform _spawnPos;
        [HideInInspector] public NetworkManager _networkManager;
        private Multipass _mp;
        private Tugboat _tugboat;
        private Bayou _bayou;
        private LocalConnectionState _clientState;
        private LocalConnectionState _serverState;
        public string PlayerName = "Player @";


        public void SetupNetwork() 
        {
            _networkManager = FindFirstObjectByType<NetworkManager>();
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

            _uiManager.SetActive(!GameManager.Instance.IsServer);

            _networkManager.TransportManager.Transport.SetClientAddress(GameManager.Instance.IsDev ? "localhost" :Sanitize(_serverIpAddress));
           
            if (GameManager.Instance.IsServer || GameManager.Instance.IsDev)
            {
               StartServer();
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
        
        private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
        {
            _clientState = obj.ConnectionState;
            Debug.Log(_clientState);
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

        public void StartClient()
        {
            _tugboat.SetPort(_port);
            _bayou.SetPort(_port);
        
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

        static string Sanitize(string dirtyString)
        {
            return Regex.Replace(dirtyString, "[^A-Za-z0-9.]", "");
        }

    }
}
