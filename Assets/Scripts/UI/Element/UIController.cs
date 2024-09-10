using UnityEngine;
using UnityEngine.UI;

namespace masterland
{
    using UI;
    
    public class UIController : Singleton<UIController>
    {
        public bool Standby;
        //Interact
        [SerializeField] private UIInteractTutorial _interactTutorial;

        public void ShowInteractTutorial(string function) 
        {
            _interactTutorial.Show("f", function);
        }

        public void HideInteractTutorial() => _interactTutorial.Hide();

    }
}
