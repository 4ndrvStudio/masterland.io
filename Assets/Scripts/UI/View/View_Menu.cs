using UnityEngine;
using System.Collections.Generic;

namespace masterland.UI
{
    public class View_Menu : UIView
    {
        [SerializeField] private List<UITabButton> _tabBtnList = new();
        [SerializeField] private List<UITab> _tabList = new();

        void Start() 
        {
            _tabBtnList.ForEach(tabBtn => tabBtn.Setup(_tabList));
        }
        
        public override void Show(Dictionary<string, object> customProperties = null)
        {
            base.Show(customProperties);
            _tabBtnList[0].OnClick();
        }

        public override void Hide() 
        {
            
        }
    }
}
