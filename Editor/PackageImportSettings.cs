using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

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

            //Add Zappar/CameraBackgroundShader to be always included
            AddAlwaysIncludedShader("Zappar/CameraBackgroundShader");

#if !UNITY_2021_3_OR_NEWER
            //C++11 or above required for compiling advanced mesh plugins with EMC
            if (!PlayerSettings.WebGL.emscriptenArgs.Contains("-std"))
            {
                PlayerSettings.WebGL.emscriptenArgs += " -std=c++11";
                Debug.Log("Updated WebGL emscripten arguments: " + PlayerSettings.WebGL.emscriptenArgs);
            }
#endif
        }

        public static void AddAlwaysIncludedShader(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
                return;

            GraphicsSettings gSettings = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            SerializedObject so = new SerializedObject(gSettings);
            SerializedProperty arrayProp = so.FindProperty("m_AlwaysIncludedShaders");
            bool hasShader = false;
            for (int i = 0; i < arrayProp.arraySize; ++i)
            {
                SerializedProperty arrayElem = arrayProp.GetArrayElementAtIndex(i);
                if (shader == arrayElem.objectReferenceValue)
                {
                    hasShader = true;
                    break;
                }
            }

            if (!hasShader)
            {
                int arrayIndex = arrayProp.arraySize;
                arrayProp.InsertArrayElementAtIndex(arrayIndex);
                SerializedProperty arrayElem = arrayProp.GetArrayElementAtIndex(arrayIndex);
                arrayElem.objectReferenceValue = shader;

                so.ApplyModifiedProperties();

                AssetDatabase.SaveAssets();
            }
        }
    }
}