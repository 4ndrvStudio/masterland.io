using UnityEngine;

namespace masterland
{
    public class SelectedObject : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _material;
        
        [SerializeField]  private Color _rootColor;

        private void Start() 
        {
            _material = new Material(_renderer.material);
            _renderer.material = _material;
            _rootColor = _material.GetColor("_BaseColor");
        }

        public void EnableOutline() 
        {
            _material.SetColor("_BaseColor", Color.green);
    
        }

        public void DisableOutline() 
        {
            _material.SetColor("_BaseColor", _rootColor);
        }
    }
}
