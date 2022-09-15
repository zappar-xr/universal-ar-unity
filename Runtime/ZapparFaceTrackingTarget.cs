using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Zappar.Obsolete
{
    internal static class ZapparFaceTrackingManager
    {
        public static int NumberOfTrackers { get; private set; } = 0;
        public static bool HasInitialized = false;
        public delegate void FaceTrackerInitialized(IntPtr faceTrackingPipeline, bool isMirrored);

        private static List<FaceTrackerInitialized> listeners = new List<FaceTrackerInitialized>();
        private static IntPtr? s_faceTrackingPipeline = null;
        public static bool IsMirrored = false;

        public static IntPtr? FaceTrackerPipeline
        {
            get { return s_faceTrackingPipeline; }
            set
            {
                if (value == null) return;
                s_faceTrackingPipeline = value;                             
                foreach (var v in listeners) v?.Invoke(value.Value, IsMirrored);
            }
        }

        public static void RegisterTracker(ZapparFaceTrackingTarget target, bool add)
        {
            if (add)
                NumberOfTrackers++;
            else 
                NumberOfTrackers--;
            if (NumberOfTrackers < 0) NumberOfTrackers = 0;
        }

        public static void RegisterPipelineCallback(FaceTrackerInitialized funcPtr)
        {
            if(!listeners.Contains(funcPtr))
            {
                listeners.Add(funcPtr);
            }
        }
        public static void DeRegisterPipelineCallback(FaceTrackerInitialized funcPtr)
        {
            listeners.Remove(funcPtr);
        }
    }

    public class ZapparFaceTrackingTarget : ZapparTrackingTarget, ICameraListener
    {
        public UnityEvent OnSeenEvent;
        public UnityEvent OnNotSeenEvent;

        [SerializeField, HideInInspector]
        private int m_faceNumber = 0;
        private bool m_hasInitialised = false;
        private bool m_isMirrored;
        private bool m_isVisible = false;

        public float[] Identity => m_identity;
        public float[] Expression => m_expression;

        private float[] m_identity = new float[NumIdentityCoefficients];
        private float[] m_expression = new float[NumExpressionCoefficients];

        public const int NumIdentityCoefficients = 50;
        public const int NumExpressionCoefficients = 29;

        public int FaceTrackingId
        {
            get { return m_faceNumber; }
            set { m_faceNumber = (value < 0 ? 0 : value); }
        }

        public void InitCoeffs()
        {
            m_identity = (m_identity == null || m_identity.Length < NumIdentityCoefficients) ? new float[NumIdentityCoefficients] : m_identity;
            m_expression = (m_expression == null || m_expression.Length < NumExpressionCoefficients) ? new float[NumExpressionCoefficients] : m_expression;
            for (int i = 0; i < NumIdentityCoefficients; ++i) m_identity[i] = 0.0f;
            for (int i = 0; i < NumExpressionCoefficients; ++i) m_expression[i] = 0.0f;
        }

        void Start()
        {
            InitCoeffs();
            ZapparFaceTrackingManager.RegisterTracker(this, true);

            if (ZapparCamera.Instance != null)
                ZapparCamera.Instance.RegisterCameraListener(this, true);
        }

        public void OnZapparInitialized(IntPtr pipeline)
        {
            if (!ZapparFaceTrackingManager.HasInitialized)
            {
                IntPtr faceTracker = Z.FaceTrackerCreate(pipeline);
                Z.FaceTrackerMaxFacesSet(faceTracker, ZapparFaceTrackingManager.NumberOfTrackers);
                
#if UNITY_EDITOR
                byte[] faceTrackerData = Z.LoadRawBytes(Z.FaceTrackingModelPath());
                Z.FaceTrackerModelLoadFromMemory(faceTracker, faceTrackerData);
#else
                Z.FaceTrackerModelLoadDefault(faceTracker);
#endif
                ZapparFaceTrackingManager.FaceTrackerPipeline = faceTracker;
                ZapparFaceTrackingManager.HasInitialized = true;
            }
            m_hasInitialised = true;
        }

        public void OnZapparCameraPaused(bool pause) { }

        public void OnMirroringUpdate(bool mirrored)
        {
            ZapparFaceTrackingManager.IsMirrored = m_isMirrored = mirrored;
        }

        void UpdateTargetPose()
        {
            Matrix4x4 cameraPose = ZapparCamera.Instance.CameraPose;
            Matrix4x4 facePose = Z.FaceTrackerAnchorPose(ZapparFaceTrackingManager.FaceTrackerPipeline.Value, m_faceNumber, cameraPose, m_isMirrored);
            Matrix4x4 targetPose = Z.ConvertToUnityPose(facePose);

            transform.localPosition = Z.GetPosition(targetPose);
            transform.localRotation = Z.GetRotation(targetPose);
            transform.localScale = Z.GetScale(targetPose);
        }

        void Update()
        {
            if (!m_hasInitialised || ZapparFaceTrackingManager.FaceTrackerPipeline == null)
            {
                return;
            }
            if (Z.FaceTrackerAnchorCount(ZapparFaceTrackingManager.FaceTrackerPipeline.Value) > m_faceNumber)
            {
                if (!m_isVisible)
                {
                    m_isVisible = true;
                    OnSeenEvent?.Invoke();
                }
                Z.FaceTrackerAnchorUpdateIdentityCoefficients(ZapparFaceTrackingManager.FaceTrackerPipeline.Value, m_faceNumber, ref m_identity);
                Z.FaceTrackerAnchorUpdateExpressionCoefficients(ZapparFaceTrackingManager.FaceTrackerPipeline.Value, m_faceNumber, ref m_expression);
                UpdateTargetPose();
            }
            else
            {
                if (m_isVisible)
                {
                    m_isVisible = false;
                    OnNotSeenEvent?.Invoke();
                }
            }
        }

        void OnDestroy()
        {
            if (m_hasInitialised && m_faceNumber==0)
            {
                //Destroy face tracking pipeline while destroying last tracker
                if (ZapparFaceTrackingManager.FaceTrackerPipeline != null)
                {
                    Z.FaceTrackerDestroy(ZapparFaceTrackingManager.FaceTrackerPipeline.Value);
                    ZapparFaceTrackingManager.FaceTrackerPipeline = null;
                }
                ZapparFaceTrackingManager.HasInitialized = false;
            }

            m_hasInitialised = false;

            ZapparFaceTrackingManager.RegisterTracker(this, false);

            if (ZapparCamera.Instance != null)
                ZapparCamera.Instance.RegisterCameraListener(this, false);
        }

        public override Matrix4x4 AnchorPoseCameraRelative()
        {
            if (Z.FaceTrackerAnchorCount(ZapparFaceTrackingManager.FaceTrackerPipeline.Value) > m_faceNumber)
            {
                return Z.FaceTrackerAnchorPoseCameraRelative(ZapparFaceTrackingManager.FaceTrackerPipeline.Value, m_faceNumber, m_isMirrored);
            }
            return Matrix4x4.identity;
        }
    }
}