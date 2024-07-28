using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace masterland.InteractObject
{
    public interface IInteractObject 
    {
        void Interact(NetworkConnection connection) {}
    }
}
