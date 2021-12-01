using System;
using UnityEngine;

namespace Zappar
{
    public class ZapparInstantTrackingTarget : ZapparTrackingTarget, ZapparCamera.ICameraListener
    {
        public IntPtr? InstantTracker = null;
        [SerializeField, Tooltip("Offset the tracker in camera view when user hasn't placed the anchor")]
        private Vector3 m_anchorOffsetFromCamera = new Vector3(0, 0, -5);
        [SerializeField,Tooltip("Wait for touch event to place anchor for tracking")]
        private bool m_placeOnTouch = true;
        private bool m_hasInitialised = false;
        private bool m_isMirrored = false;
        public bool UserHasPlaced { get; private set; }

        void Start()
        {
            if (ZapparCamera.Instance != null)
                ZapparCamera.Instance.RegisterCameraListener(this, true);
        }

        public void OnZapparInitialised(IntPtr pipeline)
        {
            InstantTracker = Z.InstantWorldTrackerCreate(pipeline);
            m_hasInitialised = true;
        }

        public void OnMirroringUpdate(bool mirrored)
        {
            m_isMirrored = mirrored;
        }

        void UpdateTargetPose()
        {
            Matrix4x4 cameraPose = ZapparCamera.Instance.GetCameraPose;
            Matrix4x4 instantTrackerPose = Z.InstantWorldTrackerAnchorPose(InstantTracker.Value, cameraPose, m_isMirrored);
            Matrix4x4 targetPose = Z.ConvertToUnityPose(instantTrackerPose);

            transform.localPosition = Z.GetPosition(targetPose);
            transform.localRotation = Z.GetRotation(targetPose);
            transform.localScale = Z.GetScale(targetPose);
        }

        void Update()
        {
            if (!m_hasInitialised || InstantTracker==null)
            {
                return;
            }

            if (!UserHasPlaced)
            {
                Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(InstantTracker.Value, m_anchorOffsetFromCamera.x, m_anchorOffsetFromCamera.y, m_anchorOffsetFromCamera.z, Z.InstantTrackerTransformOrientation.MINUS_Z_AWAY_FROM_USER);
            }
            
            if (m_placeOnTouch && Input.touchCount > 0)
            {
                UserHasPlaced = true;
            }

            UpdateTargetPose();
        }

        void OnDestroy()
        {
            if (m_hasInitialised)
            {
                if (InstantTracker != null)
                {
                    Z.InstantWorldTrackerDestroy(InstantTracker.Value);
                    InstantTracker = null;
                }
            }
            if (ZapparCamera.Instance != null)
                ZapparCamera.Instance.RegisterCameraListener(this, false);
        }

        public override Matrix4x4 AnchorPoseCameraRelative()
        {
            return Z.InstantWorldTrackerAnchorPoseCameraRelative(InstantTracker.Value, m_isMirrored);
        }
    
        public void PlaceTrackerAnchor()
        {
            UserHasPlaced = true;
        }

        public void ResetTrackerAnchor()
        {
            UserHasPlaced = false;
        }
    }
}