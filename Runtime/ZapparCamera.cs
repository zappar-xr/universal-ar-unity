using System;
using UnityEngine;
using System.Collections.Generic;

namespace Zappar
{
    /// Wrapper around a single Pipeline example
    [RequireComponent(typeof(Camera))]
    public class ZapparCamera : MonoBehaviour
    {
// 0219: variable assigned but not used
// 0414: private field assigned but not used
#pragma warning disable 0219, 0414  
       
        public static ZapparCamera Instance { get; private set; }

#region Editor_Params

        public ZapparTrackingTarget AnchorOrigin;

        [Tooltip("Selection between Front/Rear camera at runtime. Not applicable for Editor mode")]
        public bool UseFrontFacingCamera;

        [Tooltip("Fix camera position at origin but apply rotation w.r.t devices' orientation. Leave " + nameof(AnchorOrigin) + " (above) as 'None' for this setting.")]
        public bool CameraAttitudeFromGyro;

        [Tooltip("Mirror the device camera stream")]
        public bool MirrorCamera = false;

        [CameraSourcesListPopup]
        [Tooltip("Select camera to use when in Play mode.")]
        public string EditorCamera;

#endregion

        private IntPtr? m_camera = null;
        private IntPtr? m_pipeline = null;

        private bool m_hasInitialized = false;

        private Matrix4x4 m_cameraPose;
        private Camera m_unityCamera = null;
        private float[] m_cameraModel = null;
        private List<ICameraListener> m_listeners = new List<ICameraListener>();
        private bool m_autoPausedWebglCamera = false;

        public IntPtr GetPipeline => m_pipeline.Value;

        public bool CameraSourceInitialized { get; private set; } = false;

        public bool CameraSourcePaused { get; private set; } = false;

        public Matrix4x4 GetCameraPose => m_cameraPose;

#pragma warning restore 0219, 0414 

        #region Unity_Methods

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.Log("Please ensure there's only one zappar camera active in scene!");
                gameObject.SetActive(false);
                return;
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

            ZPermissions.CheckAllPermissions();
        }

        void Update()
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
                        listener.OnMirroringUpdate(MirrorCamera);
                        listener.OnZapparInitialized(m_pipeline.Value);
                    }
                }
            }
            else if(m_camera!=null)
            {
                // library is initialised but camera hasn't started
                if (!CameraSourceInitialized)
                {
                    if(ZPermissions.PermissionGrantedAll)
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
                else if(!CameraSourcePaused)
                {
                    Z.Process(m_pipeline.Value);
                    Z.PipelineFrameUpdate(m_pipeline.Value);
                    Z.CameraFrameUpload(m_pipeline.Value);

                    UpdatePose();

                    m_unityCamera.projectionMatrix = Z.PipelineProjectionMatrix(m_pipeline.Value, Screen.width, Screen.height, m_unityCamera.nearClipPlane, m_unityCamera.farClipPlane, ref m_cameraModel);
                }
            }
        }

        void OnDestroy()
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
        private void InitializeZapparCamera()
        {
            if (!Z.HasInitialized())
                return;
            
            if (m_pipeline == null)
                m_pipeline = Z.PipelineCreate();

            if (m_pipeline != null)
            {
                string cameraID = "";
#if UNITY_EDITOR
                cameraID = IdFromName(EditorCamera);
#endif
                if (string.IsNullOrEmpty(cameraID))
                    cameraID = Z.CameraDefaultDeviceId(UseFrontFacingCamera);

                m_camera = Z.CameraSourceCreate(m_pipeline.Value, cameraID);

                if (!ZPermissions.PermissionGrantedAll)
                    ZPermissions.RequestPermission();

                m_hasInitialized = true;
            }
        }

        /// Register the addition/destruction of camera listeners
        public void RegisterCameraListener(ICameraListener listener, bool add)
        {
            if (add && !m_listeners.Contains(listener))
            {
                m_listeners.Add(listener);
            }else if (!add)
            {
                m_listeners.Remove(listener);
            }
        }

        /// Start or stop active zappar camera
        public bool ToggleActiveCamera(bool pause)
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
        public bool SwitchToFrontCameraMode(bool mirror = true)
        {
            UseFrontFacingCamera = true;
            MirrorCamera = mirror;

            if (!CameraSourceInitialized || m_camera == null) return false;

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
                    v.OnMirroringUpdate(MirrorCamera);
                    v.OnZapparCameraPaused(false);
                }
            }
            else
            {
                Debug.LogError("Failed to start zappar camera with new settings!");
            }
        }

        /// Switch to Rear camera mode. Returns: request success flag
        public bool SwitchToRearCameraMode(bool mirror=false)
        {
            UseFrontFacingCamera = false;
            MirrorCamera = mirror;

            if (!CameraSourceInitialized || m_camera == null) return false;

            foreach (var v in m_listeners)
            {
                v.OnZapparCameraPaused(true);
            }
            Z.CameraSourcePause(m_camera.Value);
            Z.CameraSourceDestroy(m_camera.Value);
            m_camera = null;

            Invoke(nameof(StartNewZapparCamera), Time.maximumDeltaTime);
            return true;
        }

        private void UpdatePose()
        {
            if (AnchorOrigin == null)
            {
                if (CameraAttitudeFromGyro)
                    m_cameraPose = Z.PipelineCameraPoseWithAttitude(m_pipeline.Value, UseFrontFacingCamera);
                else
                    m_cameraPose = Z.PipelineCameraPoseDefault(m_pipeline.Value);
            }
            else
            {
                Matrix4x4 anchorPose = AnchorOrigin.AnchorPoseCameraRelative();
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