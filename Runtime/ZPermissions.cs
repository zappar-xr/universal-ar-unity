using UnityEngine;

namespace Zappar
{
    public static class ZPermissions
    {
        public static bool PermissionGrantedAll { get; private set; }

        private static ZapparUARSettings s_settings;

        private static void GetUARSettings()
        {
            if(s_settings==null)
            {
                string path = ZapparUARSettings.MySettingsPath;
                s_settings = Resources.Load<ZapparUARSettings>(path.Substring(0, path.IndexOf(".asset")));
                //Debug.Log("Permission UI:" + s_settings.PermissionRequestUI);
            }
        }

        public static void CheckAllPermissions()
        {
            if (PermissionGrantedAll) return;
            PermissionGrantedAll = Z.PermissionGrantedAll();
        }

        public static void RequestPermission()
        {
            GetUARSettings();

            if (s_settings.PermissionRequestUI)
                Z.PermissionRequestUi();
            else
                Z.PermissionRequestAll();
        }
    }
}