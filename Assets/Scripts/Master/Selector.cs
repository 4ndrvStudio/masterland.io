using UnityEngine;

namespace masterland.Interact
{
    using Mineral;
    using masterland.UI;
    using System;

    public enum InteractType 
    {
        Mint_Stone = 1,
        Mint_Wood = 2,
    }


    public class Selector : MonoBehaviour
    {
        [HideInInspector]
        public SelectedObject CurrentSelectedObject;
        public LayerMask layerMask;
        public float checkInterval = 0.1f;
        private float timer = 0f;
      
        void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer >= checkInterval)
            {
                timer = 0f;

                Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2);
                Ray ray = Camera.main.ScreenPointToRay(screenCenter);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 10f, layerMask))
                {
                    SelectedObject newOutlineEffect = hit.collider.GetComponent<SelectedObject>();

                    if (newOutlineEffect != null)
                    {
                        if (CurrentSelectedObject != newOutlineEffect)
                        {
                            if (CurrentSelectedObject != null)
                            {
                                CurrentSelectedObject.DisableOutline();
                            }

                            newOutlineEffect.EnableOutline();
                            CurrentSelectedObject = newOutlineEffect;
                            UIController.Instance.ShowInteractTutorial(CurrentSelectedObject.InteractType.ConvertEnumToStringWithSpace());
                        }
                    }
                    else
                    {
                        if (CurrentSelectedObject != null)
                        {
                            CurrentSelectedObject.DisableOutline();
                            CurrentSelectedObject = null;
                            UIController.Instance.HideInteractTutorial();
                        }
                    }
                }
                else
                {
                    if (CurrentSelectedObject != null)
                    {
                        CurrentSelectedObject.DisableOutline();
                        UIController.Instance.HideInteractTutorial();
                        CurrentSelectedObject = null;
                    }
                }
            }
        }
    }
}
