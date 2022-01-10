using UnityEditor;
using UnityEngine;

namespace Zappar.Editor
{
    [CustomEditor(typeof(ZapparInstantTrackingTarget)), DisallowMultipleComponent]
    public class ZapparInstantTrackingTargetEditor : UnityEditor.Editor
    {
        class Styles
        {
            public static GUIContent ZCamera = new GUIContent("Camera","Zappar camera that provides device orientation for Z placement");
            public static GUIContent MinZDistance = new GUIContent("Min Z","Minimum Z distance away from camera");
            public static GUIContent MaxZDistance = new GUIContent("Max Z", "Maximum Z distance away from camera");
            public static Color Background = new Color(1f, 1f, 1f, 0.05f);
        }

        ZapparInstantTrackingTarget m_target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            m_target = (ZapparInstantTrackingTarget)target;

            if (m_target.MoveAnchorOnZ)
            {
                Rect adParam = EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_zCamera"), Styles.ZCamera);

                Rect scale = GUILayoutUtility.GetLastRect();
                EditorGUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
                
                var minZ = serializedObject.FindProperty("m_minZDistance");
                var maxZ = serializedObject.FindProperty("m_maxZDistance");
                EditorGUIUtility.labelWidth = scale.width / 3f;

                minZ.floatValue = EditorGUILayout.FloatField(Styles.MinZDistance, minZ.floatValue);
                maxZ.floatValue = EditorGUILayout.FloatField(Styles.MaxZDistance, maxZ.floatValue);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUI.DrawRect(adParam, Styles.Background);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}