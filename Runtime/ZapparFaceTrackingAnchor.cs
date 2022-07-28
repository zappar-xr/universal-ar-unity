using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Zappar
{
    public class ZapparFaceTrackingAnchor : ZapparTrackingTarget
    {
        [HideInInspector, SerializeField]
        public ZapparMultiFaceTrackingTarget FaceTrackingTarget = null;

        public UnityEvent OnSeenEvent;
        public UnityEvent OnNotSeenEvent;

        public delegate void FaceTrackerInitialized(IntPtr faceTrackingPipeline, bool isMirrored);
        private readonly List<FaceTrackerInitialized> m_initListeners = new List<FaceTrackerInitialized>();

        [HideInInspector]
        public int AnchorId = 0;

        public float[] Identity => m_identity;
        public float[] Expression => m_expression;

        public const int NumIdentityCoefficients = 50;
        public const int NumExpressionCoefficients = 29;

        private float[] m_identity = new float[NumIdentityCoefficients];
        private float[] m_expression = new float[NumExpressionCoefficients];

        [HideInInspector, SerializeField]
        private int m_faceNumber = 0;

        private bool m_isVisible = false;

        public int FaceTrackerIndex
        {
            get { return m_faceNumber; }
            set { m_faceNumber = (value < 0 ? 0 : value); }
        }

        public bool FaceIsVisible => m_isVisible;

        public void RegisterPipelineInitCallback(FaceTrackerInitialized method, bool add)
        {
            if (add && !m_initListeners.Contains(method))
            {
                m_initListeners.Add(method);
            }
            else if (!add && m_initListeners.Contains(method))
            {
                m_initListeners.Remove(method);
            }
        }

        public void InitFaceTracker()
        {
            InitCoeffs();
            foreach(var listener in m_initListeners)
            {
                listener?.Invoke(FaceTrackingTarget.FaceTrackerPipeline.Value, FaceTrackingTarget.IsMirrored);
            }
        }

        private void InitCoeffs()
        {
            m_identity = (m_identity == null || m_identity.Length < NumIdentityCoefficients) ? new float[NumIdentityCoefficients] : m_identity;
            m_expression = (m_expression == null || m_expression.Length < NumExpressionCoefficients) ? new float[NumExpressionCoefficients] : m_expression;
            for (int i = 0; i < NumIdentityCoefficients; ++i) m_identity[i] = 0.0f;
            for (int i = 0; i < NumExpressionCoefficients; ++i) m_expression[i] = 0.0f;
        }

        public void UpdateAnchor(bool isTracked)
        {
            if(isTracked)
            {
                if (!m_isVisible)
                {
                    m_isVisible = true;
                    OnSeenEvent?.Invoke();
                }
                Z.FaceTrackerAnchorUpdateIdentityCoefficients(FaceTrackingTarget.FaceTrackerPipeline.Value, AnchorId, ref m_identity);
                Z.FaceTrackerAnchorUpdateExpressionCoefficients(FaceTrackingTarget.FaceTrackerPipeline.Value, AnchorId, ref m_expression);
                UpdateAnchorPose();
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

        private void UpdateAnchorPose()
        {
            Matrix4x4 cameraPose = ZapparCamera.Instance.CameraPose;
            Matrix4x4 facePose = Z.FaceTrackerAnchorPose(FaceTrackingTarget.FaceTrackerPipeline.Value, AnchorId, cameraPose, FaceTrackingTarget.IsMirrored);
            Matrix4x4 targetPose = Z.ConvertToUnityPose(facePose);

            transform.localPosition = Z.GetPosition(targetPose);
            transform.localRotation = Z.GetRotation(targetPose);
            transform.localScale = Z.GetScale(targetPose);
        }

        public override Matrix4x4 AnchorPoseCameraRelative()
        {
            if (m_isVisible) 
                return Z.FaceTrackerAnchorPoseCameraRelative(FaceTrackingTarget.FaceTrackerPipeline.Value, AnchorId, FaceTrackingTarget.IsMirrored);
            
            return Matrix4x4.identity;
        }

    }
}