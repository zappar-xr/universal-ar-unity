using UnityEngine;

namespace Zappar
{
    public static class ZPermissions
    {
        public static bool PermissionGrantedAll { get; private set; }

        public static void CheckAllPermissions()
        {
            if (PermissionGrantedAll) return;
            PermissionGrantedAll = Z.PermissionGrantedAll();
        }

        public static void RequestPermission()
        {
            if (ZSettings.UARSettings.PermissionRequestUI)
                Z.PermissionRequestUi();
            else
                Z.PermissionRequestAll();
        }
    }

    public static class ZSettings
    {
        private static ZapparUARSettings s_settings;

        public static ZapparUARSettings UARSettings
        {
            get
            {
                if (s_settings == null)
                    s_settings = LoadUARSettings();
                return s_settings;
            }
        }

        private static ZapparUARSettings LoadUARSettings()
        {
            string path = ZapparUARSettings.MySettingsPath;

            var settings = Resources.Load<ZapparUARSettings>(path.Substring(0, path.IndexOf(".asset")));

            if (settings == null)
            {
                Debug.LogError("No UAR settings found at: " + path);
            }
            else
            {
#if !UNITY_EDITOR
                Debug.Log("UAR Unity SDK version:" + settings.PackageVersion);
#endif
            }

            return settings;
        }
    }
}