using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace masterland.Master
{
    public class MasterInput : MasterComponent
    {
        public Vector2 MoveDirection;
        public bool PlayAttack;
        public bool PlayJump;
        public bool PlaySprint;
        public bool PlayFastDodgeLeft;
        public bool PlayFastDodgeRight;
        public bool PlayLockOn;
        public bool PlayDodge;

        private bool _avoidMultiClick = false;
        private void Update()
        {
            if (!IsOwner) return;

            if(!_master.MasterBot.PlayBot) 
            {   
                MoveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                PlayLockOn = Input.GetMouseButton(1);
                if (Input.GetMouseButtonDown(0) && !_avoidMultiClick) {
                    PlayAttack = true;
                    StartCoroutine(IEAvoidMultiClick());
                }
                    
            }   

            PlayJump = Input.GetKeyDown(KeyCode.Space);
            PlaySprint = Input.GetKey(KeyCode.LeftShift);
            PlayFastDodgeLeft = Input.GetKeyDown(KeyCode.Q);
            PlayFastDodgeRight = Input.GetKeyDown(KeyCode.E);
            PlayDodge = PlayLockOn && PlayJump;
        }

        IEnumerator IEAvoidMultiClick() {
            _avoidMultiClick = true;
            yield return new WaitForSeconds(0.8f);
            _avoidMultiClick = false;
        }
    }
}
