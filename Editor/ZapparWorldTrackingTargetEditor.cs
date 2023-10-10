using UnityEditor;
using UnityEngine;

namespace Zappar.Editor
{
    [CustomEditor(typeof(ZapparWorldTrackingTarget)), DisallowMultipleComponent]
    public class ZapparWorldTrackingTargetEditor : UnityEditor.Editor
    {
        class Styles
        {
            public static GUIContent GAnchor = new GUIContent("Ground Anchor", "Anchor transform that contains your 3D scene locked on a tracked plane.");
            public static GUIContent InitTex = new GUIContent("Tracker init image", "On screen texture to show while tracker is in initialization stage");
            public static GUIContent InitTexTime = new GUIContent("Anim Time", "Time it takes for texture to move across the screen width.");
            public static GUIContent ZCamera = new GUIContent("Camera", "Zappar camera that provides device orientation for Z placement");
            public static GUIContent MinDistance = new GUIContent("Min Dist", "Minimum distance away from ZapparCamera");
            public static GUIContent MaxDistance = new GUIContent("Max Dist", "Maximum distance away from ZapparCamera");
            public static Color Background = new Color(1f, 1f, 1f, 0.05f);
        }

        ZapparWorldTrackingTarget m_target;

        public override void OnInspectorGUI()
        {
            m_target = (ZapparWorldTrackingTarget)target;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GroundAnchor"), Styles.GAnchor);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_showInitializationAnim"));
            if(m_target.ShowInitializationAnim)
            {
                Rect adParam = EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("InitScreenTexture"), Styles.InitTex);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("InitTextureTime"), Styles.InitTexTime);
                EditorGUILayout.EndVertical();
                EditorGUI.DrawRect(adParam, Styles.Background);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_placeOnTouch"));
             EditorGUILayout.PropertyField(serializedObject.FindProperty("m_showPlacementIndicator"));
            
            if (m_target.m_showPlacementIndicator)
            {
                Rect adParam = EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IndicatorProps"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_zCamera"), Styles.ZCamera);

                Rect scale = GUILayoutUtility.GetLastRect();
                float labelW = EditorGUIUtility.labelWidth;
                EditorGUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.ExpandWidth(true) });

                var minDist = serializedObject.FindProperty("MinDistance");
                var maxDist = serializedObject.FindProperty("MaxDistance");
                EditorGUIUtility.labelWidth = scale.width / 3f;

                minDist.floatValue = EditorGUILayout.FloatField(Styles.MinDistance, minDist.floatValue);
                maxDist.floatValue = EditorGUILayout.FloatField(Styles.MaxDistance, maxDist.floatValue);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUI.DrawRect(adParam, Styles.Background);
                EditorGUIUtility.labelWidth = labelW;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_showAnchorWithIndicator"));

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInitializationComplete")); EditorGUILayout.PropertyField(serializedObject.FindProperty("OnGroundAnchorPlaced"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTrackingReset"));


            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(5);

                EditorGUILayout.LabelField(new GUIContent("Simulate options"));
                EditorGUI.BeginDisabledGroup(m_target.TrackerInitialized);
                if (GUILayout.Button(new GUIContent("Switch to anchor placement","To allow visual testing of placement inidicator within editor"))) {
                    m_target.SimulatePlacementIndicator();
                }
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}