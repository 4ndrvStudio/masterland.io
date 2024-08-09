using System.Collections;
using System.Collections.Generic;
using masterland.UI;
using UnityEngine;
using UnityEngine.InputSystem;
namespace masterland.Master
{
    public class Input : MasterComponent
    {
        public MasterInput MasterInput;
        public Vector2 MoveDirection;
        public bool PlayAttack;
        public bool PlayJump;
        public bool PlaySprint;
        public bool PlayFastDodgeLeft;
        public bool PlayFastDodgeRight;
        public bool PlayLockOn;
        public bool PlayDodge;
        public bool PlayAI;
        public bool PlayMint;

        private bool _avoidMultiClick = false;

        public override void Awake() {
            MasterInput = new MasterInput();
        }

        public override void OnStartClient()
        {
            MasterInput.Player.Enable();
            MasterInput.Player.Attack1.performed += OnPerformAttack;
            MasterInput.Player.Jump.performed += OnPerformJump;
            MasterInput.Player.FastDodgeLeft.performed += OnPerformFastDodgeLeft;
            MasterInput.Player.FastDodgeRight.performed += OnPerformFastDodgeRight;
            MasterInput.Player.Interact.performed += OnPerformInteract;
            MasterInput.Player.ResidentLicense.performed += OnPerformResidentLicense;

        }
        public override void OnStopClient()
        {
            MasterInput.Player.Disable();
            MasterInput.Player.Attack1.performed -= OnPerformAttack;
            MasterInput.Player.Jump.performed -= OnPerformJump;
            MasterInput.Player.FastDodgeLeft.performed -= OnPerformFastDodgeLeft;
            MasterInput.Player.FastDodgeRight.performed -= OnPerformFastDodgeRight;
            MasterInput.Player.Interact.performed -= OnPerformInteract;
            MasterInput.Player.ResidentLicense.performed -= OnPerformResidentLicense;
        }

        public void OnPerformAttack(InputAction.CallbackContext context) 
        { 
            if (!_avoidMultiClick) 
            {
                StartCoroutine(IEPlayAttack());
            }
        }
        public void OnPerformJump(InputAction.CallbackContext context) 
        { 
            PlayJump =true;
            if(PlayLockOn) 
                StartCoroutine(IEPlayDodge());
              
        }
        public void OnPerformFastDodgeLeft(InputAction.CallbackContext context) { PlayFastDodgeLeft = true ; }
        public void OnPerformFastDodgeRight(InputAction.CallbackContext context) { PlayFastDodgeRight = true ; }

        public void OnPerformInteract(InputAction.CallbackContext context) 
        { 
            if(_master.Selector.CurrentSelectedObject == null ) return;

            _master.Selector.CurrentSelectedObject.CurrentSelectedObject.Interact(_master.Owner);
        }

        public void OnPerformResidentLicense(InputAction.CallbackContext context) { UIManager.Instance.ShowPopup(PopupName.ResidentLicense);}

        private void Update()
        {
            if (!IsOwner) return;

            if(!_master.MasterBot.PlayBot) 
            {   
                MoveDirection =  MasterInput.Player.Move.ReadValue<Vector2>();
                PlayLockOn = MasterInput.Player.LockOn.IsPressed();
            }   
            PlaySprint = MasterInput.Player.Sprint.IsPressed();


        }

        IEnumerator IEPlayAttack() {
            PlayAttack =true;
            yield return new WaitForSeconds(0.2f);
            PlayAttack = false;
        }

        IEnumerator IEPlayDodge() {
            PlayDodge =true;
            yield return new WaitForSeconds(0.2f);
            PlayDodge =false;
        }
    }
}
