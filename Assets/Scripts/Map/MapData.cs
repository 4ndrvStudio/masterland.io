using System.Collections.Generic;
using Den.Tools;
using FishNet.Object;
using UnityEngine;

namespace masterland.Map
{
    using Mineral;

    [System.Serializable]
    public class TransitionData 
    {
        public MineralType MineralType;
        public List<Transition> Drafts;
        public List<NetworkObject> MineralObList;
    }

    public class MapData : MonoBehaviour
    {
        public int MapID;
        public List<TransitionData> TransitionDatas = new();
        public Transform SpawnPos;
    }
}
