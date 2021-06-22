using System;
using UnityEngine;

namespace Zappar
{
    public class ZapparInstantTrackingTarget : ZapparTrackingTarget, ZapparCamera.ICameraListener
    {
        private IntPtr m_instantTracker = IntPtr.Zero;
        private bool m_userHasPlaced = false;
        private bool m_hasInitialised = false;
        private bool m_isMirrored = false;

        void Start()
        {
            ZapparCamera.Instance.RegisterCameraListener(this);
        }

        public void OnZapparInitialised(IntPtr pipeline)
        {
            m_instantTracker = Z.InstantWorldTrackerCreate(pipeline);
            m_hasInitialised = true;
        }

        public void OnMirroringUpdate(bool mirrored)
        {
            m_isMirrored = mirrored;
        }

        void UpdateTargetPose()
        {
            Matrix4x4 cameraPose = ZapparCamera.Instance.GetPose();
            Matrix4x4 instantTrackerPose = Z.InstantWorldTrackerAnchorPose(m_instantTracker, cameraPose, m_isMirrored);
            Matrix4x4 targetPose = Z.ConvertToUnityPose(instantTrackerPose);

            transform.localPosition = Z.GetPosition(targetPose);
            transform.localRotation = Z.GetRotation(targetPose);
            transform.localScale = Z.GetScale(targetPose);
        }

        void Update()
        {
            if (!m_hasInitialised)
            {
                return;
            }

            if (!m_userHasPlaced)
            {
                Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(m_instantTracker, 0, 0, -5, Z.InstantTrackerTransformOrientation.MINUS_Z_AWAY_FROM_USER);
            }

            if (Input.touchCount > 0)
            {
                m_userHasPlaced = true;
            }

            UpdateTargetPose();
        }

        void OnDestroy()
        {
            if (m_hasInitialised)
            {
                if (m_instantTracker != IntPtr.Zero) Z.InstantWorldTrackerDestroy(m_instantTracker);
            }
        }

        public override Matrix4x4 AnchorPoseCameraRelative()
        {
            return Z.InstantWorldTrackerAnchorPoseCameraRelative(m_instantTracker, m_isMirrored);
        }
    }
}