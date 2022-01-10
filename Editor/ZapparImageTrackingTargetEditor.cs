using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;

namespace Zappar.Editor
{
    [CustomEditor(typeof(ZapparImageTrackingTarget))]
    public class ZapparImageTrackingTargetEditor : UnityEditor.Editor
    {
        class Styles
        {
            public static GUIContent TargetContent = new GUIContent("Target", "Select the ZPT file you would like to track");
            public static GUIContent OrientationContent = new GUIContent("Orientation", "During play offset the tracker's rotation accordingly");
        }

        private string m_imgTarget;
        private int m_targetIndx;
        private ZapparImageTrackingTarget.PlaneOrientation m_orient;

        ZapparImageTrackingTarget m_target = null;
        private bool m_imgPreviewEnabled;
        [SerializeField]
        private IntPtr? m_editorPipeline = null;
        [SerializeField]
        private IntPtr? m_editorTracker = null;
        List<string> m_zptFiles = new List<string>();

        private const int TrackIndx = 0;
        private const string PreviewObjName = "Preview Image";

        private void OnEnable()
        {
            if (Application.isPlaying) return;
            
            var settings = AssetDatabase.LoadAssetAtPath<ZapparUARSettings>(ZapparUARSettings.MySettingsPathInPackage);
            m_imgPreviewEnabled = settings.ImageTargetPreviewEnabled;
            m_target = (ZapparImageTrackingTarget)target;
            
            UpdateZptList();
            
            if (m_zptFiles.Count == 0) return;
            
            if (string.IsNullOrEmpty(m_target.Target) || m_target.Target == "No ZPT files available.")
            {
                m_imgTarget = m_target.Target = m_zptFiles[0];
                m_targetIndx = 0;
            }
            else
            {
                m_imgTarget = m_target.Target;
                m_targetIndx = Mathf.Max(m_zptFiles.IndexOf(m_imgTarget), 0);
            }
            m_orient = m_target.Orientation;

            ToggleImagePreview(m_imgPreviewEnabled);
        }

        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying)
            {
                if (m_zptFiles?.Count > 0)
                {
                    m_targetIndx = Mathf.Max(m_zptFiles.IndexOf(m_imgTarget), 0);
                    int index = EditorGUILayout.Popup(Styles.TargetContent, m_targetIndx, m_zptFiles.ToArray());
                    if(index!= m_targetIndx)
                    {
                        m_imgTarget = m_zptFiles[index];
                        m_targetIndx = index;
                        OnZPTFilenameChange(m_imgTarget); 
                        EditorUtility.SetDirty(m_target.gameObject);
                    }                    
                }
                else
                {
                    EditorGUILayout.LabelField("<color=#CC0011>No ZPT files found!</color>", new GUIStyle() { richText=true });
                }

                m_orient = (ZapparImageTrackingTarget.PlaneOrientation)EditorGUILayout.EnumPopup(Styles.OrientationContent, m_target.Orientation);
                if(m_orient != m_target.Orientation)
                {
                    m_target.Orientation = m_orient;
                    if (m_editorTracker == null)
                        OnZPTFilenameChange(m_imgTarget);
                    else
                        SetupImagePreview();
                    EditorUtility.SetDirty(m_target.gameObject);
                }
            }
            //base.OnInspectorGUI();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnSeenEvent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnNotSeenEvent"));
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnDisable()
        {
            ClearEditorPipeline();
        }

        private void ToggleImagePreview(bool enable)
        {
            if (m_target == null) return;

            if (enable)
            {
                if (!Z.HasInitialized() || (m_target.PreviewImagePlane != null))
                    return;

                OnZPTFilenameChange(m_target.Target);
            }
            else
            {
                //clear the preview
                ClearEditorPipeline();

                if (m_target.PreviewImagePlane != null)
                {
                    DestroyImmediate(m_target.PreviewImagePlane);
                    EditorUtility.SetDirty(m_target.gameObject);
                }
            }
        }

