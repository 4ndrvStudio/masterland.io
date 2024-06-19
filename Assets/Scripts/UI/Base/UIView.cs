using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace masterland.UI
{
    public enum ViewName
    {
        None,
        Init = 1,
        Login = 2,
        Menu = 3,
        Gameplay = 100
    }
    public class UIView : MonoBehaviour
    {
        [SerializeField] private ViewName _viewName = ViewName.None;
        public ViewName ViewName => _viewName;
        [SerializeField] private List<UIElement> _elementList = new();
        

        public virtual void CountinueStep(Dictionary<string, object> customProperties = null) { }

        public virtual void Show(Dictionary<string, object> customProperties = null)
        {
            gameObject.SetActive(true);

            if(ViewName == ViewName.Login) 
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else 
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            //update element
            foreach(UIElement el in _elementList) {
                el.Setup();
            }
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

