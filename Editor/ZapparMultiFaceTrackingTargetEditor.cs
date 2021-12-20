using UnityEditor;
using UnityEngine;

namespace Zappar.Editor
{
    [CustomEditor(typeof(ZapparMultiFaceTrackingTarget))]
    [DisallowMultipleComponent]
    public class ZapparMultiFaceTrackingTargetEditor : UnityEditor.Editor
    {
        private ZapparMultiFaceTrackingTarget m_target = null;
        private ZapparUARSettings m_settings = null;

        class Styles
        {
            public static GUIContent AnchorCount = new GUIContent("Anchors count", "Number of face tracking anchors. Update Universal AR setting to adjust the limit.");
            public static GUIContent AddAnchorr = new GUIContent("Add New Anchor", "Add new face tracking anchor for this target");
            public static GUIContent RemoveAnchor = new GUIContent("Remove Last Anchor", "Remove last face tracking anchor for this target");
            public static GUIStyle Heading1 = new GUIStyle() { richText = true, fontStyle = FontStyle.Bold, fontSize = (int)(EditorGUIUtility.singleLineHeight * 0.85f) };
            public static GUIStyle NormalText = new GUIStyle() { richText = true };
        }

        public void OnEnable()
        {
            if (Application.isPlaying) return;

            m_settings = AssetDatabase.LoadAssetAtPath<ZapparUARSettings>(ZapparUARSettings.MySettingsPathInPackage);
            if (m_settings == null)
            {
                Debug.LogError("UAR Settings not found!");
                return;
            }
            m_target = (ZapparMultiFaceTrackingTarget)target;
            if(m_target.NumberOfAnchors==0)
            {
                if (m_target.GetComponentInChildren<ZapparFaceTrackingAnchor>() != null)
                {
                    foreach (var anchor in m_target.GetComponentsInChildren<ZapparFaceTrackingAnchor>())
                    {
                        m_target.RegisterAnchor(anchor, true);
                    }
                }
                else if (m_target.transform.childCount == 0)
                {
                    AddNewAnchor();
                }
            }

            ValidateTrackersList();
        }

        private void AddNewAnchor()
        {
            GameObject go = ZAssistant.GetZapparFaceTrackingAnchor();
            ZapparFaceTrackingAnchor anchor = go.GetComponent<ZapparFaceTrackingAnchor>();
            go.GetComponentInChildren<ZapparFaceDepthMask>().FaceTrackingAnchor = anchor;
            anchor.FaceTrackingTarget = m_target;
            anchor.FaceTrackerIndex = m_target.NumberOfAnchors;
            m_target.RegisterAnchor(anchor, true);
            go.transform.name += " " + anchor.FaceTrackerIndex.ToString();
            go.transform.SetParent(m_target.transform);
            Undo.RegisterCreatedObjectUndo(go, "New Face Anchor");
        }

        public override void OnInspectorGUI()
        {
            m_target = (ZapparMultiFaceTrackingTarget)target;

            EditorGUILayout.TextField(Styles.AnchorCount, "<color=#CCCCCC>" + m_target.NumberOfAnchors.ToString() + "</color>", Styles.NormalText);

            if (Application.isPlaying) return;

            EditorGUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.ExpandWidth(true) });

            EditorGUI.BeginDisabledGroup(m_settings.ConcurrentFaceTrackerCount <= m_target.NumberOfAnchors);            
            if (GUILayout.Button(Styles.AddAnchorr))
            {
                //Debug.Log("Adding new anchor");
                AddNewAnchor();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(m_target.NumberOfAnchors <= 1);
            if(GUILayout.Button(Styles.RemoveAnchor))
            {
                //Debug.Log("Removing anchor");
                ZapparFaceTrackingAnchor lAnchor = m_target.FaceAnchors[m_target.NumberOfAnchors - 1];
                m_target.RegisterAnchor(lAnchor, false);
                DestroyImmediate(lAnchor.gameObject);
                EditorUtility.SetDirty(m_target.gameObject);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FaceAnchors"), new GUIContent("Anchors list"), true);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }

        private void ValidateTrackersList()
        {
            ZapparMultiFaceTrackingTarget faceTarget = (ZapparMultiFaceTrackingTarget)target;
            if (faceTarget.FaceAnchors.RemoveAll(ent => ent == null) > 0)
            {
                int i = 0;
                foreach (var anchor in faceTarget.FaceAnchors)
                {
                    anchor.FaceTrackerIndex = i++;
                }
            }
        }
    }
}