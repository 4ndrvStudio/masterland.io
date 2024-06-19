using System.Collections.Generic;
using UnityEngine;

namespace masterland.Inventory
{
    public class Inventory : Singleton<Inventory>
    {
        [SerializeField] private List<Item> _items = new();
        [SerializeField] private List<Item> _quickItems = new();
        
        private void Start() 
        {

        }

    }
}
