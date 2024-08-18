using UnityEngine;

namespace masterland.Building
{
    [CreateAssetMenu(fileName = "New BuildingComponent", menuName = "masterland/BuildingComponent")]
    public class BuildingComponent : ScriptableObject
    {
        public GameObject PreviewPrefab;
        public GameObject BuildingElementPrefab;
        public CircularMenuElement MenuElement;
    }
}

