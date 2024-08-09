using UnityEngine;

namespace masterland.UI
{
    public class UIToast : Singleton<UIToast>
    {
        [SerializeField] private GameObject _toastHolder;
        [SerializeField] private GameObject _toastOb;

        public void Show(ToastModel toastModel, int timeToWait = 1000) 
        {
            GameObject toast = Instantiate(_toastOb, _toastHolder.transform);
            toast.GetComponent<ToastElement>().Show(toastModel,timeToWait);
        }
    }
}
