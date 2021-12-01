using System;
using UnityEngine;

namespace Zappar
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ZapparFaceMeshTarget : ZapparFaceMesh
    {
        public Material FaceMaterial;
        public ZapparFaceTrackingTarget FaceTrackingTarget;

        private void Start()
        {
            InitFaceMeshOnStart();
        }

        public override ZapparFaceTrackingTarget GetFaceTrackingTarget()
        {
            return (FaceTrackingTarget == null) ? GetComponentInParent<ZapparFaceTrackingTarget>() : FaceTrackingTarget;
        }

        public override void UpdateMaterial()
        {
            if (FaceMaterial != null)
                gameObject.GetComponent<MeshRenderer>().sharedMaterial = FaceMaterial;
        }
    }
}