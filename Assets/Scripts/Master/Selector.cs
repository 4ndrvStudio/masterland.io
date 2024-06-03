using UnityEngine;

namespace masterland.Master
{
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

                if (Physics.Raycast(ray, out hit, 50f, layerMask))
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
                        }
                    }
                    else
                    {
                        if (_currentOutlineEffect != null)
                        {
                            _currentOutlineEffect.DisableOutline();
                            _currentOutlineEffect = null;
                        }
                    }
                }
                else
                {
                    if (_currentOutlineEffect != null)
                    {
                        _currentOutlineEffect.DisableOutline();
                        _currentOutlineEffect = null;
                    }
                }
            }
        }
    }
}
