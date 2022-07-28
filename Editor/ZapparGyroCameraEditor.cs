using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Zappar.Editor
{
    [CustomEditor(typeof(ZapparGyroCamera))]
    public class ZapparGyroCameraEditor : UnityEditor.Editor
    {
        class Styles
        {
            public static GUIContent CameraBackground = new GUIContent("Camera Background", "Use rear camera feed as scene background");
            public static GUIContent CameraId = new GUIContent("Editor Camera", "Camera to use when in Play mode.");
        }

        private ZapparGyroCamera m_target;
        private List<string> m_editorCams = new List<string>();
        private Dictionary<string, string> m_editorIdCams = new Dictionary<string, string>();
        private int m_camIndx = -1;

        private void OnEnable()
        {
            if (Application.isPlaying)
                return;

            UpdateCamList();
            m_target = (ZapparGyroCamera)target;
            if (string.IsNullOrEmpty(m_target.EditorCameraId) && m_editorCams.Count > 0)
            {
                m_camIndx = 0;
                (target as ZapparGyroCamera).EditorCameraId = m_editorCams[0];
            }
            else
            {
                m_camIndx = m_editorCams.IndexOf(m_target.EditorCameraId);
            }
        }

        private void UpdateCamList()
        {
            try
            {
                for (int i = 0; i < Z.CameraCount(); ++i)
                {
                    string cam = Z.CameraName(i);
                    string id = Z.CameraId(i);

                    if (m_editorIdCams.ContainsKey(id)) continue;

                    if (m_editorCams.Contains(cam))
                    {
                        cam += " (" + id + ")";
                    }
                    m_editorCams.Add(cam);
                    m_editorIdCams.Add(id, cam);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed checking camera list. Exception: " + e.Message);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("The camera will remain at the origin of your scene, but rotate according to the device's orientation in 3D space", MessageType.Info);

            if (Application.isPlaying)
                return;
            
            m_target = (ZapparGyroCamera)target;
            bool m_camBackground = EditorGUILayout.Toggle(Styles.CameraBackground, m_target.UseCameraBackground);

            if ( m_camBackground!= m_target.UseCameraBackground)
            {
                ZapparGyroCameraBackground zcb = m_target.transform.GetComponentInChildren<ZapparGyroCameraBackground>();
                if(m_camBackground && zcb==null)
                {
                    UpdateCamList();
                    m_target.UseCameraBackground = true;
                    Camera main = m_target.transform.GetComponent<Camera>();
                    main.clearFlags = CameraClearFlags.Nothing;
                    main.depth = 1;
                    GameObject child = new GameObject("Zappar Camera Background", new[] { typeof(Camera), typeof(ZapparGyroCameraBackground) });
                    child.GetComponent<Camera>().cullingMask = 0;
                    child.transform.SetParent(m_target.transform);
                }else if(!m_camBackground && zcb!= null)
                {
                    m_target.UseCameraBackground = false;
                    m_target.EditorCameraId = "";
                    m_editorIdCams.Clear();
                    m_editorCams.Clear();
                    DestroyImmediate(zcb.transform.gameObject);
                    Camera main = m_target.transform.GetComponent<Camera>();
                    main.clearFlags = CameraClearFlags.Skybox;
                    main.depth = 0;
                }

                m_target.UseCameraBackground = m_camBackground;
                EditorUtility.SetDirty(m_target.gameObject);
            }

            if(m_target.UseCameraBackground)
            {
                int ind = EditorGUILayout.Popup(Styles.CameraId, m_camIndx, m_editorCams.ToArray());
                if (m_editorCams.Count > ind && ind != m_camIndx)
                {
                    m_camIndx = ind;
                    m_target.EditorCameraId = m_editorCams[ind];
                    EditorUtility.SetDirty(m_target.gameObject);
                }
            }
        }
    }
}