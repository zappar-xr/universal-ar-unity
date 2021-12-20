using UnityEngine;
using UnityEditor;

namespace Zappar.Editor
{
    [CustomEditor(typeof(ZapparFaceMeshTarget))]
    [DisallowMultipleComponent]
    public class ZapparFaceMeshTargetEditor : UnityEditor.Editor
    {
        private bool m_usingFullHead;

        private ZapparFaceMeshTarget m_target = null;

        private void OnEnable()
        {
            m_target = (ZapparFaceMeshTarget)target;
            if (m_target.FaceTrackingAnchor == null)
            {
                Debug.Log("Assign Face tracking anchor for this face mesh");
                return; 
            }
            m_usingFullHead = m_target.UseDefaultFullHead;

            if(!m_target.HaveInitializedFaceMesh)
            {
                m_target.CreateMesh();
            }
        }

        private void CheckForHeadMeshUpdate()
        {
            if (m_usingFullHead != m_target.UseDefaultFullHead)
            {
                m_target.CreateMesh(true);
                m_usingFullHead = m_target.UseDefaultFullHead;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUI.changed)
            {
                CheckForHeadMeshUpdate();
            }
        }

    }
}
