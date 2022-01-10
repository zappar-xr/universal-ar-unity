using System;
using UnityEngine;

namespace Zappar
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ZapparFaceMeshTarget : ZapparFaceMesh
    {
        [Tooltip("The face material this mesh should use.")]
        public Material FaceMaterial;
        [Tooltip("Face tracking anchor that this landmark should use. Also parent this object under the respective anchor for correct pose update.")]
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