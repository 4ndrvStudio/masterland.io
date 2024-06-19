using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace masterland.Master
{
    public class MasterBot : MasterComponent
    {   
        public bool PlayBot;
        private bool _isCooldown;

        public override void OnStartClient() 
        {
            if(!IsOwner)
                return;
            _master.Input.MasterInput.Player.AIMode.performed += OnPerformAImode;
            _master.Input.MasterInput.Player.NormalMode.performed += OnPerformNormalmode;
        }

        public override void OnStopClient() 
        {
             if(!IsOwner)
                return;
            _master.Input.MasterInput.Player.AIMode.performed -= OnPerformAImode;
            _master.Input.MasterInput.Player.NormalMode.performed -= OnPerformNormalmode;
        }

        private void OnPerformAImode(InputAction.CallbackContext context) {PlayBot = true;}
        private void OnPerformNormalmode(InputAction.CallbackContext context) {PlayBot = false;}


        public override void OnTick()
        {
            base.OnTick();

            if(!IsOwner)
                return;

            if(PlayBot) 
            {
                PlayBotState();
            }
        }

        public void PlayBotState() {
            _master.Input.PlayLockOn =true;
            
            if(!_isCooldown) {
                StartCoroutine(PlayAttack());
            }
        }

        IEnumerator PlayAttack() 
        {
            _master.Input.PlayAttack = true;
            _isCooldown = true;
            yield return new WaitForSeconds (Random.Range(1,8));
             _isCooldown = false; 
        }
    }
}
