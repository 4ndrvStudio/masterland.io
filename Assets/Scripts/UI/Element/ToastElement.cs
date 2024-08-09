using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace masterland.UI
{
    public class ToastModel 
    {
        public bool IsSuccess;
        public string Title;
        public string Description;
    }

    public class ToastElement : MonoBehaviour
    {
        [SerializeField] private RectTransform _transform;
        [SerializeField] private Image _background;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Text _title;
        [SerializeField] private Text _desc;

        [SerializeField] private Color _successColor;
        [SerializeField] private Color _failColor;
         
        public async void Show(ToastModel toastModel, int timeToWait) 
        {
            _canvasGroup.alpha = 0;
            _background.color = toastModel.IsSuccess ? _successColor : _failColor;
            _title.text = toastModel.Title;
            _desc.text = toastModel.Description;
            gameObject.SetActive(true);
            DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1, 0.4f);
            await UniTask.Delay(timeToWait);
            DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 0,1.5f);
            _transform.DOMoveY(_transform.position.y + 250f, 1.5f).SetEase(Ease.OutQuad);
            Destroy(this.gameObject, 1.5f);
        }

    }
}
