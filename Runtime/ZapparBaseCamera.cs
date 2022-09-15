using System;
using UnityEngine;
using System.Collections.Generic;

namespace Zappar
{
    public abstract class ZapparBaseCamera : MonoBehaviour
    {
        public abstract ZapparTrackingTarget TrackerAtOrigin { get; set; }

        public abstract Camera UnityCamera { get; }

        public abstract bool FrontFacingCamera { get; set; }

        public abstract bool UseGyroForCameraAttitude { get; set; }

        public abstract bool MirrorCameraFeed { get; set; }

#if UNITY_EDITOR
        public abstract string EditorCameraId { get; set; }
#endif

        private IntPtr? m_camera = null;
        private IntPtr? m_pipeline = null;

        private bool m_hasInitialized = false;

        private Matrix4x4 m_cameraPose;
        private float[] m_cameraModel = null;
        private List<ICameraListener> m_listeners = new List<ICameraListener>();
#if UNITY_WEBGL && !UNITY_EDITOR
        private bool m_autoPausedWebglCamera = false;
#endif

        public bool PipelineIsInitialized => m_hasInitialized;
        public IntPtr GetPipeline => m_pipeline.Value;
        public bool CameraSourceInitialized { get; protected set; } = false;

        public bool CameraSourcePaused { get; private set; } = false;

        public Matrix4x4 CameraPose { get => m_cameraPose; protected set => m_cameraPose = value; }

#region Unity_Methods

        public virtual void Awake()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Z.AndroidApplicationContextSet();
#endif
            m_cameraModel = new float[] { 0, 0, 0, 0, 0, 0 };
        }

        public virtual void Start()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
            var log = LogManager.Instance;
#endif
            m_cameraPose = Matrix4x4.identity;

            Z.Initialize();
            var settings = ZSettings.UARSettings;
            ZPermissions.CheckAllPermissions();
        }

        public virtual void Update()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (Z.ZapparInFocus())
            {
                if (CameraSourceInitialized && m_autoPausedWebglCamera && CameraSourcePaused)
                {
                    ToggleActiveCamera(false);
                    m_autoPausedWebglCamera = false;
                }
            }
            else if (CameraSourceInitialized && !CameraSourcePaused)
            {
                ToggleActiveCamera(true);
                m_autoPausedWebglCamera = true;
            }
#endif

            // If we haven't yet initialised the Zappar library.
            if (!m_hasInitialized)
            {
                InitializeZapparCamera();
                if (m_hasInitialized)
                {
                    // The Zappar library has been initialised and _this_ pipeline is created, so it is safe for 
                    // any listeners to now initialise.
                    foreach (ICameraListener listener in m_listeners)
                    {
                        listener.OnMirroringUpdate(MirrorCameraFeed);
                        listener.OnZapparInitialized(m_pipeline.Value);
                    }
                }
            }
            else if (m_camera != null)
            {
                // library is initialised but camera hasn't started
                if (!CameraSourceInitialized)
                {
                    if (ZPermissions.PermissionGrantedAll)
                    {
                        //Permissions have been granted but camera isn't started
                        Z.PipelineGLContextSet(m_pipeline.Value);
                        Z.CameraSourceStart(m_camera.Value);
                        CameraSourceInitialized = true;
                    }
                    else
                    {
                        // Zappar library is initialised, permissions have now been requested
                        ZPermissions.CheckAllPermissions();
                    }
                }
                // initialised, permissions granted, and camera started
                else if (!CameraSourcePaused)
                {
                    PipelineFrameUpdate();
                }
            }
        }

        protected virtual void PipelineFrameUpdate(bool updateUnityCam = true, bool uploadCameraFrame = true)
        {
            Z.Process(m_pipeline.Value);
            Z.PipelineFrameUpdate(m_pipeline.Value);
            if (uploadCameraFrame)
                Z.CameraFrameUpload(m_pipeline.Value);

            UpdatePose();
            if (updateUnityCam)
                UnityCamera.projectionMatrix = Z.PipelineProjectionMatrix(
                    m_pipeline.Value,
                    Screen.width,
                    Screen.height,
                    UnityCamera.nearClipPlane,
                    UnityCamera.farClipPlane,
                    ref m_cameraModel);
        }

        public virtual void OnDestroy()
        {
            if (m_pipeline != null) Z.PipelineDestroy(m_pipeline.Value);
            if (m_camera != null)
            {
                Z.CameraSourcePause(m_camera.Value);
                Z.CameraSourceDestroy(m_camera.Value);
            }
            m_hasInitialized = false;
        }

