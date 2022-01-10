using UnityEngine;
using UnityEditor;

namespace Zappar.Obsolete.Editor
{
    [CustomEditor(typeof(ZapparFaceTrackingTarget))]
    [DisallowMultipleComponent]
    public class ZapparFaceTrackingTargetEditor : UnityEditor.Editor
    {
        private ZapparFaceTrackingTarget m_target = null;
        private int m_maxTrackerAllowed = 1;
        private GUIContent m_idGui;
        
        private void OnEnable()
        {
            if (Application.isPlaying) return;

            var settings = AssetDatabase.LoadAssetAtPath<ZapparUARSettings>(ZapparUARSettings.MySettingsPathInPackage);
            m_target = (ZapparFaceTrackingTarget)target;
            m_maxTrackerAllowed = settings.ConcurrentFaceTrackerCount;

            m_idGui = new GUIContent("Face Number", "Unique id for face tracker [0-" + (m_maxTrackerAllowed - 1) + "]");            
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Application.isPlaying) return;

            int id = EditorGUILayout.IntField(m_idGui, m_target.FaceTrackingId);
            if (id < 0 || id >= m_maxTrackerAllowed) { Debug.Log("Please update UAR settings to fit the range!"); }
            else if (id != m_target.FaceTrackingId) { m_target.FaceTrackingId = id; EditorUtility.SetDirty(m_target.gameObject); }
        }
    }
}