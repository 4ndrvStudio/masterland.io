using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapMagic.Core;
using MapMagic.Products;
using MapMagic.Terrains;
using UnityEngine.UI;
using Den.Tools;
using System.Linq;
using Unity.Mathematics;

namespace masterland
{
    [System.Serializable]
    public class MineralNetworkData 
    {
        public int id;
        public Vector3 pos;
    }

    public class MapManager : Singleton<MapManager>
    {
        [SerializeField] private MapMagicObject _mapMagicObject;
        [SerializeField] private TerrainTile _terrainTile;
        [SerializeField] private List<MineralNetworkData> _treeMineralList = new();
        [SerializeField] private List<MineralNetworkData> _stoneMineralList = new();
        [SerializeField] Button _addtree;
        [SerializeField] Button _refeshMap;
        [SerializeField] GameObject TreePrefabs;
        
        void OnEnable() 
        {
            TerrainTile.OnTileApplied += TerrainTile_OnTileApplied;
        }
        void Start()
        {
            _refeshMap.onClick.AddListener(Generate);
            _addtree.onClick.AddListener(GenerateTree);
            Generate();
        }

        void OnDisable()
        {
            TerrainTile.OnTileApplied -= TerrainTile_OnTileApplied;

        }

        public void TerrainTile_OnTileApplied(TerrainTile tile, TileData tileData, StopToken stopToken)
        {
            if(Application.isPlaying && !tileData.isDraft) 
            {
                //clone objects tile data
                var st = _terrainTile.objectsPool.GetDrafts(0);
                var tt = _terrainTile.objectsPool.GetDrafts(1);
                
                foreach(var i in st) 
                {
                    _stoneMineralList.Add(new MineralNetworkData{
                        id = i.id,
                        pos= i.pos
                    });
                }

                foreach(var i in tt) 
                {
                    _treeMineralList.Add(new MineralNetworkData{
                        id = i.id,
                        pos= i.pos
                    });
                }

                GenerateTree();
            }
            
        }


        public void Generate() {
            _mapMagicObject.graph.random.Seed = 1;
            _mapMagicObject.Refresh(true);
        }

        public void GenerateTree() {
            _treeMineralList.ForEach(i => {
                Instantiate(TreePrefabs,i.pos + _mapMagicObject.transform.position, quaternion.identity);
            });
        }


    }

}
