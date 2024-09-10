using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace masterland.Master
{
    using UI;
    using Manager;

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
        public bool PlayBuild;
        public bool HouseRotationLeft;
        public bool HouseRotationRight;
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
            MasterInput.Player.Building.performed += OnPerformBuilding;

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
            MasterInput.Player.Building.performed -= OnPerformBuilding;
        }

        public void OnPerformAttack(InputAction.CallbackContext context) 
        { 
            if(GameManager.Instance && UIController.Instance &&  UIController.Instance.Standby) 
                return;

            if (!_avoidMultiClick) 
            {
                if(!PlayBuild)
                    StartCoroutine(IEPlayAttack());
                else
                    _master.Selector.RequireBuild();
            }   
            
        }
        public void OnPerformJump(InputAction.CallbackContext context) 
        { 
            if(GameManager.Instance && UIController.Instance &&  UIController.Instance.Standby) 
                return;

            PlayJump =true;
            if(PlayLockOn) 
                StartCoroutine(IEPlayDodge());
              
        }
        public void OnPerformFastDodgeLeft(InputAction.CallbackContext context) 
        { 
            if(GameManager.Instance && UIController.Instance &&  UIController.Instance.Standby) 
                return;
            if(PlayBuild)
                return;

            PlayFastDodgeLeft = true ; 
        }
        public void OnPerformFastDodgeRight(InputAction.CallbackContext context) 
        { 
            if(GameManager.Instance && UIController.Instance &&  UIController.Instance.Standby) 
                return;

            if(PlayBuild)
                return;

            PlayFastDodgeRight = true ; 
        }

        public void OnPerformInteract(InputAction.CallbackContext context) 
        { 
            if(GameManager.Instance && UIController.Instance &&  UIController.Instance.Standby) 
                return;

            if(_master.Selector.CurrentSelectedObject == null ) return;

            _master.Selector.CurrentSelectedObject.CurrentSelectedObject.Interact(_master.Owner);
        }

        public void OnPerformResidentLicense(InputAction.CallbackContext context) 
        { 
            if(GameManager.Instance && UIController.Instance &&  UIController.Instance.Standby) 
                return;
            
            UIManager.Instance.ShowPopup(PopupName.ResidentLicense);
        }

        public void OnPerformBuilding(InputAction.CallbackContext context) 
        { 
            if(GameManager.Instance && UIController.Instance &&  UIController.Instance.Standby) 
                return;
            
            if(SceneController.Instance.LoadSceneAsync(SceneName.Scene_Building)) 
                UIManager.Instance.ToggleView(ViewName.Unuse);
        }

        private void Update()
        {
            if(GameManager.Instance && UIController.Instance &&  UIController.Instance.Standby) 
                return;

            if (!IsOwner) return;

            if(!_master.MasterBot.PlayBot) 
            {   
                MoveDirection =  MasterInput.Player.Move.ReadValue<Vector2>();
                PlayLockOn = MasterInput.Player.LockOn.IsPressed();
            }   
            PlaySprint = MasterInput.Player.Sprint.IsPressed();
            HouseRotationLeft = PlayBuild && MasterInput.Player.FastDodgeLeft.IsPressed();
            HouseRotationRight = PlayBuild && MasterInput.Player.FastDodgeRight.IsPressed();
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
