using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace masterland.Weapon
{
    using Master;
    public class Weapon : MonoBehaviour
    {
       [HideInInspector] public Master MasterOwner;
        public int Damage;
    }
}
