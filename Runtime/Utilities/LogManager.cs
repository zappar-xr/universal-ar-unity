using UnityEngine;

namespace Zappar
{
    public class LogManager : SingletonMono<LogManager>
    {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
        private Z.DebugMode DebugMode = Z.DebugMode.UnityLog;
        private Z.LogLevel LogLevel = Z.LogLevel.WARNING;


        public static event Z.DebugLogDelegate onDebugLog = OnDebugLog;
        public static event Z.DebugLogDelegate onDebugErr = OnDebugErr;
        [AOT.MonoPInvokeCallback(typeof(Z.DebugLogDelegate))]
        private static void OnDebugLog(string msg) { Debug.Log(msg); }
        [AOT.MonoPInvokeCallback(typeof(Z.DebugLogDelegate))]
        private static void OnDebugErr(string msg) { Debug.LogError(msg); }


        private void Awake()
        {
            RegisterInstanceOnDestroy(this);
        }

        private void Start()
        {
            DontDestroyOnLoad(this);

            var settings = Resources.FindObjectsOfTypeAll< ZapparUARSettings>();
            if (settings != null && settings.Length>0)
            {
                DebugMode = settings[0].DebugMode;
                LogLevel = settings[0].LogLevel;
            }
            else
            {
                Debug.LogError("ZapparUARSettings not found");
            }
            Z.SetDebugMode(DebugMode, LogLevel);

        }

        void OnEnable()
        {
            Z.SetLogFunc(onDebugLog);
            Z.SetErrorFunc(onDebugErr);
        }

        void OnDisable()
        {
            Z.SetLogFunc(null);
            Z.SetErrorFunc(null);
        }
#endif
    }
}