        private void OnZPTFilenameChange(string newTarget)
        {
            if (m_target==null || !m_target.gameObject.activeInHierarchy)
            {
                Debug.Log("Could not start LoadZPTTarget Coroutine as gameobject is inactive.");
                return;
            }
            
            if (newTarget == "No ZPT files available." || string.IsNullOrEmpty(newTarget))
                return;
            
            m_target.Target = newTarget;

            if (!m_imgPreviewEnabled) return;

            if (m_editorPipeline == null)
                m_editorPipeline = Z.PipelineCreate();
            if (m_editorTracker == null)
                m_editorTracker = Z.ImageTrackerCreate(m_editorPipeline.Value);

            EditorCoroutineUtility.StartCoroutine(Z.LoadZPTTarget(newTarget, TargetDataAvailableCallback), m_target);
        }

        private void SetupImagePreview()
        { 
            if (m_target==null || !m_imgPreviewEnabled) return;
            
            int previewWidth = Z.ImageTrackerTargetPreviewRgbaWidth(m_editorTracker.Value, TrackIndx);
            int previewHeight = Z.ImageTrackerTargetPreviewRgbaHeight(m_editorTracker.Value, TrackIndx);

            Debug.Log("Preview image res: " + previewWidth + "x" + previewHeight);

            if (previewWidth == 0 || previewHeight == 0)
                return;

            if (m_target.PreviewImagePlane == null)
            {
                GameObject plane = null;
                for (int i = 0; i < m_target.transform.childCount; ++i)
                {
                    if (m_target.transform.GetChild(i).gameObject.name == PreviewObjName)
                    {
                        plane = m_target.transform.GetChild(i).gameObject;
                        if (plane.GetComponent<MeshFilter>() == null) plane = null;
                    }
                }
                if (plane == null)
                {
                    m_target.PreviewImagePlane = GameObject.CreatePrimitive(PrimitiveType.Quad) as GameObject;
                    Undo.RegisterCreatedObjectUndo(m_target.PreviewImagePlane, "New preview object");
                    m_target.PreviewImagePlane.name = PreviewObjName;
                    m_target.PreviewImagePlane.transform.SetParent(m_target.transform);
                }
                else
                {
                    m_target.PreviewImagePlane = plane;
                    EditorUtility.SetDirty(m_target.gameObject);
                }
            }

            m_target.PreviewImagePlane.transform.localEulerAngles = m_target.Orientation == ZapparImageTrackingTarget.PlaneOrientation.Flat ?
                new Vector3(90, 0, 0) : new Vector3(0,0,0);
            m_target.PreviewImagePlane.transform.localPosition = Vector3.zero;

            float aspectRatio = (float)previewWidth / (float)previewHeight;
            const float scaleFactor = 2f;
            m_target.PreviewImagePlane.transform.localScale = new Vector3(aspectRatio, 1.0f, 1.0f) * scaleFactor;

            byte[] previewData = Z.ImageTrackerTargetPreviewRgba(m_editorTracker.Value, 0);

            Texture2D texture = new Texture2D(previewWidth, previewHeight, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(previewData);
            texture.Apply();

            Material material = new Material(Shader.Find("Zappar/UnlitTextureUV"));
            material.mainTexture = texture;
            material.SetFloat("_FlipTexY",1);
            material.EnableKeyword("FlipTexV");

            m_target.PreviewImagePlane.GetComponent<Renderer>().material = material;
        }

        private void TargetDataAvailableCallback(byte[] data)
        {
            if (m_editorTracker != null)
            {
                Z.ImageTrackerTargetLoadFromMemory(m_editorTracker.Value, data);
                SetupImagePreview();
            }
            else
            {
                Debug.LogError("No image tracker found to enable preview");
            }
        }

        private void UpdateZptList()
        {
            m_zptFiles.Clear();
            try
            {
                DirectoryInfo directory = new DirectoryInfo(Application.streamingAssetsPath);
                FileInfo[] files = directory.GetFiles("*.zpt");
                foreach (FileInfo file in files)
                {
                    m_zptFiles.Add(file.Name);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to check streaming assets path! Exception: " + e.Message);
            }
        }

        private void ClearEditorPipeline()
        {
            if (m_editorTracker != null) Z.ImageTrackerDestroy(m_editorTracker.Value);
            if (m_editorPipeline != null) Z.PipelineDestroy(m_editorPipeline.Value);
            m_editorPipeline = m_editorTracker = null;
        }
    }
}