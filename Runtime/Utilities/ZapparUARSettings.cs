using UnityEngine;

namespace Zappar
{
    public class ZapparUARSettings : ScriptableObject
    {
        public const string MySettingsPath = "User/ZapparUARSettings.asset";
        public const string MySettingsPathInPackage = "Packages/com.zappar.uar/Resources/" + MySettingsPath;

        [SerializeField]
        private string m_packageVersion = "na";
        public string PackageVersion
        {
            get { return m_packageVersion; }
            set { m_packageVersion = value; }
        }

        [SerializeField]
        private bool m_enableImageTargetPreview = false;
        public bool ImageTargetPreviewEnabled
        {
            get { return m_enableImageTargetPreview; }
            set { m_enableImageTargetPreview = value; }
        }

        [SerializeField]
        private bool m_excludeZPTFromBuild = false;
        public bool ExcludeZPTFromBuild
        {
            get { return m_excludeZPTFromBuild; }
            set { m_excludeZPTFromBuild = value; }
        }

        [SerializeField]
        private int m_concurrentFaceTrackerCount = 2;
        public int ConcurrentFaceTrackerCount
        {
            get { return m_concurrentFaceTrackerCount; }
            set { m_concurrentFaceTrackerCount = (value > 0 ? value : 2); }
        }

        [SerializeField]
        private bool m_permissionRequestUI = true;
        public bool PermissionRequestUI
        {
            get { return m_permissionRequestUI; }
            set { m_permissionRequestUI = value; }
        }

        [SerializeField]
        private bool m_enableRealtimeReflections = false;
        public bool EnableRealtimeReflections
        {
            get { return m_enableRealtimeReflections; }
            set { m_enableRealtimeReflections = value; }
        }

        [SerializeField]
        private Z.DebugMode m_debugMode = Z.DebugMode.UnityLog;
        public Z.DebugMode DebugMode
        {
            get { return m_debugMode; }
            set { m_debugMode = value; }
        }

        [SerializeField]
        private Z.LogLevel m_logLevel = Z.LogLevel.WARNING;
        public Z.LogLevel LogLevel
        {
            get { return m_logLevel; }
            set { m_logLevel = value; }
        }

    }
}