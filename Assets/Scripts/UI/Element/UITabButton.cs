using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace masterland.UI
{
    public class UITabButton : MonoBehaviour
    {
        [SerializeField] private UITab _tab;
        [SerializeField] private Button _button;
        
        private List<UITab> _tabList;

        void Start()
        {
            _button.onClick.AddListener(OnClick);
        }

        public void Setup(List<UITab> tabList) 
        {
            _tabList = tabList;
        }

        public void OnClick() 
        {
            if(_tab.IsOpen) return;
            _tabList.ForEach(tab => tab.Hide());
            _tab.Show();
        }
    }
}
