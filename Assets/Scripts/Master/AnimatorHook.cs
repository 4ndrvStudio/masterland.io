using System.Collections;
using System.Collections.Generic;
using FishNet.Managing.Timing;
using UnityEngine;
using FishNet.Object;
using FishNet;
using FishNet.Component.Animating;

namespace masterland.Master
{
    public class AnimatorHook : NetworkBehaviour
    {
        public Master Master;
        public Animator animator;
        public Vector3 DeltaPosition;
        public bool WeaponColliderEnable;
        private Vector3 currentVelocity = Vector3.zero;

        [SerializeField] private List<GameObject> slashFx = new();
        [SerializeField] private List<GameObject> hitFx =new();

        private void OnAnimatorMove() 
        {

            DeltaPosition =  animator.deltaPosition;
        }

        public void OnEnableWeapon(int index) 
        {
            WeaponColliderEnable = index == 1;
            if(index == 1) 
            {
                StartCoroutine(DisableWeapon());
            }
        }

        private void Update() 
        {
           if(Master.State.IsAction == false)
               WeaponColliderEnable = false;
        }
       
        
        public void OnEnableFX(int index) 
        {
            GameObject fx = Instantiate(slashFx[index-1]);
            fx.transform.position = slashFx[index-1].transform.position;
            fx.transform.rotation = slashFx[index-1].transform.rotation;
            fx.SetActive(true);
            Destroy(fx,1f);
            
            if(!IsServerInitialized)
                Master.Audio.PlayOneShot(AudioType.SwordSlash);
        }
        public void OnHit(int index) 
        {
            GameObject fx = Instantiate(hitFx[index-1]);
            fx.transform.position = hitFx[index-1].transform.position;
            fx.transform.rotation = hitFx[index-1].transform.rotation;
            fx.SetActive(true);
            Destroy(fx,1f);
        }
        IEnumerator DisableWeapon() {
            yield return new WaitForSeconds(0.7f);
        }
      
    }
}
