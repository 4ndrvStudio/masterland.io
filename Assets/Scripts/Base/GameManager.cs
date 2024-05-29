using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using Unity.VisualScripting;
using UnityEngine;

namespace masterland.Manager
{
    using Map;
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
        private Tugboat _tugBoat;
        private LocalConnectionState _clientState;
        private LocalConnectionState _serverState;
        public string PlayerName = "Player @";

        // Start is called before the first frame update
        void Awake()
        {
            if (Instance == null)
                Instance = this;

            _networkManager = _networkManager = FindFirstObjectByType<NetworkManager>();
            _tugBoat = FindFirstObjectByType<Tugboat>();
        
            _uiManager.SetActive(!_isServer);
         
            if (_isDev)
            {
                _tugBoat.SetClientAddress("127.0.0.1");
                if (_isServer)
                {
                    StartServer();
                }
            }
            else
            {
                _tugBoat.SetClientAddress(Sanitize(_serverIpAddress));
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


        private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
        {
            _clientState = obj.ConnectionState;
             if(_clientState == LocalConnectionState.Starting) {
                Debug.Log("Starting ....");
           }
           if(_clientState == LocalConnectionState.Stopped) {
                Debug.Log("Stopped ....");
           }
           if(_clientState == LocalConnectionState.Started) {
                Debug.Log("Started ....");
           }
           if(_clientState == LocalConnectionState.Stopping) {
                Debug.Log("Stopping ....");
           }
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
            _tugBoat.SetPort(_port);
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
            _tugBoat.SetPort(port);
            if (_clientState != LocalConnectionState.Stopped)
                _networkManager.ClientManager.StopConnection();

            StartCoroutine(IEStartClient());
        }

        IEnumerator IEStartClient()
        {
            UIManager.Instance.ToggleWaiting(true);
            UIManager.Instance.ToggleLoginPanel(false);
            Debug.Log("Init client");
            yield return new WaitUntil(() => _clientState == LocalConnectionState.Stopped);
            Debug.Log("Stop client");
            _networkManager.ClientManager.StartConnection();
            Debug.Log("start connect");

            yield return new WaitUntil(() => _clientState == LocalConnectionState.Started);
            Debug.Log("start client");
            yield return new WaitForSeconds(2f);
            Debug.Log("Loaded");
            UIManager.Instance.ToggleWaiting(false);
        }

        public void Play() => StartClient(_port);


        static string Sanitize(string dirtyString)
        {
            return Regex.Replace(dirtyString, "[^A-Za-z0-9.]", "");
        }





    }
}
