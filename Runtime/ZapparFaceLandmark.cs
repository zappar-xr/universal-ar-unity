using System;
using UnityEngine;

namespace Zappar
{
    public class ZapparFaceLandmark : MonoBehaviour
    {
        public enum Face_Landmark_Name
        {
            LeftEye = 0,
            RightEye,
            LeftEar,
            RightEar,
            NoseBridge,
            NoseTip,
            NoseBase,
            LipTop,
            LipBottom,
            MouthCenter,
            Chin,
            LeftEyebrow,
            RightEyebrow
        };

        [Tooltip("Face tracking anchor that this landmark should use. Also parent this object under the respective anchor for correct pose update.")]
        public ZapparFaceTrackingAnchor FaceTrackingAnchor;

        public Face_Landmark_Name LandmarkName;

        private Face_Landmark_Name m_currentLandmark;
        private bool m_isMirrored;
        private IntPtr? m_faceLandmarkPtr = null;

        private void Start()
        {
            if (FaceTrackingAnchor == null)
            {
                FaceTrackingAnchor = transform.GetComponentInParent<ZapparFaceTrackingAnchor>();
                if (FaceTrackingAnchor == null)
                {
                    Debug.LogError("Missing face tracking anchor reference!");
                    gameObject.SetActive(false);
                    return;
                }
            }

            FaceTrackingAnchor.RegisterPipelineInitCallback(OnFaceTrackingPipelineInitialised, true);

            if (FaceTrackingAnchor.FaceTrackingTarget.HasInitialized && m_faceLandmarkPtr == null)
                OnFaceTrackingPipelineInitialised(FaceTrackingAnchor.FaceTrackingTarget.FaceTrackerPipeline.Value, FaceTrackingAnchor.FaceTrackingTarget.IsMirrored);
        }

        private void Update()
        {
            if (m_faceLandmarkPtr == null || !FaceTrackingAnchor.FaceIsVisible) return;

            if (LandmarkName != m_currentLandmark)
                InitFaceLandmark();

            Z.FaceLandmarkUpdate(m_faceLandmarkPtr.Value, FaceTrackingAnchor.Identity, FaceTrackingAnchor.Expression, m_isMirrored);

            var matrix = Z.ConvertToUnityPose(Z.FaceLandmarkAnchorPose(m_faceLandmarkPtr.Value));
            transform.localPosition = Z.GetPosition(matrix);
            transform.localRotation = Z.GetRotation(matrix);
        }

        private void OnDestroy()
        {
            if (m_faceLandmarkPtr != null)
                Z.FaceLandmarkDestroy(m_faceLandmarkPtr.Value);
            m_faceLandmarkPtr = null;
            FaceTrackingAnchor.RegisterPipelineInitCallback(OnFaceTrackingPipelineInitialised, false);
        }

        public void OnFaceTrackingPipelineInitialised(IntPtr pipeline, bool mirrored)
        {
            if (FaceTrackingAnchor == null)
            {
                Debug.LogError("The face landmark will not work unless a Face Tracker is assigned.");
                return;
            }

            InitFaceLandmark();
            m_isMirrored = mirrored;
        }

        private void InitFaceLandmark()
        {
            if (m_faceLandmarkPtr != null)
            {
                Z.FaceLandmarkDestroy(m_faceLandmarkPtr.Value);
            }
            m_faceLandmarkPtr = Z.FaceLandmarkCreate((uint)LandmarkName);
            m_currentLandmark = LandmarkName;
        }
    }
}