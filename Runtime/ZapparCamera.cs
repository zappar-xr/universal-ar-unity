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
        public interface ICameraListener
        {
            void OnZapparInitialised(IntPtr pipeline);
            void OnMirroringUpdate(bool mirrored);
        }

        public static ZapparCamera Instance { get; private set; }

#region Editor_Params

        public ZapparTrackingTarget anchorOrigin;

        [Tooltip("Selection between Front/Rear camera at runtime. Not applicable for Editor mode")]
        public bool UseFrontFacingCamera;

        [Tooltip("Fix camera position at origin but apply rotation w.r.t devices' orientation. Leave " + nameof(anchorOrigin) + " (above) as 'None' for this setting.")]
        public bool CameraAttitudeFromGyro;

        public bool MirrorRearCameras = false;

        public bool MirrorUserCameras = true;

        [CameraSourcesListPopup]
        [Tooltip("Select camera to use when in Play mode.")]
        public string EditorCamera;

#endregion

        private IntPtr? m_camera = null;
        private IntPtr? m_pipeline = null;

        private bool m_hasInitialised = false;

        private Matrix4x4 m_cameraPose;
        private Camera m_unityCamera = null;
        private float[] m_cameraModel = null;
        private List<ICameraListener> m_listeners = new List<ICameraListener>();
        private bool autoPausedWebglCamera = false;

        public bool IsMirrored { get; private set; }

        public IntPtr GetPipeline => m_pipeline.Value;

        public bool CameraHasStarted { get; private set; }

        public bool CameraSourcePaused { get; private set; }

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
                if (autoPausedWebglCamera && CameraHasStarted && CameraSourcePaused)
                {
                    ToggleActiveCamera(false);
                    autoPausedWebglCamera = false;
                }
            }
            else if (CameraHasStarted && !CameraSourcePaused)
            {
                ToggleActiveCamera(true);
                autoPausedWebglCamera = true;
            }
#endif

            // If we haven't yet initialised the Zappar library.
            if (!m_hasInitialised)
            {
                if (Z.HasInitialized())
                {
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

                        IsMirrored = (UseFrontFacingCamera && MirrorUserCameras) || (!UseFrontFacingCamera && MirrorRearCameras);

                        // The Zappar library has been initialised and _this_ pipeline is created, so it is safe for 
                        // any listeners to now initialise.
                        foreach (ICameraListener listener in m_listeners)
                        {
                            listener.OnMirroringUpdate(IsMirrored);
                            listener.OnZapparInitialised(m_pipeline.Value);
                        }

                        m_hasInitialised = true;
                    }
                }
            }
            else
            {
                // library is initialised but camera hasn't started
                if (!CameraHasStarted)
                {
                    if(ZPermissions.PermissionGrantedAll)
                    {
                        //Permissions have been granted but camera isn't started
                        Z.PipelineGLContextSet(m_pipeline.Value);
                        Z.CameraSourceStart(m_camera.Value);
                        CameraHasStarted = true;
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
            m_hasInitialised = false;
        }

#endregion

        // Register the addition/destruction of camera listeners
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

        //Start or stop active zappar camera
        public bool ToggleActiveCamera(bool pause)
        {
            if (pause == CameraSourcePaused || !m_hasInitialised) return false;

            if(pause)
            {
                Z.CameraSourcePause(m_pipeline.Value);
            }
            else
            {
                Z.CameraSourceStart(m_pipeline.Value);
            }
            CameraSourcePaused = pause;
            return true;
        }

        private void UpdatePose()
        {
            if (anchorOrigin == null)
            {
                if (CameraAttitudeFromGyro)
                    m_cameraPose = Z.PipelineCameraPoseWithAttitude(m_pipeline.Value, UseFrontFacingCamera);
                else
                    m_cameraPose = Z.PipelineCameraPoseDefault(m_pipeline.Value);
            }
            else
            {
                Matrix4x4 anchorPose = anchorOrigin.AnchorPoseCameraRelative();
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