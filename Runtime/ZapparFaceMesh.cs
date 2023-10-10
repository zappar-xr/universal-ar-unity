using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Zappar
{
    public abstract class ZapparFaceMesh : MonoBehaviour
    {
        public IntPtr? FaceMeshPtr { get; protected set; } = null;
        private bool m_hasInitialised = false;
        
        public bool UseDefaultFullHead = true;
        public bool FillEyeLeft;
        public bool FillEyeRight;
        public bool FillMouth;
        public bool FillNeck;
        
        private ZapparFaceTrackingAnchor m_faceTracker;
        
        public Mesh UnityMesh { get; protected set; } = null;
        public bool HaveInitializedFaceMesh { get; protected set; } = false;
        
        private bool m_isMirrored = false;
        
        public abstract void UpdateMaterial();

        public abstract ZapparFaceTrackingAnchor GetFaceTrackingAnchor();
        
        public void InitFaceMeshOnStart()
        {
            m_faceTracker = GetFaceTrackingAnchor();
            if (m_faceTracker == null)
            {
                Debug.LogError("Missing face tracking anchor reference!");
                gameObject.SetActive(false);
                return;
            }
            
            m_faceTracker.RegisterPipelineInitCallback(OnFaceTrackerPipelineInitialised, true);
            
            CreateMesh(true);
            
            if (m_faceTracker.FaceTrackingTarget == null) return;
            
            if (m_faceTracker.FaceTrackingTarget.HasInitialized && !m_hasInitialised)
                OnFaceTrackerPipelineInitialised(m_faceTracker.FaceTrackingTarget.FaceTrackerPipeline.Value, m_faceTracker.FaceTrackingTarget.IsMirrored);
        }

        private void OnFaceTrackerPipelineInitialised(IntPtr pipeline, bool mirrored)
        {
            m_isMirrored = mirrored;
            
            m_hasInitialised = true;
            HaveInitializedFaceMesh = false;
            
            CreateMesh();
        }

        public void CreateMesh(bool force = false)
        {
            if (UnityMesh != null && !force)
                return;
            
            if (m_faceTracker == null)
                m_faceTracker = GetFaceTrackingAnchor();
            
            if (FaceMeshPtr == null)
            {
                FaceMeshPtr = Z.FaceMeshCreate();
            }
            else
            {
                if (Application.isPlaying)
                {
                    Z.FaceMeshClearVertexBuffer(FaceMeshPtr.Value);
                }
                Z.FaceMeshDestroy(FaceMeshPtr.Value);
                FaceMeshPtr = Z.FaceMeshCreate();
            }
            
            DestroyUnityMesh();
            LoadMeshData();
            
            UnityMesh = gameObject.GetComponent<MeshFilter>().sharedMesh ?? new Mesh();
            //Enable mesh buffers to be CPU writable -- by default they are immutable and only GPU-readable
            UnityMesh.MarkDynamic();
            UnityMesh.name = "ZFaceMesh" + (UseDefaultFullHead ? "_Full" : "");
            gameObject.GetComponent<MeshFilter>().sharedMesh = UnityMesh;
            
            UpdateMeshData();
            UpdateMaterial();
        }
        
        private void LoadMeshData()
        {
#if UNITY_EDITOR
            string filename;
            if (UseDefaultFullHead)
                filename = Z.FaceMeshFullHeadSimplifiedModelPath();
            else
                filename = Z.FaceMeshFaceModelPath();
            byte[] data = Z.LoadRawBytes(filename);
            Z.FaceMeshLoadFromMemory(FaceMeshPtr.Value, data, FillMouth, FillEyeLeft, FillEyeRight, FillNeck);
#else
            if (UseDefaultFullHead)
            {
                Z.FaceMeshLoadDefaultFullHeadSimplified(FaceMeshPtr.Value, FillMouth, FillEyeLeft, FillEyeRight, FillNeck);
            }
            else
            {
                if (!FillEyeLeft && !FillEyeRight && !FillMouth)
                {
                    Z.FaceMeshLoadDefault(FaceMeshPtr.Value);
                }
                else Z.FaceMeshLoadDefaultFace(FaceMeshPtr.Value, FillEyeLeft, FillEyeRight, FillMouth);
            }
#endif            
        }

        private void UpdateMeshData()
        {
            if (UnityMesh == null || m_faceTracker == null)
                return;

            Z.FaceMeshUpdate(FaceMeshPtr.Value, m_faceTracker.Identity, m_faceTracker.Expression, m_isMirrored);

            if (HaveInitializedFaceMesh || Z.FaceMeshVerticesSize(FaceMeshPtr.Value) == 0) return;
                        
            if (!HaveInitializedFaceMesh)
            {
                // Do not change this layout!
                var desiredVertexLayout = new[]
                {
                        new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                        new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                        //new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4),
                        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
                    };
                UnityMesh.SetVertexBufferParams(Z.FaceMeshVerticesSize(FaceMeshPtr.Value)/3, desiredVertexLayout);
                float[] m_faceVertices = new float[Z.FaceMeshVerticesSize(FaceMeshPtr.Value)];
                float[] m_faceNormals = new float[Z.FaceMeshNormalsSize(FaceMeshPtr.Value)];
                Z.UpdateFaceMeshVertices(FaceMeshPtr.Value, ref m_faceVertices);
                Z.UpdateFaceMeshNormals(FaceMeshPtr.Value, ref m_faceNormals);
                UnityMesh.vertices = Z.UpdateFaceMeshVerticesForUnity(m_faceVertices);
                UnityMesh.normals = Z.UpdateFaceMeshNormalsForUnity(m_faceNormals);
                UnityMesh.triangles = Z.UpdateFaceMeshTrianglesForUnity(Z.FaceMeshIndices(FaceMeshPtr.Value));
                UnityMesh.uv = Z.UpdateFaceMeshUVsForUnity(Z.FaceMeshUvs(FaceMeshPtr.Value));
                // Make sure to have vertex colors so that the plugin can rely on a known vertex layout (position+normal+color+UV). 
                // Since Unity 2019.3 it's easier since there are APIs to query all that info.
                //var colors = UnityMesh.colors;
                //UnityMesh.colors = colors;
                
                if (Application.isPlaying)
                {
                    Z.FaceMeshSetVertexBuffer(FaceMeshPtr.Value, UnityMesh);
                }
                HaveInitializedFaceMesh = true;
            }
        }

        private void Update()
        {
            if (!m_hasInitialised || !m_faceTracker.FaceIsVisible)
                return;

            UpdateMeshData();
        }

        private void OnDestroy()
        {
            m_faceTracker.RegisterPipelineInitCallback(OnFaceTrackerPipelineInitialised, false);

            if (FaceMeshPtr != null && Application.isPlaying)
            {
                Z.FaceMeshClearVertexBuffer(FaceMeshPtr.Value);
                Z.FaceMeshDestroy(FaceMeshPtr.Value);
            }
            
            DestroyUnityMesh();
            m_hasInitialised = false;
        }
        
        protected void DestroyUnityMesh()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                Destroy(UnityMesh);
            else
                DestroyImmediate(UnityMesh);
#else
            Destroy(UnityMesh);
#endif
            UnityMesh = null;
            HaveInitializedFaceMesh = false;
        }
    }
}