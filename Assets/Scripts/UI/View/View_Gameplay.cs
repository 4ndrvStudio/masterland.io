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
        }

        public override void Hide() 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
