using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
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
                    Singleton<T>.singleton = (T)Object.FindFirstObjectByType(typeof(T));
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

    public class SingletonNetwork<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        private static T singleton;

        public static T Instance
        {
            get
            {
                if (SingletonNetwork<T>.singleton == null)
                {
                    SingletonNetwork<T>.singleton = (T)Object.FindFirstObjectByType(typeof(T));
                    if (SingletonNetwork<T>.singleton == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = "[@" + typeof(T).Name + "]";
                        SingletonNetwork<T>.singleton = obj.AddComponent<T>();
                    }
                }

                return SingletonNetwork<T>.singleton;
            }
        }

    }
}
