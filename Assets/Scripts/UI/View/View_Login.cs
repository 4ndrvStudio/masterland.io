using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace masterland.UI
{
    using Wallet;
    using Manager;
    using TMPro;
    using System.Collections.Generic;

    public enum WalletPanelType
    {
        Wallet,
        Create,
        Import,
    }

    public class View_Login : UIView
    {
        [SerializeField] private GameObject _walletPanel;
        [SerializeField] private GameObject _createPanel;
        [SerializeField] private GameObject _importPanel;

        [Header("Wallet")]
        [SerializeField] private Button _createWalletBtn;
        [SerializeField] private Button _importWalletBtn;

        [Header("Create")]
        [SerializeField] private Button _okeBtn;
        [SerializeField] private TMP_InputField _new12passphrase;

        [Header("Import")]
        [SerializeField] private Button _importBtn;
        [SerializeField] private Button _backBtn;
        [SerializeField] private TMP_InputField _import12passphrase;
        [SerializeField] private TextMeshProUGUI _notifyText;


        private void Start() 
        {
            _createWalletBtn.onClick.AddListener(Create);
            _importWalletBtn.onClick.AddListener(()=> TogglePanel(WalletPanelType.Import));
            _okeBtn.onClick.AddListener(() => WalletInGame.Login());

            _importBtn.onClick.AddListener(Import);
            _backBtn.onClick.AddListener(()=> { 
                _import12passphrase.text = "";
                _notifyText.text = "";
                TogglePanel(WalletPanelType.Wallet);
            });

            _import12passphrase.onValueChanged.AddListener((value) => {
                _notifyText.text = "";
            });
        }

        public override void Show(Dictionary<string, object> customProperties = null)
        {
            base.Show(customProperties);
            TogglePanel(WalletPanelType.Wallet);
        }

        public void TogglePanel(WalletPanelType walletPanelType) 
        {
            _walletPanel.SetActive(walletPanelType == WalletPanelType.Wallet);
            _createPanel.SetActive(walletPanelType == WalletPanelType.Create);
            _importPanel.SetActive(walletPanelType == WalletPanelType.Import);
            _import12passphrase.text = "";
        }

        public void Create() 
        {
            string mnemo = SuiWallet.CreateNewWallet();
            _new12passphrase.text = mnemo;
            TogglePanel(WalletPanelType.Create);
        }

        public void Import() 
        {
            if (_import12passphrase.text.Trim().Split(" ").Length != 12)
            {
                _notifyText.text = "Please check your 12 passphrases";
                return;
            }
            WalletInGame.Login(_import12passphrase.text.Trim());
        }

        
    }
}
