using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using System.Linq;
using TMPro;
using FishNet.CodeGenerating;
using FishNet.Object.Synchronizing;

namespace masterland.Master
{
    using Manager;
    public class Master : NetworkBehaviour
    {
        public static Master Local;
        public Transform Body;
        public Animator Animator;
        public Input Input;
        public Stats Stats;
        public AnimatorHook AnimatorHook;
        public Movement Movement;
        public Animation Animation;
        public State State;
        public Combat Combat;
        public Damageable Damageable;
        public MasterWeapon Weapon;
        public AimAssist AimAssist;
        public Audio Audio;
        public MasterBot MasterBot;
        private List<MasterComponent> _masterComponentList = new();

        [SerializeField] private Material _ownerMat;
        [SerializeField] private Material _enemyMat;
        [SerializeField] private SkinnedMeshRenderer _bodyMesh;
        
        [SerializeField] private TextMeshPro _nameText;
    
        [AllowMutableSyncType]
        public SyncVar<string> MasterName = new SyncVar<string>("I'm Idiot",new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.Observers));


        public override void OnStartClient()
        {
            base.OnStartClient();
            _bodyMesh.material = _enemyMat;
            if(IsOwner) 
            {
                if(Local == null) 
                    Local = this;
                SetupOwner();
            }
            SetupMasterComponent();
            gameObject.name = ObjectId.ToString();

            MasterName.OnChange += OnNameChanged;
       
        }
        public override void OnStartServer()
        {
            base.OnStartServer();
            SetupMasterComponent();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
        }

        private void SetupOwner() 
        {
            //Setup Camera 
            _bodyMesh.material = _ownerMat;
            _nameText.gameObject.SetActive(false);
            if(GameManager.Instance != null) {
                MasterName.Value = string.IsNullOrEmpty(GameManager.Instance.PlayerName) ? "Player #"+gameObject.name : GameManager.Instance.PlayerName;
            }
            
            CameraManager.Instance.Setup(Body.transform);
        }

        private void SetupMasterComponent() 
        {
            AnimatorHook.Master = this;
            _masterComponentList = GetComponents<MasterComponent>().ToList();
            
            foreach(MasterComponent masterComponent in _masterComponentList) {
                masterComponent.Setup(this);
            }
        }

        public void OnNameChanged(string pre, string next, bool asServer) {
            _nameText.text = next;
        }


    }
}
