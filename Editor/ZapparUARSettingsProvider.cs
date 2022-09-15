using System.IO;
using UnityEngine;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
namespace Zappar.Editor
{
    static class ZapparUARSettingsProvider
    {
        class Styles
        {
            public static GUIContent ImageTargetPreview = new GUIContent("Enable Image Tracker Preview", "Add preview for image tracking target in editor");
            public static GUIContent ExcludeZPT = new GUIContent("Exclude zpt from build", "Remove ZPT files from build to minimize size. Recommended for builds not using image tracking type.");
            public static GUIContent ConcurrentFaceTracker = new GUIContent("Concurrent Face Trackers", "Number of faces to track at the same time");
            public static GUIContent RequestPermissionUI = new GUIContent("Permission UI (WebGL)", "Request device permissions with additional UI on WebGL build");
            public static GUIContent RealtimeReflections = new GUIContent("Enable Realtime Reflection", "Use ZCV camera source for realtime reflection");
            public static GUIContent DebugMode = new GUIContent("ZCV debug mode", "write logs to editor or to a file ");
            public static GUIContent LogLevel = new GUIContent("ZCV log level", "Log levels");
            public static GUIStyle Heading1 = new GUIStyle() { richText=true, fontStyle = FontStyle.Bold, fontSize = (int)(EditorGUIUtility.singleLineHeight * 0.85f) };
            public static Color Background = new Color(1f, 1f, 1f, 0.05f);
        }

        struct Constants
        {
            public const string PackageVersionProp = "m_packageVersion";
            public const string ImagePreviewProp = "m_enableImageTargetPreview";
            public const string ExcludeZPTProp = "m_excludeZPTFromBuild";
            public const string FaceTrackerProp = "m_concurrentFaceTrackerCount";
            public const string PermissionRequestProp = "m_permissionRequestUI";
            public const string RealtimeReflectionProp = "m_enableRealtimeReflections";
            public const string DebugModeProp = "m_debugMode";
            public const string LogLevelProp = "m_logLevel";
        }

        public static void GUIHandler(string searchContext, SerializedObject settings)
        {
            settings.Update();

            EditorGUILayout.HelpBox("Version: " + settings.FindProperty(Constants.PackageVersionProp).stringValue, MessageType.Info);

            EditorGUILayout.Space(10);
            GUILayout.Label("<color=#CCCCCC>Runtime Settings</color>", Styles.Heading1);
            float labelWidth = GUILayoutUtility.GetLastRect().width;
            Rect runRect = EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = labelWidth / 2;
            EditorGUILayout.PropertyField(settings.FindProperty(Constants.FaceTrackerProp), Styles.ConcurrentFaceTracker);
            if(EditorGUI.EndChangeCheck())
            {
                int val = settings.FindProperty(Constants.FaceTrackerProp).intValue;
                if (val < 1 || val > 10)
                {
                    Debug.Log("An ideal range for tracker would be [1-5]");
                    if (val < 1) settings.FindProperty(Constants.FaceTrackerProp).intValue = 1;
                }
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(settings.FindProperty(Constants.RealtimeReflectionProp), Styles.RealtimeReflections);
            if (EditorGUI.EndChangeCheck())
            {
                bool add = settings.FindProperty(Constants.RealtimeReflectionProp).boolValue;
                if (add) ZapparUtilities.CreateLayer(ZapparReflectionProbe.ReflectionLayer); else ZapparUtilities.RemoveLayer(ZapparReflectionProbe.ReflectionLayer);
                if (add && !QualitySettings.realtimeReflectionProbes)
                {
                    Debug.LogError("Please enable Realtime reflections from project Quality settings as well!");
                }
            }
            EditorGUILayout.PropertyField(settings.FindProperty(Constants.PermissionRequestProp), Styles.RequestPermissionUI);
            EditorGUILayout.EndVertical();
            EditorGUI.DrawRect(runRect, Styles.Background);
            //GUI.Box(runRect, GUIContent.none);

            EditorGUILayout.Space(15);

            GUILayout.Label("<color=#CCCCCC>Editor Settings</color>", Styles.Heading1);
            Rect edRect = EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            EditorGUILayout.PropertyField(settings.FindProperty(Constants.ImagePreviewProp), Styles.ImageTargetPreview);
            if (!settings.FindProperty(Constants.ImagePreviewProp).boolValue)
            {
                Rect rect = EditorGUILayout.BeginVertical();
                EditorGUILayout.TextArea("<color=white>    <b>Note:</b> Already added preview images would still be present unless removed manually</color>", new GUIStyle() { fontSize = (int)EditorGUIUtility.singleLineHeight/2, wordWrap = true, richText = true });
                EditorGUILayout.EndVertical();
                GUI.Box(rect, GUIContent.none); 
                EditorGUILayout.Space(2);
            }
            EditorGUILayout.PropertyField(settings.FindProperty(Constants.ExcludeZPTProp), Styles.ExcludeZPT);
            if(settings.FindProperty(Constants.ExcludeZPTProp).boolValue)
            {
                Rect rect = EditorGUILayout.BeginVertical();
                EditorGUILayout.TextArea("<color=white>    Don't use this if you're using any <b>image tracking</b> targets.</color>", new GUIStyle() { fontSize = (int)EditorGUIUtility.singleLineHeight / 2, wordWrap = true, richText = true });
                EditorGUILayout.EndVertical();
                GUI.Box(rect, GUIContent.none);
                EditorGUILayout.Space(2);
            }
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(settings.FindProperty(Constants.DebugModeProp), Styles.DebugMode);
            EditorGUILayout.PropertyField(settings.FindProperty(Constants.LogLevelProp), Styles.LogLevel);

#if ZAPPAR_SRP
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox("Scriptable Pipeline Enabled for Universal AR", MessageType.None, true);
#endif
            EditorGUILayout.EndVertical();
            EditorGUI.DrawRect(edRect, Styles.Background);

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
            return File.Exists(ZapparUARSettings.MySettingsPathInPackage);
        }


        public static ZapparUARSettings GetOrCreateSettings()
        {
            PackageInfo info = PackageInfo.FindForAssetPath("Packages/com.zappar.uar/package.json");
            var settings = AssetDatabase.LoadAssetAtPath<ZapparUARSettings>(ZapparUARSettings.MySettingsPathInPackage);
            if (settings == null || settings.PackageVersion != info.version)
            {
                if (!Directory.Exists(Path.GetDirectoryName(ZapparUARSettings.MySettingsPathInPackage)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ZapparUARSettings.MySettingsPathInPackage));
                }
                settings = ScriptableObject.CreateInstance<ZapparUARSettings>();
                settings.PackageVersion = info.version;
                settings.ImageTargetPreviewEnabled = true;
                settings.ConcurrentFaceTrackerCount = 2;
                settings.PermissionRequestUI = true;
                settings.EnableRealtimeReflections = false;
                settings.ExcludeZPTFromBuild = false;
                settings.DebugMode = Z.DebugMode.UnityLog;
                settings.LogLevel = Z.LogLevel.WARNING;
                AssetDatabase.CreateAsset(settings, ZapparUARSettings.MySettingsPathInPackage);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        private static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}