#endregion

        /// <summary>
        /// Create new zappar camera with set settings
        /// </summary>
        protected void InitializeZapparCamera()
        {
            if (!Z.HasInitialized())
                return;

            if (m_pipeline == null)
                m_pipeline = Z.PipelineCreate();

            if (m_pipeline != null)
            {
                string cameraID = "";
#if UNITY_EDITOR
                cameraID = IdFromName(EditorCameraId);
#endif
                if (string.IsNullOrEmpty(cameraID))
                    cameraID = Z.CameraDefaultDeviceId(FrontFacingCamera);

                m_camera = Z.CameraSourceCreate(m_pipeline.Value, cameraID);

                if (!ZPermissions.PermissionGrantedAll)
                    ZPermissions.RequestPermission();

                m_hasInitialized = true;
            }
        }

        /// Register the addition/destruction of camera listeners
        public virtual void RegisterCameraListener(ICameraListener listener, bool add)
        {
            if (add && !m_listeners.Contains(listener))
            {
                m_listeners.Add(listener);
            }
            else if (!add)
            {
                m_listeners.Remove(listener);
            }
        }

        /// Start or stop active zappar camera
        public virtual bool ToggleActiveCamera(bool pause)
        {
            if (pause == CameraSourcePaused || !m_hasInitialized) return false;

            if (pause)
            {
                Z.CameraSourcePause(m_camera.Value);
            }
            else
            {
                Z.CameraSourceStart(m_camera.Value);
            }

            CameraSourcePaused = pause;

            foreach (var v in m_listeners)
            {
                v.OnZapparCameraPaused(pause);
            }

            return true;
        }

        /// Switch to Front/Selfie camera mode. Returns: request success flag
        public virtual bool SwitchToFrontCameraMode(bool mirror = true)
        {
            if (!CameraSourceInitialized || m_camera == null) return false;
            
            FrontFacingCamera = true;
            MirrorCameraFeed = mirror;

            foreach (var v in m_listeners)
            {
                v.OnZapparCameraPaused(true);
            }
            //Z.PipelineDestroy(m_pipeline.Value);
            Z.CameraSourcePause(m_camera.Value);
            Z.CameraSourceDestroy(m_camera.Value);
            m_camera = null;

            Invoke(nameof(StartNewZapparCamera), 0.1f);
            return true;
        }

        private void StartNewZapparCamera()
        {
            InitializeZapparCamera();
            if (m_camera != null)
            {
                Z.CameraSourceStart(m_camera.Value);
                foreach (var v in m_listeners)
                {
                    v.OnMirroringUpdate(MirrorCameraFeed);
                    v.OnZapparCameraPaused(false);
                }
            }
            else
            {
                Debug.LogError("Failed to start zappar camera with new settings!");
            }
        }

        /// Switch to Rear camera mode. Returns: request success flag
        public virtual bool SwitchToRearCameraMode(bool mirror = false)
        {
            if (!CameraSourceInitialized || m_camera == null) return false;

            FrontFacingCamera = false;
            MirrorCameraFeed = mirror;

            foreach (var v in m_listeners)
            {
                v.OnZapparCameraPaused(true);
            }
            Z.CameraSourcePause(m_camera.Value);
            Z.CameraSourceDestroy(m_camera.Value);
            m_camera = null;

            Invoke(nameof(StartNewZapparCamera), 0.1f);
            return true;
        }

        protected virtual void UpdatePose()
        {
            if (TrackerAtOrigin == null)
            {
                if (UseGyroForCameraAttitude)
                    m_cameraPose = Z.PipelineCameraPoseWithAttitude(m_pipeline.Value, FrontFacingCamera);
                else
                    m_cameraPose = Z.PipelineCameraPoseDefault(m_pipeline.Value);
            }
            else
            {
                Matrix4x4 anchorPose = TrackerAtOrigin.AnchorPoseCameraRelative();
                m_cameraPose = Z.PipelineCameraPoseWithOrigin(m_pipeline.Value, anchorPose);
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

    }
}