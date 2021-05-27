using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using Zappar;

public class ZapparFaceLandmark : MonoBehaviour, ZapparCamera.ICameraListener
{
    public enum Face_Landmark_Name {
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
    
    public ZapparFaceTrackingTarget faceTracker;
    public Face_Landmark_Name landmark;
    
    private Face_Landmark_Name m_currentLandmark;
    private bool m_isMirrored;
    private IntPtr m_faceTrackerPtr;
    private IntPtr m_faceLandmarkPtr = IntPtr.Zero;

    // Start is called before the first frame update
    void Start()
    {
        if (ZapparCamera.Instance != null)
            ZapparCamera.Instance.RegisterCameraListener( this );
    }

    // Update is called once per frame
    void Update()
    {
        if (m_faceLandmarkPtr == IntPtr.Zero) return;
        
        if(landmark != m_currentLandmark)
            InitFaceLandmark();
        
        var identity = Z.FaceTrackerAnchorIdentityCoefficients(m_faceTrackerPtr, 0);
        var expression = Z.FaceTrackerAnchorExpressionCoefficients(m_faceTrackerPtr, 0);
        Z.FaceLandmarkUpdate(m_faceLandmarkPtr, identity, expression, m_isMirrored);
        
        var matrix = Z.ConvertToUnityPose(Z.FaceLandmarkAnchorPose(m_faceLandmarkPtr));
        transform.localPosition = Z.GetPosition(matrix);
        transform.localRotation = Z.GetRotation(matrix);
    }
    
    public void OnZapparInitialised(IntPtr pipeline) 
    {
        if (faceTracker is null)
        {
            Debug.Log("Warning: The face landmark will not work unless a Face Tracker is assigned.");
            return;
        }
        
        InitFaceLandmark();
        m_faceTrackerPtr = faceTracker.m_faceTracker;
    }

    public void OnMirroringUpdate(bool mirrored)
    {
        m_isMirrored = mirrored;
    }

    void OnDestroy()
    {
        if(m_faceLandmarkPtr != IntPtr.Zero)
            Z.FaceLandmarkDestroy(m_faceLandmarkPtr);
    }

    void InitFaceLandmark()
    {
        if(m_faceLandmarkPtr != IntPtr.Zero)
            Z.FaceLandmarkDestroy(m_faceLandmarkPtr);
        m_faceLandmarkPtr = Z.FaceLandmarkCreate((uint)landmark);
        m_currentLandmark = landmark;
    }
}