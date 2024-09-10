using UnityEngine;

namespace masterland.Building
{
    public class Foundation : MonoBehaviour
    {
        public Material BuildableMaterial, UnbuildableMaterial;
        private Material[] materials;
        private MeshRenderer meshrenderer;
        public bool IsPreview;
        
        private void Awake()
        {
            meshrenderer = GetComponent<MeshRenderer>();
            materials = meshrenderer.materials;
        }


        private void SetMaterials(Material _material)
        {
            if (materials != null && materials[0] == _material)
            {
                return;
            }

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = _material;
            }
            meshrenderer.materials = materials;
        }

        private void OnTriggerEnter(Collider _other)
        {
            if(!IsPreview)
                return;
            if(_other.tag == "Ground")
           
            SetMaterials(_other.tag == "Ground"? BuildableMaterial : UnbuildableMaterial);
        }
    }
}
