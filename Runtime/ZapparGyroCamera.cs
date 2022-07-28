using UnityEngine;

namespace Zappar
{
    /// <summary>
    /// Fix camera position at origin and apply rotation w.r.t devices' orientation
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ZapparGyroCamera : ZapparBaseCamera
    {
        public static ZapparGyroCamera Instance { get; private set; }

        [SerializeField, HideInInspector]
        public bool UseCameraBackground = false;

        public override ZapparTrackingTarget TrackerAtOrigin { get => null; set => Debug.Log("Gyro camera doesn't track any target"); }
        public override Camera UnityCamera { get => m_unityCamera; }
        public override bool FrontFacingCamera { get => false; set => Debug.Log("Rear by default"); }
        public override bool UseGyroForCameraAttitude { get => true; set => Debug.Log("Always true"); }
        public override bool MirrorCameraFeed { get => false; set => Debug.Log("Always false"); }

#if UNITY_EDITOR
        [HideInInspector]
        public string EditorCamera = "";
        public override string EditorCameraId { get => EditorCamera; set => EditorCamera = value; }
#endif
        private Camera m_unityCamera;
        private const float InitialDelay = 0.9f;    //delay first pose update
        private bool m_disablePoseUpdate = false;

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

            transform.localPosition = Vector3.zero;
#if UNITY_WEBGL && !UNITY_EDITOR
            m_disablePoseUpdate = true;
            Invoke(nameof(EnablePoseUpdate),InitialDelay);
#endif
        }
        private void EnablePoseUpdate()
        {
            m_disablePoseUpdate = false;
        }

        public override void Start()
        {
            m_unityCamera = GetComponent<Camera>();
            base.Start();
        }

        protected override void PipelineFrameUpdate(bool updateUnityCam = true, bool uploadCameraFrame = true)
        {
            base.PipelineFrameUpdate(UseCameraBackground, UseCameraBackground);
        }

        public override bool SwitchToFrontCameraMode(bool mirror = true) => false;
        public override bool SwitchToRearCameraMode(bool mirror = false) => false;

        protected override void UpdatePose()
        {
            CameraPose = Z.PipelineCameraPoseWithAttitude(GetPipeline, false);

            if (m_disablePoseUpdate) return;

            Matrix4x4 cameraPoseUnity = Z.ConvertToUnityPose(CameraPose);
            transform.localPosition = Z.GetPosition(cameraPoseUnity);
            transform.localRotation = Z.GetRotation(cameraPoseUnity);
            transform.localScale = Z.GetScale(cameraPoseUnity);
        }

    }
}