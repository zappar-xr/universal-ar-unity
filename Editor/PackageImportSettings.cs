using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Zappar.Editor
{
    [InitializeOnLoad]
    public class PackageImportSettings
    {
        static PackageImportSettings()
        {
            RefreshPackage();
        }

        public static void RefreshPackage()
        {
            if (Application.isPlaying) return;

            //Copy WebGL templates from package to project
            string srcDir = "Packages/com.zappar.uar/WebGLTemplates";
            string destDir = Application.dataPath + "/WebGLTemplates";
            Utils.ZUtils.DirectoryCopy(srcDir, destDir, true);

            //Copy test zpt
            srcDir = "Packages/com.zappar.uar/ZapparResources~";
            destDir = Application.streamingAssetsPath;
            Utils.ZUtils.DirectoryCopy(srcDir, destDir, false);

            //Cache UARSettings in local asset database 
            ZapparUARSettingsProvider.GetOrCreateSettings();

#if !UNITY_2021_3_OR_NEWER
            //C++11 or above required for compiling advanced mesh plugins with EMC
            if (!PlayerSettings.WebGL.emscriptenArgs.Contains("-std"))
            {
                PlayerSettings.WebGL.emscriptenArgs += " -std=c++11";
                Debug.Log("Updated WebGL emscripten arguments: " + PlayerSettings.WebGL.emscriptenArgs);
            }
#endif
        }
    }
}