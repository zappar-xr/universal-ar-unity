using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Zappar;

namespace Zappar.Editor
{
    public class BuildPostProcessor
    {
        public const string WebGLPermissionsTag = "ZAPPAR_PERMISSIONS_IMPLEMENTATION";

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string targetPath)
        {
            if (target != BuildTarget.WebGL)
                return;

#if !UNITY_2020_1_OR_NEWER
            string path = Path.Combine(targetPath, "Build/UnityLoader.js");
            string text = File.ReadAllText(path);
            text = text.Replace("UnityLoader.SystemInfo.mobile", "false");
            File.WriteAllText(path, text);
#endif

            var settings = AssetDatabase.LoadAssetAtPath<ZapparUARSettings>(ZapparUARSettings.MySettingsPathInPackage);

            if (settings == null)
            {
                Debug.LogError("UAR settings not found! Invalid html template!!");
                return;
            }

            if(settings.ExcludeZPTFromBuild)
            {
                string zpt_dir = Path.Combine(targetPath, "StreamingAssets");
                DirectoryInfo di = new DirectoryInfo(zpt_dir);
                foreach(var file in di.EnumerateFiles())
                {
                    if (file.FullName.EndsWith(".zpt"))
                    {
                        Debug.Log("Removed " + file.FullName);
                        file.Delete();
                    }
                }
            }

            string indexFile = Path.Combine(targetPath, "index.html");
            string indexContent = File.ReadAllText(indexFile);
            if (settings.PermissionRequestUI)
                indexContent = indexContent.Replace(WebGLPermissionsTag, "window.zappar.permission_request_ui_promise().then(WaitForZCVLoad);");
            else
                indexContent = indexContent.Replace(WebGLPermissionsTag, "WaitForZCVLoad();");
            File.WriteAllText(indexFile, indexContent);
        }
    }
}