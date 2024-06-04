using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace masterland.UI
{
    using Manager;
    public class UIManager : Singleton<UIManager>
    {

        [SerializeField] private List<UIView> _listView = new List<UIView>();
        [SerializeField] private List<UIPopup> _listPopup = new List<UIPopup>();

        [SerializeField] private UINotifyToast _notifyToast;

        public UIView CurrentView;

        public void ShowPopup(PopupName popupName, Dictionary<string, object> customProperties = null)
        {
            UIPopup selectedPopup = _listPopup.Find(popup => popup.PopupName == popupName);
            if (selectedPopup != null) selectedPopup.Show(customProperties);

        }
        public void HidePopup(PopupName popupName)
        {
            UIPopup selectedPopup = _listPopup.Find(popup => popup.PopupName == popupName);
            if (selectedPopup != null) selectedPopup.Hide();
        }

        public void ToggleView(ViewName viewName, Dictionary<string, object> customProperties = null)
        {
            UIView selectedPanel = _listView.Find(tab => tab.ViewName == viewName);
            if (selectedPanel != null)
            {
                _listView.ForEach(view => view.Hide());
                selectedPanel.Show(customProperties);
                CurrentView = selectedPanel;
            }
        }

        public void CountinueStep(Dictionary<string, object> customProperties = null)
        {
            CurrentView.CountinueStep(customProperties);
        }

        public void ShowAlert(string msg, AlertType type)
        {
            _notifyToast.Show(msg, type);
        }

    }
}
