using UnityEngine;
using UnityEngine.UI;

namespace masterland.UI
{
    using Manager;
    public class View_Login : UIView
    {
        [SerializeField] private Button _startServerBtn;
        [SerializeField] private Button _playBtn;

        private void Start() 
        {
            _startServerBtn.gameObject.SetActive(Application.isEditor);
            _playBtn.onClick.AddListener(GameManager.Instance.Play);
            _startServerBtn.onClick.AddListener(GameManager.Instance.StartServer);
        }
    }
}
