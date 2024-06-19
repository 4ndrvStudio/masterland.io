using UnityEngine;
using UnityEngine.UI;

namespace masterland.UI
{
    using Wallet;
    using Manager;
    public class View_Login : UIView
    {
        [SerializeField] private Button _startServerBtn;
        [SerializeField] private Button _playBtn;

        private void Start() 
        {
            //_startServerBtn.gameObject.SetActive(true);
            //_playBtn.onClick.AddListener(WalletInteractor.Instance.Send_GetAddress);
            //_startServerBtn.onClick.AddListener(GameManager.Instance.StartServer);
        }

        
    }
}
