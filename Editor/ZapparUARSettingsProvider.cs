using System.IO;
using UnityEngine;
using UnityEditor;

namespace Zappar.Editor
{
    static class ZapparUARSettingsProvider
    {
        class Styles
        {
            public static GUIContent ImageTargetPreview = new GUIContent("Enable Image Tracker Preview", "Show preview of image target");
            public static GUIContent DebugMode = new GUIContent("ZCV debug mode", "write logs to editor or to a file ");
            public static GUIContent LogLevel = new GUIContent("ZCV log level", "Log levels");
        }

        public static void GUIHandler(string searchContext, SerializedObject settings)
        {
            settings.Update();

            EditorGUILayout.LabelField(string.Format("Version 1.0"));
            EditorGUILayout.PropertyField(settings.FindProperty("m_EnableImageTargetPreview"), Styles.ImageTargetPreview);
            EditorGUILayout.PropertyField(settings.FindProperty("m_DebugMode"), Styles.DebugMode);
            EditorGUILayout.PropertyField(settings.FindProperty("m_LogLevel"), Styles.LogLevel);

            settings.ApplyModifiedProperties();
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            if (!IsSettingsAvailable())
            {
                GetSerializedSettings();
            }

            var provider = new SettingsProvider("Project/ZapparUARSettings", SettingsScope.Project)
            {
                label = "Zappar Universal AR",
                guiHandler = (searchContext) =>
                {
                    var settings = GetSerializedSettings();
                    GUIHandler(searchContext, settings);
                },
                // Automatically extract all keywords from the Styles.
                keywords = SettingsProvider.GetSearchKeywordsFromGUIContentProperties<Styles>()
            };

            return provider;
        }

        private static bool IsSettingsAvailable()
        {
            return File.Exists(ZapparUARSettings.MySettingsPath);
        }


        public static ZapparUARSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ZapparUARSettings>(ZapparUARSettings.MySettingsPath);
            if (settings == null)
            {
                if (Directory.Exists(Path.GetDirectoryName(ZapparUARSettings.MySettingsPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ZapparUARSettings.MySettingsPath));
                }
                settings = ScriptableObject.CreateInstance<ZapparUARSettings>();
                settings.ImageTargetPreviewEnabled = true;
                settings.DebugMode = Z.DebugMode.UnityLog;
                settings.LogLevel = Z.LogLevel.WARNING;
                AssetDatabase.CreateAsset(settings, ZapparUARSettings.MySettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}