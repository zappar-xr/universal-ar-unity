using UnityEditor;
using UnityEngine;

namespace Zappar.Editor
{
    [CustomEditor(typeof(ZapparFaceLandmark))]
    public class ZapparFaceLandmarkEditor : UnityEditor.Editor
    {
        private ZapparFaceLandmark m_target = null;
        private ZapparFaceLandmark.Face_Landmark_Name m_landmarkName = ZapparFaceLandmark.Face_Landmark_Name.LeftEye;

        private readonly Vector3[] m_landmarkPositions = {
            new Vector3(0.23f,0.18f,-0.7f), //left eye
            new Vector3(-0.23f,0.18f,-0.7f), //right eye
            new Vector3(0.54f,0.2f,-0.24f), //left ear
            new Vector3(-0.54f,0.2f,-0.24f), //right ear
            new Vector3(0,0.18f,-0.85f), //nose bridge
            new Vector3(0,-0.03f,-1f), //nose tip
            new Vector3(0,-0.14f,-0.8f), //nose base
            new Vector3(0,-0.26f,-0.8f), //lip top
            new Vector3(0,-0.36f,-0.8f), //lip botton
            new Vector3(0,-0.3f,-0.8f), //mouth center
            new Vector3(0,-0.59f,-0.8f), //chin
            new Vector3(0.23f,0.35f,-0.8f), //left eyebrow
            new Vector3(-0.23f,0.35f,-0.8f)  //right eyebrow
        };

        private void OnEnable()
        {
            if (Application.isPlaying) return;

            m_target = (ZapparFaceLandmark)target; 
            if (m_target.FaceTrackingAnchor == null)
            {
                Debug.Log("Assign Face tracking anchor for this face landmark");
                return;
            }
            m_landmarkName = m_target.LandmarkName;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying) return;

            m_target = (ZapparFaceLandmark)target;

            if (m_target.FaceTrackingAnchor != null && m_target.LandmarkName != m_landmarkName)
            {
                m_landmarkName = m_target.LandmarkName;
                m_target.transform.localPosition = m_landmarkPositions[(int)m_landmarkName];
            }
        }
    }
}