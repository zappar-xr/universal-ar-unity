using System;
using System.Collections.Generic;
using UnityEngine;

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

        private ZapparFaceTrackingTarget m_faceTracker;

        public Mesh UnityMesh { get; protected set; } = null;
        public bool HaveInitialisedFaceMesh { get; protected set; } = false;

        private bool m_isMirrored;

        private IntPtr m_faceTrackingTargetPipeline;
        private int m_faceTrackingTargetId;

        private float[] m_faceVertices = null;
        private float[] m_faceNormals = null;

        public abstract void UpdateMaterial();
        
        public abstract ZapparFaceTrackingTarget GetFaceTrackingTarget();

        public void InitFaceMeshOnStart()
        {
            m_faceTracker = GetFaceTrackingTarget();
            if (m_faceTracker == null) 
            { 
                Debug.LogError("No face tracking target reference found!");
                return; 
            }

            ZapparFaceTrackingManager.RegisterPipelineCallback(OnFaceTrackerPipelineInitialised);

            CreateMesh(true);
        }

        private void OnFaceTrackerPipelineInitialised(IntPtr pipeline, bool mirrored)
        {
            m_faceTrackingTargetPipeline = pipeline;
            m_faceTrackingTargetId = m_faceTracker.FaceTrackingId;
            m_isMirrored = mirrored;

            m_hasInitialised = true;
            HaveInitialisedFaceMesh = false;

            CreateMesh();
        }

        public void CreateMesh(bool force=false)
        {
            if (UnityMesh != null && !force)
                return;

            if (m_faceTracker == null)
                m_faceTracker = GetFaceTrackingTarget();

            if (FaceMeshPtr == null)
            {
                FaceMeshPtr = Z.FaceMeshCreate();
            }
            else
            {
                Z.FaceMeshDestroy(FaceMeshPtr.Value);
                FaceMeshPtr = Z.FaceMeshCreate();
            }

            DestroyUnityMesh();
            LoadMeshData();

            UnityMesh = new Mesh();
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
            if (UnityMesh == null || m_faceTracker==null)
                return;

            Z.FaceMeshUpdate(FaceMeshPtr.Value, m_faceTracker.Identity, m_faceTracker.Expression, m_isMirrored);

            if (m_faceVertices==null || m_faceVertices.Length == 0)
            {
                m_faceVertices = new float[Z.FaceMeshVerticesSize(FaceMeshPtr.Value)];
                m_faceNormals = new float[Z.FaceMeshNormalsSize(FaceMeshPtr.Value)];
            }

            Z.UpdateFaceMeshVertices(FaceMeshPtr.Value, ref m_faceVertices);
            Z.UpdateFaceMeshNormals(FaceMeshPtr.Value, ref m_faceNormals);
            UnityMesh.vertices = Z.UpdateFaceMeshVerticesForUnity(m_faceVertices);
            UnityMesh.normals = Z.UpdateFaceMeshNormalsForUnity(m_faceNormals);

            if (!HaveInitialisedFaceMesh)
            {
                UnityMesh.triangles = Z.UpdateFaceMeshTrianglesForUnity(Z.FaceMeshIndices(FaceMeshPtr.Value));
                UnityMesh.uv = Z.UpdateFaceMeshUVsForUnity(Z.FaceMeshUvs(FaceMeshPtr.Value));
                HaveInitialisedFaceMesh = true;
            }
        }

        void Update()
        {
            if (!m_hasInitialised || Z.FaceTrackerAnchorCount(m_faceTrackingTargetPipeline) <= m_faceTrackingTargetId)
                return;

            UpdateMeshData();
        }

        void OnDestroy()
        {
            ZapparFaceTrackingManager.DeRegisterPipelineCallback(OnFaceTrackerPipelineInitialised);
            if (FaceMeshPtr != null && Application.isPlaying)
                Z.FaceMeshDestroy(FaceMeshPtr.Value);

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
            m_faceVertices = null;
            m_faceNormals = null;
            HaveInitialisedFaceMesh = false;
        }
    }
}