using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace masterland.UI
{
    public enum PopupName
    {
        None=0,
        Waiting=1,
        SuiWallet=2,
        ResidentLicense=3,
    }

    public class UIPopup : MonoBehaviour
    {
        [SerializeField] private PopupName _popupName = PopupName.None;
        public PopupName PopupName => _popupName;

        protected Dictionary<string, object> _customProperties;


        public virtual void Show(Dictionary<string, object> customProperties = null)
        {
            this._customProperties = customProperties;
            gameObject.SetActive(true);
            
            if(UIManager.Instance.CurrentView.ViewName == ViewName.Gameplay) 
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public virtual void Hide()
        {
            _customProperties = null;
            gameObject.SetActive(false);
           
            if(UIManager.Instance.CurrentView.ViewName == ViewName.Gameplay) 
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

}

