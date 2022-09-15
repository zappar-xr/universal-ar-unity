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

        enum ImageTargetType : int
        {
            Flat = 0,
            Cylindrical,
            Conical
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
        private List<string> m_zptFiles = new List<string>();
        [SerializeField]
        private ImageTargetType m_targetType = ImageTargetType.Flat;

        private const int TrackIndx = 0;
        private const string PreviewObjName = "Preview Object";
        private Matrix4x4 Rx = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, -1, 0, 0),
            new Vector4(0, 0, 0, 1));

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

                EditorGUI.BeginDisabledGroup(m_targetType != ImageTargetType.Flat);
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
                EditorGUI.EndDisabledGroup();
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
                if (!Z.HasInitialized() || (m_target.PreviewImageObject != null))
                    return;

                OnZPTFilenameChange(m_target.Target);
            }
            else
            {
                //clear the preview
                ClearEditorPipeline();

                if (m_target.PreviewImageObject != null)
                {
                    DestroyImmediate(m_target.PreviewImageObject);
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

            uint type = Z.ImageTrackerTargetType(m_editorTracker.Value, TrackIndx);
            m_targetType = (ImageTargetType)type;
            Debug.Log("Image target type: " + m_targetType);

            int previewWidth = Z.ImageTrackerTargetPreviewRgbaWidth(m_editorTracker.Value, TrackIndx);
            int previewHeight = Z.ImageTrackerTargetPreviewRgbaHeight(m_editorTracker.Value, TrackIndx);

            Debug.Log("Preview image res: " + previewWidth + "x" + previewHeight);

            if (previewWidth == 0 || previewHeight == 0)
                return;

            float topRadius = Z.ImageTrackerTargetRadiusTop(m_editorTracker.Value, TrackIndx);
            float bottomRadius = Z.ImageTrackerTargetRadiusBottom(m_editorTracker.Value, TrackIndx);
            float sideLength = Z.ImageTrackerTargetSideLength(m_editorTracker.Value, TrackIndx);
            float meterScale = Z.ImageTrackerTargetPhysicalScaleFactor(m_editorTracker.Value, TrackIndx);
            if (meterScale <= 0) meterScale = 1;
#if UNITY_EDITOR
            Debug.Log("params: TR:" + topRadius + " BR:" + bottomRadius + " SL:" + sideLength + " PS: " + meterScale);
#endif
            if (m_target.PreviewImageObject == null)
            {
                GameObject previewObj = null;
                for (int i = 0; i < m_target.transform.childCount && previewObj == null; ++i)
                {
                    if (m_target.transform.GetChild(i).gameObject.name == PreviewObjName)
                    {
                        previewObj = m_target.transform.GetChild(i).gameObject;
                        if (previewObj.GetComponent<MeshFilter>() == null) previewObj = null;
                    }
                }
                if (previewObj == null)
                {
                    //m_target.PreviewImagePlane = GameObject.CreatePrimitive(PrimitiveType.Quad) as GameObject;
                    m_target.PreviewImageObject = new GameObject(PreviewObjName, new[] { typeof(MeshFilter), typeof(MeshRenderer) });
                    Undo.RegisterCreatedObjectUndo(m_target.PreviewImageObject, "New preview object");
                    m_target.PreviewImageObject.transform.SetParent(m_target.transform);
                }
                else
                {
                    m_target.PreviewImageObject = previewObj;
                    EditorUtility.SetDirty(m_target.gameObject);
                }
            }
            
            m_target.PreviewImageObject.transform.localPosition = Vector3.zero;
            MeshFilter mf = m_target.PreviewImageObject.GetComponent<MeshFilter>();
            if (mf.sharedMesh != null)
                mf.sharedMesh.Clear();
            else
                mf.sharedMesh = new Mesh();
            mf.sharedMesh.name = "Z" + m_targetType + "_target";

            int[] tris = Z.ImageTrackerTargetPreviewMeshIndices(m_editorTracker.Value, TrackIndx);
            for(int i=0;i<tris.Length;i+=3)
            {
                tris[i] = tris[i] + tris[i + 2];
                tris[i + 2] = tris[i] - tris[i + 2];
                tris[i] = tris[i] - tris[i + 2];
            }
            float[] verts = Z.ImageTrackerTargetPreviewMeshVertices(m_editorTracker.Value, TrackIndx);
            float[] norms = Z.ImageTrackerTargetPreviewMeshNormals(m_editorTracker.Value, TrackIndx);
            float[] uvs = Z.ImageTrackerTargetPreviewMeshUvs(m_editorTracker.Value, TrackIndx);
            Vector3[] vs = new Vector3[verts.Length / 3];
            for (int i = 0; i < vs.Length; ++i)
            {
                vs[i] = new Vector3(
                      verts[3 * i],
                      verts[3 * i + 1],
                      -verts[3 * i + 2]
                      );
                if (m_targetType == ImageTargetType.Flat && m_orient == ZapparImageTrackingTarget.PlaneOrientation.Flat)
                    vs[i] = Rx.MultiplyPoint3x4(vs[i]);
            }
            mf.sharedMesh.vertices = vs;
            for (int i = 0; i < vs.Length; ++i)
            {
                vs[i] = new Vector3(
                      norms[3 * i],
                      norms[3 * i + 1],
                      -norms[3 * i + 2]
                      );
                if (m_targetType == ImageTargetType.Flat && m_orient == ZapparImageTrackingTarget.PlaneOrientation.Flat)
                    vs[i] = Rx.MultiplyPoint3x4(vs[i]);
            }
            mf.sharedMesh.normals = vs;
            mf.sharedMesh.triangles = tris;
            Vector2[] uv = new Vector2[uvs.Length / 2];
            for (int i = 0; i < uv.Length; ++i)
                uv[i] = new Vector2(uvs[2 * i], uvs[2 * i + 1]);
            mf.sharedMesh.uv = uv;

            byte[] previewData = Z.ImageTrackerTargetPreviewRgba(m_editorTracker.Value, TrackIndx);
            
            Texture2D texture = new Texture2D(previewWidth, previewHeight, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(previewData);
            texture.Apply();
            
            Material material = new Material(Shader.Find("Zappar/UnlitTextureUV"));
            material.mainTexture = texture;
            material.SetFloat("_FlipTexY", 1);
            material.EnableKeyword("FlipTexV");
            if (m_targetType != ImageTargetType.Flat)
                material.SetInt("_Culling", 0);
            m_target.PreviewImageObject.GetComponent<Renderer>().material = material;
            if (m_targetType != ImageTargetType.Flat)
                m_target.Orientation = ZapparImageTrackingTarget.PlaneOrientation.Vertical;
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