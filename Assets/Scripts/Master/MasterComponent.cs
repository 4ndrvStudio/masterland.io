using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace masterland.Master
{
    public class MasterComponent : NetworkBehaviour
    {
        protected Master _master;
    
        public virtual void Setup(Master master) 
        {
            _master = master;
        }

        public virtual void Awake() 
        {
            InstanceFinder.TimeManager.OnTick += OnTick;
            InstanceFinder.TimeManager.OnUpdate += OnUpdate;
            InstanceFinder.TimeManager.OnPreTick += OnPreTick;
            InstanceFinder.TimeManager.OnPostTick += OnPostTick;
        }
        public virtual void OnDestroy() 
        {
            if (InstanceFinder.TimeManager != null)
            {
                InstanceFinder.TimeManager.OnTick -= OnTick;
                InstanceFinder.TimeManager.OnUpdate -= OnUpdate;
                InstanceFinder.TimeManager.OnPostTick -= OnPostTick;
                InstanceFinder.TimeManager.OnPreTick -= OnPreTick;

            }
        }

        public virtual void OnTick() 
        {
            if(_master == null) return;
        }
        public virtual void OnUpdate() 
        {
            if(_master == null) return;
        }

        public virtual void OnPostTick() 
        {
            if(_master == null) return;
        }
        public virtual void OnPreTick() 
        {
            if(_master == null) return;
        }
      
    }
}
