using UnityEngine;

namespace masterland.UI
{
    public class UITab : MonoBehaviour
    {
        public bool IsOpen;

        public virtual void Show() 
        {
            this.gameObject.SetActive(true);
            IsOpen = true;
        }
        public virtual void Hide() 
        {
            this.gameObject.SetActive(false);
            IsOpen = false;
            StopAllCoroutines();
        }

        private void OnDisable() {
            StopAllCoroutines();
        }
    }
}
