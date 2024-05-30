using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Transporting;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using FishNet.CodeGenerating;
using UnityEngine.InputSystem;


namespace masterland.Master
{
    public class MasterWeapon : MasterComponent
    {
        
        [AllowMutableSyncType]
        public SyncVar<byte> WeaponId = new SyncVar<byte>(0,new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));
        [ServerRpc(RunLocally = true)] private void SetWeapon(byte value) {
            if (value != WeaponId.Value) 
                WeaponId.Value = value;
        }

        [SerializeField] private GameObject _weaponHolder;
        [SerializeField] private GameObject _Sword1ObTest;

        private Collider _currentWeaponCollider;

        public override void OnStartClient()
        {
            base.OnStartClient();
            if(!IsOwner)
                return;
            _master.Input.MasterInput.Player.UseSlot1.performed += OnPerformUseSlot1;
            _master.Input.MasterInput.Player.UnuseWeapon.performed += OnPerformUnuseWeapon;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
             if(!IsOwner)
                return;

            _master.Input.MasterInput.Player.UseSlot1.performed -= OnPerformUseSlot1;
            _master.Input.MasterInput.Player.UnuseWeapon.performed -= OnPerformUnuseWeapon;

        }

        private void OnPerformUseSlot1(InputAction.CallbackContext context) {SetWeapon(101);}
        private void OnPerformUnuseWeapon(InputAction.CallbackContext context) {SetWeapon(0);}


        public override void Setup(Master master) 
        {
            base.Setup(master);
            WeaponId.OnChange += OnWeaponChanged;
        }

        private void OnWeaponChanged(byte prev, byte next, bool asServer) 
        {
            _master.Combat.ResetCombat();
            _master.Animation.SetWeaponType(GetTypeId(next));

            //test only
            _Sword1ObTest.SetActive(next ==101);
            _Sword1ObTest.GetComponent<Weapon.Weapon>().MasterOwner = _master;
            _currentWeaponCollider = _Sword1ObTest.GetComponent<Collider>();
            _currentWeaponCollider.enabled = false;
        }
        public int GetTypeId(byte id) => id / 100;
        public int GetTypeId()  => WeaponId.Value / 100;

        public override void OnUpdate()
        {
            base.OnUpdate();


            if(IsOwner) {
                GetInput();
            };

            if(IsServerInitialized) {
                EnableWeaponCollider();
            }
                
                
        }

   
        

        private void GetInput() 
        {
            // if(Input.GetKeyDown(KeyCode.X)) 
            //     SetWeapon(0);
            // if(Input.GetKeyDown(KeyCode.Alpha1)) 
            //     SetWeapon(101);
        }

        private void EnableWeaponCollider() 
        {
            _Sword1ObTest.GetComponent<Collider>().enabled = _master.AnimatorHook.WeaponColliderEnable;
        }
        public void EnableWeaponCollider(bool isEnable) 
        {
            _master.AnimatorHook.WeaponColliderEnable =isEnable;
        }
        

    }
}
