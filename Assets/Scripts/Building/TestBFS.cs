using UnityEngine;
using System.Collections.Generic;
namespace masterland
{
    public class TestBFS : MonoBehaviour
    {
    public bool IsConnected(GameObject objectStart, GameObject objectTarget, Dictionary<GameObject, List<GameObject>> connections)
    {

        HashSet<GameObject> visited = new HashSet<GameObject>();
        return DFS(objectStart, objectTarget, connections, visited);
    }


    private bool DFS(GameObject current, GameObject target, Dictionary<GameObject, List<GameObject>> connections, HashSet<GameObject> visited)
    {
        if (current == target)
        {
            return true; 
        }

        visited.Add(current); 

        if (connections.ContainsKey(current))
        {
            foreach (GameObject neighbor in connections[current])
            {
                if (!visited.Contains(neighbor))
                {
                    if (DFS(neighbor, target, connections, visited)) 
                    {
                        return true;
                    }
                }
            }
        }

        return false; 
    }
    }
}
