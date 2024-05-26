using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace masterland
{
    public class Global : Singleton<Global>
    {
       public int targetFrameRate = 60;
    
       void Start() => DontDestroyOnLoad(gameObject);

       void Update() 
       {
            // if(Application.targetFrameRate != targetFrameRate) {
            //     QualitySettings.vSyncCount = 0;
            //     Application.targetFrameRate = targetFrameRate;
            // }
       }
    }
}
