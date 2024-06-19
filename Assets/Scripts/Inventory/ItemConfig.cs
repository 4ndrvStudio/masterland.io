using UnityEngine;

namespace masterland.Inventory
{
    public enum ItemType 
    {
        Weapon,
    }

    [CreateAssetMenu(fileName = "Item", menuName = "masterland/Item")]
    public class ItemConfig : ScriptableObject
    {
        public string Name;
        public int Id;
        public ItemType ItemType;
        public GameObject Prefab;
        public Sprite IconUI;
    }
}
