using System.Collections;
using System.Collections.Generic;
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
        }

        public override void OnStopClient()
        {
            MasterInput.Player.Disable();
            MasterInput.Player.Attack1.performed -= OnPerformAttack;
            MasterInput.Player.Jump.performed -= OnPerformJump;
            MasterInput.Player.FastDodgeLeft.performed -= OnPerformFastDodgeLeft;
            MasterInput.Player.FastDodgeRight.performed -= OnPerformFastDodgeRight;
        }

        public void OnPerformAttack(InputAction.CallbackContext context) 
        { 
            if (!_avoidMultiClick) 
            {
                PlayAttack = true;
                StartCoroutine(IEAvoidMultiClick());
            }
        }
        public void OnPerformJump(InputAction.CallbackContext context) { PlayJump = true; }
        public void OnPerformFastDodgeLeft(InputAction.CallbackContext context) { PlayFastDodgeLeft = true ; }
        public void OnPerformFastDodgeRight(InputAction.CallbackContext context) { PlayFastDodgeRight = true ; }

        private void Update()
        {
            if (!IsOwner) return;

            if(!_master.MasterBot.PlayBot) 
            {   
                MoveDirection =  MasterInput.Player.Move.ReadValue<Vector2>();
                PlayLockOn = MasterInput.Player.LockOn.IsPressed();
            }   
            PlaySprint = MasterInput.Player.Sprint.IsPressed();
            PlayDodge = PlayLockOn && PlayJump;
        }

        IEnumerator IEAvoidMultiClick() {
            _avoidMultiClick = true;
            yield return new WaitForSeconds(0.8f);
            _avoidMultiClick = false;
        }
    }
}
