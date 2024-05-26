using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace masterland
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T singleton;

        public static T Instance
        {
            get
            {
                if (Singleton<T>.singleton == null)
                {
                    Singleton<T>.singleton = (T)FindObjectOfType(typeof(T));
                    if (Singleton<T>.singleton == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = "[@" + typeof(T).Name + "]";
                        Singleton<T>.singleton = obj.AddComponent<T>();
                    }
                }

                return Singleton<T>.singleton;
            }
        }

    }
}
