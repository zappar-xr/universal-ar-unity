using System;
using UnityEngine;

namespace Zappar
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    internal class ZapparFaceMeshTarget : ZapparFaceMesh
    {
        public Material faceMaterial;

        void Start()
        {
            InitCoeffs();

            if (ZapparCamera.Instance != null)
                ZapparCamera.Instance.RegisterCameraListener(this);

            if (!Application.isPlaying && m_faceMesh == IntPtr.Zero)
            {
                //Create new face model
                m_faceMesh = Z.FaceMeshCreate();
                CreateMesh();
            }
        }

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