using System;
using UnityEngine;

namespace Zappar
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    internal class ZapparFaceMeshTarget : ZapparFaceMesh
    {
        public Material faceMaterial;

        private bool usingFullHead = true;

        private void Awake()
        {
            InitCoeffs();
            usingFullHead = useDefaultFullHead;
        }

        void Start()
        {
            if (ZapparCamera.Instance != null)
                ZapparCamera.Instance.RegisterCameraListener(this);

            if (!Application.isPlaying && m_faceMesh == IntPtr.Zero)
            {
                //Create new face model
                m_faceMesh = Z.FaceMeshCreate();
                CreateMesh(true);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += CheckForHeadMeshUpdate;
        }

        private void CheckForHeadMeshUpdate()
        {
            if (usingFullHead != useDefaultFullHead)
            {
                if (m_faceMesh != IntPtr.Zero)
                    Z.FaceMeshDestroy(m_faceMesh);

                DestroyUnityMesh();
                m_faceMesh = Z.FaceMeshCreate();
                CreateMesh();
                usingFullHead = useDefaultFullHead;
            }
        }
#endif

        public void OnEnable()
        {
            if (faceMaterial != null)
                gameObject.GetComponent<MeshRenderer>().sharedMaterial = faceMaterial;
            if (faceTracker == null)
                faceTracker = GetComponentInParent<ZapparFaceTrackingTarget>();
        }

        public override void UpdateMaterial()
        {
            if (faceMaterial != null)
                gameObject.GetComponent<MeshRenderer>().sharedMaterial = faceMaterial;
        }
    }
}