using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace masterland.UI
{
    public class View_Gameplay : UIView
    {
        public override void Show(Dictionary<string, object> customProperties = null)
        {
            base.Show(customProperties);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if(UIController.Instance)
                UIController.Instance.Standby = false;
        }

        public override void Hide() 
        {
            base.Hide();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if(UIController.Instance)
                UIController.Instance.Standby = true;
        }
    }
}
