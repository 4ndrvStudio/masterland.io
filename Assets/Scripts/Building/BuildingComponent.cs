using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace masterland.Building
{
    public enum BuildingComponentType 
    {
        Floor,
        Wall,
        Door,
        Roof,
        WallCut
    }

    [CreateAssetMenu(fileName = "New BuildingComponent", menuName = "masterland/BuildingComponent")]
    public class BuildingComponent : ScriptableObject
    {
        public GameObject PreviewPrefab;
        public GameObject BuildingElementPrefab;
        public CircularMenuElement MenuElement;
        public int Stone;
        public int Wood;
        public BuildingComponentType Type;
    }

    [System.Serializable]
    public class BuildingData 
    {
        public SerializedVector3 Position;
        public SerializedVector3 EulerAngles;
        public List<BuildingComponentData> Components;
    }

    [System.Serializable]
    public class BuildingComponentData
    {
        public BuildingComponentType Type;
        public SerializedVector3 Position;
        public SerializedVector3 EulerAngles;
    }

    [System.Serializable]
    public class SerializedVector3
    {
        public float x;
        public float y;
        public float z;

        [JsonConstructor]
        public SerializedVector3(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public SerializedVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

}

