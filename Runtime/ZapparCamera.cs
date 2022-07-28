using UnityEngine;

namespace Zappar
{
    /// Wrapper around a single Pipeline example
    [RequireComponent(typeof(Camera))]
    public class ZapparCamera : ZapparBaseCamera
    {
       
        public static ZapparCamera Instance { get; private set; }

        #region Editor_Params

        [Tooltip("Place anchor/target at origin for tracking")]
        public ZapparTrackingTarget AnchorOrigin;

        [Tooltip("Selection between Front/Rear camera at runtime. Not applicable for Editor mode")]
        public bool UseFrontFacingCamera;
        
        [Tooltip("Fix camera position at origin but apply rotation w.r.t device orientation. Leave " + nameof(AnchorOrigin) + " (above) as 'None' for this setting.")]
        public bool CameraAttitudeFromGyro;

        [Tooltip("Mirror the device camera stream")]
        public bool MirrorCamera = false;

        [CameraSourcesListPopup]
        [Tooltip("Select camera to use when in Play mode.")]
        public string EditorCamera;

        #endregion

        public override ZapparTrackingTarget TrackerAtOrigin { get => AnchorOrigin; set => AnchorOrigin = value; }
        public override Camera UnityCamera { get => m_unityCamera; }
        public override bool FrontFacingCamera { get => UseFrontFacingCamera; set => UseFrontFacingCamera = value; }
        public override bool UseGyroForCameraAttitude { get => CameraAttitudeFromGyro; set => CameraAttitudeFromGyro = value; }
        public override bool MirrorCameraFeed { get => MirrorCamera; set => MirrorCamera = value; }

#if UNITY_EDITOR
        public override string EditorCameraId { get => EditorCamera; set => EditorCamera = value; }
#endif

        private Camera m_unityCamera;

        #region Unity_Methods

        public override void Awake()
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

            base.Awake();
        }

        public override void Start()
        {
            m_unityCamera = GetComponent<Camera>();
            base.Start();
        }

        #endregion

    }
}