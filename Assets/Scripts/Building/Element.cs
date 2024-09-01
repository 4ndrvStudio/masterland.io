using System.Collections.Generic;
using UnityEngine;

namespace masterland.Building
{
    public class Element : MonoBehaviour
    {
        public BuildingComponent BuildingComponent;
        [SerializeField] private Material _deleteMaterial;
        [SerializeField] private MeshRenderer _meshrenderer;
        
        private Material[] _materials;
        private Material[] _deleteMaterials;
        
        [SerializeField] private List<Transform> _collideTransform;

        private void Awake()
        {
            _materials = new Material[_meshrenderer.materials.Length];
            _deleteMaterials = new Material[_materials.Length];
            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i] = new Material(_meshrenderer.materials[i]);
                _deleteMaterials[i] = _deleteMaterial;
            }
        }

        public void CanDelete(bool isCan) 
        {
            _meshrenderer.materials = isCan ? _deleteMaterials : _materials;
        }

        
    }
}