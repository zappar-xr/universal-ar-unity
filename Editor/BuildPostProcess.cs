using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Zappar;
#if UNITY_EDITOR && UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace Zappar.Editor
{
    public class BuildPostProcessor
    {
        public const string WebGLPermissionsTag = "ZAPPAR_PERMISSIONS_IMPLEMENTATION";
        public const string WebGLCacheTag = "ZAPPAR_WEBGL_CACHING"; //Only supported on Unity 2020 and above

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string targetPath)
        {
            if (target == BuildTarget.iOS)
                IOSPostProcessBuild(targetPath);

            if (target == BuildTarget.WebGL)
                WebGLPostProcessBuild(targetPath);

        }

        private static void IOSPostProcessBuild(string targetPath)
        {
#if UNITY_EDITOR && UNITY_IOS
            string projectPath = PBXProject.GetPBXProjectPath(targetPath);
            PBXProject project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));
            string targetGUID = project.GetUnityFrameworkTargetGuid();

            project.AddFrameworkToProject(targetGUID, "OpenGLES.framework", false);
            project.AddFrameworkToProject(targetGUID, "Accelerate.framework", false);


            //copy ZCV.Bundle to iOS project root
            string srcDir = "Packages/com.zappar.uar/Plugins/iOS/ZCV.bundle";
            string destDir = targetPath + "/ZCV.bundle";
            Utils.ZUtils.DirectoryCopy(srcDir,destDir,true);

            File.WriteAllText(projectPath, project.WriteToString());
#endif
        }

        private static void WebGLPostProcessBuild(string targetPath)
        {
#if !UNITY_2020_1_OR_NEWER
            if (!PlayerSettings.WebGL.nameFilesAsHashes)
            {
                string path = Path.Combine(targetPath, "Build/UnityLoader.js");
                string text = File.ReadAllText(path);
                text = text.Replace("UnityLoader.SystemInfo.mobile", "false");
                File.WriteAllText(path, text);
            }else{
                //Find loader.js file
                string[] files = Directory.GetFiles(Path.Combine(targetPath, "Build"), "*.js");
                if (files.Length == 1)
                {
                    string text = File.ReadAllText(files[0]);
                    text = text.Replace("UnityLoader.SystemInfo.mobile", "false");
                    File.WriteAllText(files[0], text);
                }
                else
                {
                    Debug.LogWarning("Can't determine UnityLoader.js file under build directory! Replace the following string: <b>UnityLoader.SystemInfo.mobile</b> with <b>false</b> in UnityLoader.js (or FILEHASH.js) to suppress Unity mobile warning.");
                }
            }
#endif
            string indexFile = "";
            string indexContent = "";
#if UNITY_2020_1_OR_NEWER
            indexFile = Path.Combine(targetPath, "index.html");
            indexContent = File.ReadAllText(indexFile);
            string cacheJS = "if (url.match(/\\.data/) || url.match(/\\.bundle/) || url.match(/\\.zpt/)) {\n             " +
                                  (PlayerSettings.WebGL.nameFilesAsHashes ? "return \"immutable\";\n" : "return \"must-revalidate\";\n            ") +
                              "}\n";
            indexContent = indexContent.Replace(WebGLCacheTag, cacheJS);
#endif

            var settings = AssetDatabase.LoadAssetAtPath<ZapparUARSettings>(ZapparUARSettings.MySettingsPathInPackage);

            if (settings == null)
            {
                Debug.LogError("UAR settings not found! Invalid html template!!");
                return;
            }

            if (settings.ExcludeZPTFromBuild)
            {
                string zpt_dir = Path.Combine(targetPath, "StreamingAssets");
                DirectoryInfo di = new DirectoryInfo(zpt_dir);
                foreach (var file in di.EnumerateFiles())
                {
                    if (file.FullName.EndsWith(".zpt"))
                    {
                        Debug.Log("Removed " + file.FullName);
                        file.Delete();
                    }
                }
            }
            if (string.IsNullOrEmpty(indexFile))
                indexFile = Path.Combine(targetPath, "index.html");
            if (string.IsNullOrEmpty(indexContent))
                indexContent = File.ReadAllText(indexFile);
            if (settings.PermissionRequestUI)
                indexContent = indexContent.Replace(WebGLPermissionsTag, "window.zappar.permission_request_ui_promise().then(WaitForZCVLoad);");
            else
                indexContent = indexContent.Replace(WebGLPermissionsTag, "WaitForZCVLoad();");
            File.WriteAllText(indexFile, indexContent);
        }


    }
}