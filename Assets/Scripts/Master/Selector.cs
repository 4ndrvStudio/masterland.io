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
        public LayerMask layerMask;
        public float checkInterval = 0.1f;
        private float timer = 0f;
        private SelectedObject _currentOutlineEffect;

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
                        if (_currentOutlineEffect != newOutlineEffect)
                        {
                            if (_currentOutlineEffect != null)
                            {
                                _currentOutlineEffect.DisableOutline();
                            }

                            newOutlineEffect.EnableOutline();
                            _currentOutlineEffect = newOutlineEffect;
                          
                            UIController.Instance.ShowInteractTutorial(_currentOutlineEffect.InteractType.ConvertEnumToStringWithSpace());
                        }
                    }
                    else
                    {
                        if (_currentOutlineEffect != null)
                        {
                            _currentOutlineEffect.DisableOutline();
                            _currentOutlineEffect = null;
                            UIController.Instance.HideInteractTutorial();
                        }
                    }
                }
                else
                {
                    if (_currentOutlineEffect != null)
                    {
                        _currentOutlineEffect.DisableOutline();
                        UIController.Instance.HideInteractTutorial();
                        _currentOutlineEffect = null;
                    }
                }
            }
        }
    }
}
