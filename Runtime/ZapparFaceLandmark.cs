using System;
using UnityEngine;
using Zappar;

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

        public ZapparFaceTrackingTarget FaceTracker;

        public Face_Landmark_Name LandmarkName;

        private Face_Landmark_Name m_currentLandmark;
        private bool m_isMirrored;
        private IntPtr? m_faceTrackerPipeline = null;
        private int m_faceTrackerId;
        private IntPtr? m_faceLandmarkPtr = null;

        private const int NumIdentityCoefficients = 50;
        private const int NumExpressionCoefficients = 29;

        void Start()
        {
            if (FaceTracker == null)
            {
                FaceTracker = transform.GetComponentInParent<ZapparFaceTrackingTarget>();
            }
            ZapparFaceTrackingManager.RegisterPipelineCallback(OnFaceTrackingPipelineInitialised);
        }

        void Update()
        {
            if (m_faceLandmarkPtr == null) return;

            if (LandmarkName != m_currentLandmark)
                InitFaceLandmark();

            Z.FaceLandmarkUpdate(m_faceLandmarkPtr.Value, FaceTracker.Identity, FaceTracker.Expression, m_isMirrored);

            var matrix = Z.ConvertToUnityPose(Z.FaceLandmarkAnchorPose(m_faceLandmarkPtr.Value));
            transform.localPosition = Z.GetPosition(matrix);
            transform.localRotation = Z.GetRotation(matrix);
        }

        void OnDestroy()
        {
            if (m_faceLandmarkPtr != null)
                Z.FaceLandmarkDestroy(m_faceLandmarkPtr.Value);
            m_faceLandmarkPtr = null;
            ZapparFaceTrackingManager.DeRegisterPipelineCallback(OnFaceTrackingPipelineInitialised);
        }

        public void OnFaceTrackingPipelineInitialised(IntPtr pipeline, bool mirrored)
        {
            if (FaceTracker == null)
            {
                Debug.LogError("The face landmark will not work unless a Face Tracker is assigned.");
                return;
            }

            InitFaceLandmark();
            m_faceTrackerPipeline = pipeline;
            m_faceTrackerId = FaceTracker.FaceTrackingId;
            m_isMirrored = mirrored;
        }

        void InitFaceLandmark()
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