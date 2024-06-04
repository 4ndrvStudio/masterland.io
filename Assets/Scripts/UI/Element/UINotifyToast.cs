using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace masterland.UI
{
    public class UINotifyToast : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectBackground = default;
        [SerializeField] private TextMeshProUGUI _tmpAlert = default;
        private bool _isPressed = false;

        public void Show(string msg, AlertType type)
        {
            if (!_isPressed)
            {
                _rectBackground.gameObject.SetActive(true);
                _isPressed = true;
                _tmpAlert.color = type == AlertType.Error ? Color.red : type == AlertType.Warning ? Color.yellow : Color.green;
                _tmpAlert.SetText(msg);
                _rectBackground.anchoredPosition = Vector2.zero;
                _rectBackground.DOAnchorPosY(350, 2.5f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    _tmpAlert.SetText("");
                    _isPressed = false;
                    _rectBackground.gameObject.SetActive(false);
                });
            }
        }
    }

    public enum AlertType
    {
        None,
        Normal,
        Warning,
        Error,
    }
}
