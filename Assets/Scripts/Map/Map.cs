using FishNet.Object;
using UnityEngine;
using System.Collections.Generic;
using Den.Tools;
using GameKit.Dependencies.Utilities;

namespace masterland.Map
{
    [System.Serializable]
    public class MineralSpawner 
    {
        public Vector3 Position;
        public Vector3 Scale;
        public NetworkObject MineralOb;

        public MineralSpawner(Vector3 position, Vector3 scale) 
        {
            Position = position;
            Scale = scale;
        }
    }

    public class Map : SingletonNetwork<Map>
    {
        [HideInInspector]
        public MapData MapData;
        public GameObject MapDataObTest;
        
        public List<MineralSpawner> StoneList = new();
        public List<MineralSpawner> TreeList = new();

        private List<NetworkObject> StoneObList = new();
        private List<NetworkObject> TreeObList = new();

        void Start() 
        {
            //LoadMapTest();
            //InitMineralSpawn();
        }

        public override void OnStartNetwork()
        {
            base.OnStartServer();
            LoadMapTest();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            if(!IsServerInitialized) 
                return;
            InitMineralSpawn();
        }

        void LoadMapTest() 
        {
            if(MapDataObTest == null)
                return;
            GameObject map = Instantiate(MapDataObTest);    
            MapData = map.GetComponent<MapData>();
        }

        void Loadmap(int id) 
        {
            //LoadMap from Server
        }

       

        void InitMineralSpawn() 
        {
            var stoneData = MapData.TransitionDatas.Find(item => item.MineralType == Mineral.MineralType.Stone);
            var treeData = MapData.TransitionDatas.Find(item => item.MineralType == Mineral.MineralType.Wood);
            
            //instantiate data

            foreach(Transition transition in stoneData.Drafts) 
            {
                StoneList.Add(new MineralSpawner(transition.pos, transition.scale));
            }

            foreach(Transition transition in treeData.Drafts) 
            {
                TreeList.Add(new MineralSpawner(transition.pos, transition.scale));
            }

            //add mineral prefabs
            StoneObList = stoneData.MineralObList;
            TreeObList = treeData.MineralObList;

            //spawn all mineral
            foreach(MineralSpawner mineralSpawner in StoneList) 
            {
             
                mineralSpawner.MineralOb = SpawnMineral(StoneObList[Random.Range(0, StoneObList.Count)],mineralSpawner.Position, mineralSpawner.Scale);
                ServerManager.Spawn(mineralSpawner.MineralOb);
            }

            foreach(MineralSpawner mineralSpawner in TreeList) 
            {
                mineralSpawner.MineralOb = SpawnMineral(TreeObList[Random.Range(0, TreeObList.Count)], mineralSpawner.Position, mineralSpawner.Scale);
                ServerManager.Spawn(mineralSpawner.MineralOb);
            }

        }

        private NetworkObject SpawnMineral(NetworkObject networkObject, Vector3 position, Vector3 scale) 
        {
            networkObject.transform.SetScale(scale);
            return Instantiate(networkObject,position,RandomQuaternion());
        }
    

        public Quaternion RandomQuaternion() => Quaternion.Euler(0, Random.Range(0, 360), 0);
    }

}
