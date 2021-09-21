using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Zappar
{
    /// Wrapper around a single Pipeline example
    [RequireComponent(typeof(Camera))]
    public class ZapparCamera : MonoBehaviour
    {
        public interface ICameraListener
        {
            void OnZapparInitialised(IntPtr pipeline);
            void OnMirroringUpdate(bool mirrored);
        }

        private static ZapparCamera sInstance;
        public static ZapparCamera Instance => sInstance;

        public bool CameraHasStarted => m_cameraHasStarted;

        public ZapparTrackingTarget anchorOrigin;
        // --------
        public bool useFrontFacingCamera;
        public bool cameraAttitudeFromGyro;

        public bool mirrorRearCameras = false;
        public bool mirrorUserCameras = true;

        [CameraSourcesListPopup]
        [Tooltip("Select camera to use when in Play mode.")]
        public string EditorCamera;
        // --------

        private IntPtr m_camera = IntPtr.Zero;
        private IntPtr m_pipeline = IntPtr.Zero;

        private bool m_hasInitialised = false;
        private static bool m_permissionIsGranted = false;
        private bool m_cameraHasStarted = false;
        private bool m_isMirrored;

        private Matrix4x4 m_cameraPose;
        private Camera m_unityCamera = null;
        private float[] m_cameraModel = null;
        private List<ICameraListener> listeners = new List<ICameraListener>();

        #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern bool ZapparIsVisible();
        private bool m_isVisibilityPaused = false;
        #endif
        
        #region unity_methods

        void Awake()
        {
            if (sInstance == null)
            {
                sInstance = this;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
        Z.AndroidApplicationContextSet();
#endif
            m_cameraModel = new float[]{ 0, 0, 0, 0, 0, 0 };
        }

        void Start()
        {
            m_unityCamera = GetComponent<Camera>();

#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
            var log = LogManager.Instance;
#endif
            m_cameraPose = Matrix4x4.identity;

            Z.Initialize();
        }

        void Update()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            if (ZapparIsVisible()) {
                if (m_cameraHasStarted && m_isVisibilityPaused) {
                    Z.CameraSourceStart(m_camera);
                    m_isVisibilityPaused = false;
                }
            } else if (m_cameraHasStarted && !m_isVisibilityPaused) {
                Z.CameraSourcePause(m_camera);
                m_isVisibilityPaused = true;
            }
            #endif

            // If we haven't yet initialised the Zappar library.
            if (!m_hasInitialised)
            {
                if (Z.HasInitialized())
                {
                    m_pipeline = Z.PipelineCreate();
                    if (m_pipeline != IntPtr.Zero)
                    {
                        string cameraID = "";
#if UNITY_EDITOR
                        cameraID = IdFromName(EditorCamera);
#endif
                        if (string.IsNullOrEmpty(cameraID))
                            cameraID = Z.CameraDefaultDeviceId(useFrontFacingCamera);

                        m_camera = Z.CameraSourceCreate(m_pipeline, cameraID);

                        if (!m_permissionIsGranted)
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

                    m_unityCamera.projectionMatrix = Z.PipelineProjectionMatrix(m_pipeline, Screen.width, Screen.height, m_unityCamera.nearClipPlane, m_unityCamera.farClipPlane, ref m_cameraModel);
                }
            }
        }

        void OnDestroy()
        {
            if (m_pipeline != IntPtr.Zero) Z.PipelineDestroy(m_pipeline);
            if (m_camera != IntPtr.Zero)
            {
                Z.CameraSourcePause(m_camera);
                Z.CameraSourceDestroy(m_camera);
            }
        }
        
        #endregion

        public void RegisterCameraListener(ICameraListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        private void UpdatePose()
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

        public static string IdFromName(string name)
        {
            try
            {
                for (int i = 0; i < Z.CameraCount(); i++)
                {
                    string sn = Z.CameraName(i) + " (" + Z.CameraId(i) + ")";
                    if (name.Equals(Z.CameraName(i)) || name.Equals(sn))
                    {
                        return Z.CameraId(i);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to check camera list: " + e.Message);
            }
            
            return null;
        }

        public Matrix4x4 GetPose()
        {
            return m_cameraPose;
        }

        public bool IsMirrored => m_isMirrored;

        public IntPtr GetPipeline => m_pipeline;

    }
}