using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace masterland.UI
{
    using masterland.Manager;
    using Wallet;

    public class View_Menu : UIView
    {
        [SerializeField] private Button _walletBtn;
        [SerializeField] private List<UITabButton> _tabBtnList = new();
        [SerializeField] private List<UITab> _tabList = new();
        [SerializeField] private Button _serverBtn;

        void Start() 
        {
            _serverBtn.gameObject.SetActive(GameManager.Instance.IsDev);
            _walletBtn.onClick.AddListener(() => 
                        UIManager.Instance.ShowPopup(PopupName.SuiWallet, new Dictionary<string, object>{{"isConfirmTx", false}}));
            _tabBtnList.ForEach(tabBtn => tabBtn.Setup(_tabList));

            _serverBtn.onClick.AddListener(() => {
                if(GameManager.Instance.IsDev)
                    Network.Instance.StartServer();
            });
        }
        
        public override void Show(Dictionary<string, object> customProperties = null)
        {
            base.Show(customProperties);           
            _tabBtnList[0].OnClick();
            _walletBtn.gameObject.SetActive(!WalletInteractor.IsWebgl);
        }

        public override void Hide() 
        {
            base.Hide();
        }
    }
}
