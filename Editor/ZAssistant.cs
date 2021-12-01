using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Zappar
{
    [Flags]
    public enum ZProjectSettings
    {
        None = 0,
        General = 1,    //Scripting back-end; stripping engine code; development build; Realtime reflection
        WebGLCompressionAndTemplate = 2,  //Compression format/fallback; Template
        GraphicsOptimum = 4,  //Platform specific graphics API;
        IosPermissions = 8, // Camera usage description;
        AndroidArchitexture = 16    //Minimum android SDK version
        //RealtimeReflection = 32
    }

    public enum ZProjectSettingsConfig : byte
    {
        WebGLAll = ZProjectSettings.General | ZProjectSettings.WebGLCompressionAndTemplate | ZProjectSettings.GraphicsOptimum,  //7
        WebGLMin = ZProjectSettings.WebGLCompressionAndTemplate,
        AndroidAll = ZProjectSettings.General | ZProjectSettings.GraphicsOptimum | ZProjectSettings.AndroidArchitexture,
        AndroidMin = ZProjectSettings.AndroidArchitexture,
        IosAll = ZProjectSettings.General | ZProjectSettings.GraphicsOptimum | ZProjectSettings.IosPermissions,
        IosMin = ZProjectSettings.IosPermissions
    }

    public class ZAssistant
    {
        public static bool MatchConfigSettings(ZProjectSettingsConfig config, ZProjectSettings setting)
        {
            return ((int)config & (int)setting) != 0;
        }


        public static void UpdateUnityProjectSettings(ZProjectSettingsConfig config)
        {
            var uarSettings = AssetDatabase.LoadAssetAtPath<ZapparUARSettings>(ZapparUARSettings.MySettingsPathInPackage);
#if UNITY_WEBGL
            if (ZAssistant.MatchConfigSettings(config, ZProjectSettings.General))
            {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP); //default is IL2CPP
                PlayerSettings.stripEngineCode = true;
                EditorUserBuildSettings.development = false;
                PlayerSettings.runInBackground = true;

                if (uarSettings != null && uarSettings.EnableRealtimeReflections && !QualitySettings.realtimeReflectionProbes)
                {
                    QualitySettings.realtimeReflectionProbes = true;
                    Debug.Log("[NOTE] Enabled realtime reflections. However your build target platform setting may vary! Make sure it's enabled there!!");
                }
            }

            if (ZAssistant.MatchConfigSettings(config, ZProjectSettings.WebGLCompressionAndTemplate))
            {
                PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                PlayerSettings.WebGL.dataCaching = true;

                //Build Settings
#if UNITY_2020_1_OR_NEWER
                PlayerSettings.WebGL.decompressionFallback = true;
                PlayerSettings.WebGL.template = "PROJECT:Zappar";
#elif UNITY_2019_1_OR_NEWER
                PlayerSettings.WebGL.template = "PROJECT:Zappar2019";
#else
                Debug.LogError("Please upgrade to newer versions of Unity");
#endif
            }

            if (ZAssistant.MatchConfigSettings(config, ZProjectSettings.GraphicsOptimum))
            {
                PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { GraphicsDeviceType.OpenGLES2 });
            }

#elif UNITY_ANDROID

            if(ZAssistant.MatchConfigSettings(config,ZProjectSettings.General))
            {
                EditorUserBuildSettings.development = false;

                if (uarSettings != null && uarSettings.EnableRealtimeReflections && !QualitySettings.realtimeReflectionProbes)
                {
                    QualitySettings.realtimeReflectionProbes = true;
                    Debug.Log("[NOTE] Enabled realtime reflections. However your build target platform setting may vary! Make sure it's enabled there!!");
                }
            }

            if (ZAssistant.MatchConfigSettings(config, ZProjectSettings.GraphicsOptimum))
            {
                PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
            }

            if (ZAssistant.MatchConfigSettings(config, ZProjectSettings.AndroidArchitexture))
            {
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            }

#elif UNITY_IOS

            if(ZAssistant.MatchConfigSettings(config,ZProjectSettings.General))
            {
                EditorUserBuildSettings.development = false;

                if (uarSettings != null && uarSettings.EnableRealtimeReflections && !QualitySettings.realtimeReflectionProbes)
                {
                    QualitySettings.realtimeReflectionProbes = true;
                    Debug.Log("[NOTE] Enabled realtime reflections. However your build target platform setting may vary! Make sure it's enabled there!!");
                }
            }

            if (ZAssistant.MatchConfigSettings(config, ZProjectSettings.GraphicsOptimum))
            {
                PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
            }

            if (ZAssistant.MatchConfigSettings(config, ZProjectSettings.IosPermissions))
            {
                if (string.IsNullOrEmpty(PlayerSettings.iOS.cameraUsageDescription))
                    PlayerSettings.iOS.cameraUsageDescription = "Zappar camera access request!";
            }

#endif
        }


    }
}