#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN

using UnityEngine;

namespace Zappar
{
    public class ZapparUARSettings : ScriptableObject
    {
        public const string MySettingsPath = "Packages/com.zappar.uar/Resources/User/ZapparUARSettings.asset";

        [SerializeField]
        private bool m_EnableImageTargetPreview = false;
        public bool ImageTargetPreviewEnabled
        {
            get { return m_EnableImageTargetPreview; }
            set { m_EnableImageTargetPreview = value; }
        }

        [SerializeField]
        private Z.DebugMode m_DebugMode = Z.DebugMode.UnityLog;
        public Z.DebugMode DebugMode
        {
            get { return m_DebugMode; }
            set { m_DebugMode = value; }
        }

        [SerializeField]
        private Z.LogLevel m_LogLevel = Z.LogLevel.WARNING;
        public Z.LogLevel LogLevel
        {
            get { return m_LogLevel; }
            set { m_LogLevel = value; }
        }

    }
}

#endif