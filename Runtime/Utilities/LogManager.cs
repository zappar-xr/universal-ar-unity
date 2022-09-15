using UnityEngine;

namespace Zappar
{
    public class LogManager : SingletonMono<LogManager>
    {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
        private Z.DebugMode m_debugMode = Z.DebugMode.UnityLog;
        private Z.LogLevel m_logLevel = Z.LogLevel.WARNING;


        public static event Z.DebugLogDelegate OnDebugLog = DebugLog;
        public static event Z.DebugLogDelegate OnDebugErr = DebugErr;
        [AOT.MonoPInvokeCallback(typeof(Z.DebugLogDelegate))]
        private static void DebugLog(string msg) { Debug.Log(msg); }
        [AOT.MonoPInvokeCallback(typeof(Z.DebugLogDelegate))]
        private static void DebugErr(string msg) { Debug.LogError(msg); }


        private void Awake()
        {
            RegisterInstanceOnDestroy(this);
        }

        private void Start()
        {
            DontDestroyOnLoad(this);

            var settings = ZSettings.UARSettings;
            if (settings != null)
            {
                m_debugMode = settings.DebugMode;
                m_logLevel = settings.LogLevel;
            }
            else
            {
                Debug.LogError("ZapparUARSettings not found");
            }
            Z.SetDebugMode(m_debugMode, m_logLevel);

        }

        private void OnEnable()
        {
            Z.SetLogFunc(OnDebugLog);
            Z.SetErrorFunc(OnDebugErr);
        }

        private void OnDisable()
        {
            Z.SetLogFunc(null);
            Z.SetErrorFunc(null);
        }
#endif
    }
}