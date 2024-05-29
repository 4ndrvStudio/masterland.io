using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace masterland
{
    using Manager;
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        
        [SerializeField] private GameObject _loginPanel;
        [SerializeField] private GameObject _waitingPanel;
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Button _playBtn;
        [SerializeField] private Button _startServerBtn;

        // Start is called before the first frame update
        void Awake()
        {
            if(Instance == null)
                Instance = this;

            _startServerBtn.gameObject.SetActive(!Application.isBatchMode);
            _playBtn.onClick.AddListener(Play);
            _startServerBtn.onClick.AddListener(() => GameManager.Instance.StartServer());
        }

        public void Play() 
        {
            GameManager.Instance.PlayerName = _nameInput.text;
            GameManager.Instance.Play();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    
        public void ToggleWaiting(bool isActive) => _waitingPanel.SetActive(isActive);
        public void ToggleLoginPanel(bool isActive) => _loginPanel.SetActive(isActive);


    }
}
