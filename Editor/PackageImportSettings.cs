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
        }
    }
}