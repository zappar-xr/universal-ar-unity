using System;
using UnityEngine;
using System.Collections.Generic;

namespace Zappar
{
    /// Wrapper around a single Pipeline example
    public class ZapparCamera : MonoBehaviour
    {
        private static ZapparCamera sInstance;
        public static ZapparCamera Instance
        {
            get { return sInstance; }
        }

        public interface ICameraListener
        {
            void OnZapparInitialised(IntPtr pipeline);
            void OnMirroringUpdate(bool mirrored);
        }

        private List<ICameraListener> listeners = new List<ICameraListener>();
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

        [CameraSourcesListPopup]
        [Tooltip("Select camera to use when in Play mode.")]
        public string EditorCamera;
        // --------

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
        }

        void Start()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
            var log = LogManager.Instance;
#endif
            m_cameraPose = Matrix4x4.identity;

            Z.Initialize();
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
                        string cameraID = "";
#if UNITY_EDITOR
                        cameraID = IdFromName(EditorCamera);
#endif
                        if (string.IsNullOrEmpty(cameraID))
                            cameraID = Z.CameraDefaultDeviceId(useFrontFacingCamera);

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
            if (m_camera != IntPtr.Zero)
            {
                Z.CameraSourcePause(m_camera);
                Z.CameraSourceDestroy(m_camera);
            }
        }
        
        #endregion

        public void RegisterCameraListener(ICameraListener listener)
        {
            listeners.Add(listener);
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

        public bool IsMirrored()
        {
            return m_isMirrored;
        }

        public IntPtr GetPipeline()
        {
            return m_pipeline;
        }

    }
}