using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace masterland.Master
{
    public class MasterBot : MasterComponent
    {
        
        public bool PlayBot;
        private bool _isCooldown;

        public override void OnTick()
        {
            base.OnTick();

            if(!IsOwner)
                return;

            if(Input.GetKeyDown(KeyCode.B)) {
                PlayBot = true;
            }
            if(Input.GetKeyDown(KeyCode.N)) {
                PlayBot = false;
            }

            if(PlayBot) 
            {
                PlayBotState();
            }
        }

        public void PlayBotState() {
            _master.Input.PlayLockOn =true;
            if(_master.AimAssist.SelectedNearest == null)
                return;

            float distaneToAnother = Vector3.Distance(_master.AimAssist.SelectedNearest.transform.position, this.transform.position);
            _master.Input.MoveDirection = new Vector2(0,distaneToAnother>= 2.5f ? 1 : 0);
            
            if(distaneToAnother < 2.5f && !_isCooldown) {
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
