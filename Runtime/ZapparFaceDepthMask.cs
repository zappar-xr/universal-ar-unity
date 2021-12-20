using System;
using UnityEngine;

namespace Zappar
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ZapparFaceDepthMask : ZapparFaceMesh
    {
        public Material FaceMaterial;
        public ZapparFaceTrackingAnchor FaceTrackingAnchor;

        private void Start()
        {
            InitFaceMeshOnStart();
        }

        public override ZapparFaceTrackingAnchor GetFaceTrackingAnchor()
        {
            return (FaceTrackingAnchor == null) ? GetComponentInParent<ZapparFaceTrackingAnchor>() : FaceTrackingAnchor;
        }

        public override void UpdateMaterial()
        {
            if (FaceMaterial != null)
                gameObject.GetComponent<MeshRenderer>().sharedMaterial = FaceMaterial;
        }
    }
}