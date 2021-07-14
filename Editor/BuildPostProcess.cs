using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Zappar.Editor
{
    public class BuildPostProcessor
    {
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
        }
    }
}