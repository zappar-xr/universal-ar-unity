using System;
using UnityEngine;

namespace Zappar
{
    public class ZapparInstantTrackingTarget : ZapparTrackingTarget, ICameraListener
    {
        public IntPtr? InstantTracker = null;
        [SerializeField, Tooltip("Offset for anchor in camera view before the placement")]
        private Vector3 m_anchorOffsetFromCamera = new Vector3(0, 0, -3);
        [SerializeField,Tooltip("Accept touch event to place the anchor for tracking")]
        private bool m_placeOnTouch = true;
        [Tooltip("Move the anchor along z-direction before the placement")]
        public bool MoveAnchorOnZ = false;
        [HideInInspector, SerializeField]
        private ZapparCamera m_zCamera;
        [HideInInspector, SerializeField]
        private float m_minZDistance = 3.0f;
        [HideInInspector, SerializeField]
        private float m_maxZDistance = 40.0f;

        private const float m_maxCameraRot = 40.0f;
        
        private bool m_hasInitialized = false;
        private bool m_isMirrored = false;
        private bool m_isPaused = false;
        public bool UserHasPlaced { get; private set; }
        
        private void Start()
        {
            if (ZapparCamera.Instance != null)
                ZapparCamera.Instance.RegisterCameraListener(this, true);

            if (ZapparCamera.Instance.CameraSourceInitialized && !m_hasInitialized)
            {
                OnMirroringUpdate(ZapparCamera.Instance.MirrorCamera);
                OnZapparCameraPaused(ZapparCamera.Instance.CameraSourcePaused);
                OnZapparInitialized(ZapparCamera.Instance.GetPipeline);
            }
        }

        public void OnZapparInitialized(IntPtr pipeline)
        {
            InstantTracker = Z.InstantWorldTrackerCreate(pipeline);
            m_hasInitialized = true;
        }

        public void OnZapparCameraPaused(bool pause) { m_isPaused = pause; }

        public void OnMirroringUpdate(bool mirrored)
        {
            m_isMirrored = mirrored;
        }

        void UpdateTargetPose()
        {
            Matrix4x4 cameraPose = ZapparCamera.Instance.CameraPose;
            Matrix4x4 instantTrackerPose = Z.InstantWorldTrackerAnchorPose(InstantTracker.Value, cameraPose, m_isMirrored);
            Matrix4x4 targetPose = Z.ConvertToUnityPose(instantTrackerPose);

            transform.localPosition = Z.GetPosition(targetPose);
            transform.localRotation = Z.GetRotation(targetPose);
            transform.localScale = Z.GetScale(targetPose);
        }

        private void Update()
        {
            if (!m_hasInitialized || InstantTracker==null || m_isPaused)
            {
                return;
            }

            if (!UserHasPlaced)
            {
                if (MoveAnchorOnZ && m_zCamera != null)
                {
                    if (m_zCamera.AnchorOrigin != null && m_zCamera.transform.rotation.eulerAngles.x < m_maxCameraRot)
                    {
                        float dist = Mathf.Lerp(m_maxZDistance, m_minZDistance, m_zCamera.transform.rotation.eulerAngles.x / m_maxCameraRot);
                        Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(InstantTracker.Value, m_anchorOffsetFromCamera.x, m_anchorOffsetFromCamera.y, -1f * dist, Z.InstantTrackerTransformOrientation.MINUS_Z_AWAY_FROM_USER);
                    }
                    else if (m_zCamera.AnchorOrigin == null)
                    {
                        if (transform.rotation == Quaternion.identity)
                        {
                            float dist = m_maxZDistance;
                            Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(InstantTracker.Value, m_anchorOffsetFromCamera.x, m_anchorOffsetFromCamera.y, -1f * dist, Z.InstantTrackerTransformOrientation.MINUS_Z_AWAY_FROM_USER);
                        }
                        else if(transform.rotation.eulerAngles.x > 360f - m_maxCameraRot)
                        {
                            float dist = Mathf.Lerp(m_maxZDistance, m_minZDistance, (360f - transform.rotation.eulerAngles.x) / m_maxCameraRot);
                            Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(InstantTracker.Value, m_anchorOffsetFromCamera.x, m_anchorOffsetFromCamera.y, -1f * dist, Z.InstantTrackerTransformOrientation.MINUS_Z_AWAY_FROM_USER);
                        }
                    }
                }
                else if(!MoveAnchorOnZ)
                {
                    Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(InstantTracker.Value, m_anchorOffsetFromCamera.x, m_anchorOffsetFromCamera.y, m_anchorOffsetFromCamera.z, Z.InstantTrackerTransformOrientation.MINUS_Z_AWAY_FROM_USER);
                }
            }
            
            if (m_placeOnTouch && Input.touchCount > 0)
            {
                UserHasPlaced = true;
            }

            UpdateTargetPose();
        }

        private void OnDestroy()
        {
            if (m_hasInitialized)
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