using UnityEngine;

namespace masterland.Interact
{
    using InteractObject;

    public class SelectedObject : MonoBehaviour
    {
        public InteractType InteractType;
        public IInteractObject CurrentSelectedObject;

        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material[] _material;
        [SerializeField]  private Color[] _rootColor;

        private void Start() 
        {
            CurrentSelectedObject = GetComponent<IInteractObject>();

            _material = new Material[_renderer.materials.Length];
            _rootColor = new Color[_renderer.materials.Length];

            for (int i = 0; i< _renderer.materials.Length; i++) 
            {
                 _material[i]= new Material(_renderer.materials[i]);
                
                 if(_material[i].HasColor("_BaseColor"))
                    _rootColor[i] = _material[i].GetColor("_BaseColor");
            }

            _renderer.materials = _material;
        }

        public void EnableOutline() 
        {
            for (int i = 0; i< _material.Length; i++) 
            {   
                if(_material[i].HasColor("_BaseColor") && _material[i].GetColor("_BaseColor") != Color.green) {
                    _material[i].SetColor("_BaseColor", Color.green);
                }
            }
    
        }

        public void DisableOutline() 
        {
            for (int i = 0; i< _material.Length; i++) 
            {
                if( _material[i].HasColor("_BaseColor"))
                    _material[i].SetColor("_BaseColor",_rootColor[i]);
            }
        }
    }
}
