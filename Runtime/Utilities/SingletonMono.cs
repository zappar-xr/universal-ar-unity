using UnityEngine;

namespace Zappar
{
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T s_Instance;
        public static T Instance
        {
            get
            {
                if(s_Instance==null)
                {
                    s_Instance = FindObjectOfType<T>();
                    if(s_Instance==null)
                    {
                        GameObject go = new GameObject("ZSingletonMono", typeof(T));
                        s_Instance = go.GetComponent<T>();
                    }
                }
                return s_Instance;
            }
        }

        protected void RegisterInstanceOnDestroy(T instance)
        {
            if(s_Instance==null)
            {
                s_Instance = instance;
            }else if(s_Instance != this)
            {
                Destroy(instance);
            }
        }

        public static bool Initialized => s_Instance != null;
    }
}