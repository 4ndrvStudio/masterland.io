using UnityEngine;

namespace masterland.Building
{
    [CreateAssetMenu(fileName = "New BuildingComponent", menuName = "BuildingComponent", order = 1)]
    public class BuildingComponent : ScriptableObject
    {
        public GameObject PreviewPrefab;
        public GameObject BuildingElementPrefab;
        public CircularMenuElement MenuElement;
    }
}

