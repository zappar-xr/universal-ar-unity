
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

using Zappar;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public class ZapparImageTrackingTarget : ZapparTrackingTarget, ZapparCamera.ICameraListener
{
    private bool m_isMirrored;

    private IntPtr m_imageTracker = IntPtr.Zero;
    private bool m_hasInitialised = false;

#if UNITY_EDITOR
    [TargetFileListPopup()]
    [Tooltip("Select the ZPT file you would like to track.")]
#endif
    public string Target;

    private enum Orientation
    {
        Flat,
        Upright
    }
    [Tooltip("During play offset the tracker's rotation accordingly")]
    [SerializeField]
    private Orientation orientation = Orientation.Flat;

    public UnityEvent m_OnSeenEvent;
    public UnityEvent m_OnNotSeenEvent;
    private bool m_isVisible = false;

    void OnValidate()
    {
#if UNITY_EDITOR
        GameObject previewImage = GameObject.Find("Preview Image");
        if(previewImage != null) 
            previewImage.transform.eulerAngles = orientation == Orientation.Flat ? new Vector3(0, 0, 0) : new Vector3(-90,0,0);

        string currentFilename = GetComponent<ZapparEditModeImageTarget>().currentFilename;
        if (currentFilename != Target)
            GetComponent<ZapparEditModeImageTarget>().OnZPTFilenameChange();
#endif
    }

    void Start()
    {
        if (m_OnSeenEvent == null)
            m_OnSeenEvent = new UnityEvent();

        if (m_OnNotSeenEvent == null)
            m_OnNotSeenEvent = new UnityEvent();

        if (ZapparCamera.Instance != null)
            ZapparCamera.Instance.RegisterCameraListener(this);
    }

    public void OnZapparInitialised(IntPtr pipeline)
    {
        if (!gameObject.activeInHierarchy){
            Debug.Log("Could not start LoadZPTTarget Coroutine as gameobject is inactive.");
            return;
        }
        m_hasInitialised = true;
        m_imageTracker = Z.ImageTrackerCreate(pipeline);

        string filename = Target;
        StartCoroutine(Z.LoadZPTTarget(filename, TargetDataAvailableCallback));
    }

    public void OnMirroringUpdate(bool mirrored)
    {
        m_isMirrored = mirrored;
    }

    void UpdateTargetPose()
    {
        Matrix4x4 cameraPose = ZapparCamera.Instance.GetPose();
        Matrix4x4 imagePose = Z.ImageTrackerAnchorPose(m_imageTracker, 0, cameraPose, m_isMirrored);
        Matrix4x4 targetPose = Z.ConvertToUnityPose(imagePose);
        transform.localPosition = Z.GetPosition(targetPose);

        // Offset rotations based on dropdown provided by inspector properties
        Quaternion rotation = orientation == Orientation.Flat ? Z.GetRotation(targetPose) * Quaternion.Euler(Vector3.left * 90) : Z.GetRotation(targetPose);
        transform.localRotation = rotation;

        transform.localScale = Z.GetScale(targetPose);
    }

    void Update()
    {
        if (!m_hasInitialised)
        {
            return;
        }

        if (Z.ImageTrackerAnchorCount(m_imageTracker) > 0)
        {
            if (!m_isVisible)
            {
                m_isVisible = true;
                m_OnSeenEvent.Invoke();
            }
            UpdateTargetPose();
        }
        else
        {
            if(m_isVisible)
            {
                m_isVisible = false;
                m_OnNotSeenEvent.Invoke();
            }
        }
    }

    private void TargetDataAvailableCallback(byte[] data)
    {
        Z.ImageTrackerTargetLoadFromMemory(m_imageTracker, data);
    }

    void OnDestroy()
    {
        if (m_hasInitialised)
        {
            if (m_imageTracker != IntPtr.Zero) Z.ImageTrackerDestroy(m_imageTracker);
        }
    }

    public override Matrix4x4 AnchorPoseCameraRelative()
    {
        if (Z.ImageTrackerAnchorCount(m_imageTracker) > 0)
        {
            return Z.ImageTrackerAnchorPoseCameraRelative(m_imageTracker, 0, m_isMirrored);
        }
        return Matrix4x4.identity;
    }

}
