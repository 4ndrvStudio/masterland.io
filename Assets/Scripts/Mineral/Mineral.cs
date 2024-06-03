using FishNet.Object;
using UnityEngine;

namespace masterland.Mineral
{
    public enum MineralType 
    {
        Stone,
        Wood
    }

    public class Mineral : NetworkBehaviour
    {
        public MineralType Type;
        
    }
}
