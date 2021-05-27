using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;

using Zappar;

/**
    ZapparCamera is a wrapper around a single Pipeline.
*/
public class ZapparCamera : MonoBehaviour
{
    private static ZapparCamera sInstance;
    public static ZapparCamera Instance
    {
        get { return sInstance; }
    }

    void Awake()
    {
        if (sInstance == null)
        {
            sInstance = this;
        }
#if UNITY_ANDROID
        Z.AndroidApplicationContextSet();
#endif
    }

    public interface ICameraListener
    {
        void OnZapparInitialised(IntPtr pipeline);
        void OnMirroringUpdate(bool mirrored);
    }

    private List<ICameraListener> listeners = new List<ICameraListener>();
    public void RegisterCameraListener(ICameraListener listener)
    {
        listeners.Add(listener);
    }

    public ZapparTrackingTarget anchorOrigin;

    private IntPtr m_camera = IntPtr.Zero;
    private IntPtr m_pipeline = IntPtr.Zero;

    private bool m_hasInitialised = false;
    private bool m_permissionIsGranted = false;
    private bool m_cameraHasStarted = false;

    private Matrix4x4 m_cameraPose;

    // --------
    public bool useFrontFacingCamera;
    public bool cameraAttitudeFromGyro;

    public bool mirrorRearCameras = false;
    public bool mirrorUserCameras = true;
    private bool m_isMirrored;

    #if UNITY_EDITOR
    [CameraSourcesListPopup()]
    [Tooltip("Select camera to use when in Play mode.")]
#endif
    public string EditorCamera;
    // --------

    void Start()
    {

        m_cameraPose = Matrix4x4.identity;

        Z.Initialize();
    }

    void UpdatePose()
    {
        if (anchorOrigin == null)
        {
            if (cameraAttitudeFromGyro)
                m_cameraPose = Z.PipelineCameraPoseWithAttitude(m_pipeline, useFrontFacingCamera);
            else
                m_cameraPose = Z.PipelineCameraPoseDefault(m_pipeline);
        }
        else
        {
            Matrix4x4 anchorPose = anchorOrigin.AnchorPoseCameraRelative();
            m_cameraPose = Z.PipelineCameraPoseWithOrigin(m_pipeline, anchorPose);
        }

        Matrix4x4 cameraPoseUnity = Z.ConvertToUnityPose(m_cameraPose);
        transform.localPosition = Z.GetPosition(cameraPoseUnity);
        transform.localRotation = Z.GetRotation(cameraPoseUnity);
        transform.localScale = Z.GetScale(cameraPoseUnity);
    }

    void Update()
    {
        // If we haven't yet initialised the Zappar library.
        if (!m_hasInitialised)
        {
            if (Z.HasInitialized())
            {
                m_pipeline = Z.PipelineCreate();
                if (m_pipeline != IntPtr.Zero)
                {
                    String cameraID = null;
                    #if UNITY_EDITOR
                    cameraID = CameraSourcesListPopupDrawer.IdFromName(EditorCamera);
                    #endif
                    if (cameraID == null) cameraID = Z.CameraDefaultDeviceId(useFrontFacingCamera);
                    m_camera = Z.CameraSourceCreate(m_pipeline, cameraID);

                    Z.PermissionRequestUi();

                    m_isMirrored = (useFrontFacingCamera && mirrorUserCameras) || (!useFrontFacingCamera && mirrorRearCameras);

                    // The Zappar library has been initialised and _this_ pipeline is created, so it is safe for 
                    // any listeners to now initialise.
                    foreach (ICameraListener listener in listeners)
                    {
                        listener.OnZapparInitialised(m_pipeline);
                        listener.OnMirroringUpdate(m_isMirrored);
                    }

                    m_hasInitialised = true;
                }
            }
        }
        else
        {
            // library is initialised but camera hasn't started
            if (!m_cameraHasStarted)
            {
                if (!m_permissionIsGranted)
                {
                    // Zappar library is initialised, permissions have now been requested
                    m_permissionIsGranted = Z.PermissionGrantedAll();
                }
                else
                {
                    // Permissions have been granted but camera hasn't started
                    Z.PipelineGLContextSet(m_pipeline);
                    Z.CameraSourceStart(m_camera);
                    m_cameraHasStarted = true;
                }
            }

            // initialised, permissions granted, and camera started
            else
            {
                Z.Process(m_pipeline);
                Z.PipelineFrameUpdate(m_pipeline);
                Z.CameraFrameUpload(m_pipeline);
                
                UpdatePose();

                GetComponent<Camera>().projectionMatrix = Z.PipelineProjectionMatrix(m_pipeline, Screen.width, Screen.height);
            }
        }
    }

    void OnDestroy()
    {
        if (m_pipeline != IntPtr.Zero) Z.PipelineDestroy(m_pipeline);
        if (m_camera != IntPtr.Zero) {
            Z.CameraSourcePause(m_camera);
            Z.CameraSourceDestroy(m_camera);
        }
    }

    public Matrix4x4 GetPose()
    {
        return m_cameraPose;
    }

    public bool IsMirrored()
    {
        return m_isMirrored;
    }

    public IntPtr GetPipeline()
    {
        return m_pipeline;
    }

}
