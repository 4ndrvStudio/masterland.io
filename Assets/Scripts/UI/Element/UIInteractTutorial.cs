using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace masterland.UI
{
    
    public class UIInteractTutorial : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _functionText;
        [SerializeField] private TextMeshProUGUI _buttonText;
        
        public void Show(string button, string function) 
        {
            if(function != _functionText.text) {
                _functionText.text = function;
                _buttonText.text = button;
                gameObject.SetActive(true);
            }
        }

        public void Hide() 
        {
            if(!gameObject.activeSelf)
                return;

            gameObject.SetActive(false);
            _functionText.text = string.Empty;
        }
    }
}
