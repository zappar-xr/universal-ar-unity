using System;
using UnityEngine;
using UnityEngine.Events;

namespace Zappar
{
    public class ZapparWorldTrackingTarget : ZapparTrackingTarget, ICameraListener
    {
        [Tooltip("Raised when world tracking initialization is finished.")]
        public UnityEvent OnInitializationComplete;
        [Tooltip("Raised when ground anchor is placed after tracking initialization step.")]
        public UnityEvent OnGroundAnchorPlaced;
        [Tooltip("Raised when OnReset is explicitly called on this behaviour")]
        public UnityEvent OnTrackingReset;

        [Serializable]
        public struct PlacementIndicator
        {
            [Tooltip("Texture used to show on ground placment position")]
            public Texture PlacementTexture;
            [Tooltip("Add shadow over ground placement for depth perception")]
            public bool AddShadow;
            [Tooltip("Scale applied to the ground plane.")]
            public float Scale;
            public PlacementIndicator(Texture tex = null, bool shadow = true, float scale=1f)
            {
                PlacementTexture = tex;
                AddShadow = shadow;
                Scale = scale;
            }
        };

        public IntPtr? WorldTracker = null;

        [SerializeField, Tooltip("Show tracker initialization state animation via texture")]
        private bool m_showInitializationAnim = true;
        [HideInInspector]
        public Texture InitScreenTexture = null;
        [HideInInspector]
        public float InitTextureTime = 2f;
        [SerializeField, Tooltip("Anchor placement indicator properties")]
        public PlacementIndicator IndicatorProps = new PlacementIndicator();
        [SerializeField, Tooltip("Accept touch event to place the ground anchor for tracking")]
        private bool m_placeOnTouch = true;
        [SerializeField, Tooltip("Show a placement indicator that allows the user to place the anchor after tracking state has finished initialization.")]
        public bool m_showPlacementIndicator = true;
        [SerializeField, Tooltip("Show the ground anchor and respective hierarchy along with placement indicator. Otherwise the ground anchor is enabled after the placement step.")]
        private bool m_showAnchorWithIndicator = false;
        [HideInInspector, SerializeField]
        public Transform GroundAnchor;
        [HideInInspector, SerializeField]
        private ZapparCamera m_zCamera;
        [HideInInspector, SerializeField]
        public float MinDistance = 0.01f;
        [HideInInspector, SerializeField]
        public float MaxDistance = 1.0f;


        //private const float m_maxCameraRot = 40.0f;
        private ZPlacementIndicator m_placementIndicator = null;
        private Ray m_zCameraRay;

        private bool m_pipelineInitialized = false;
        private bool m_isMirrored = false;
        private bool m_isPaused = false;
        public bool UserHasPlaced { get; private set; }
        //Whether world tracker is in a good tracking state
        public bool TrackerInitialized { get; private set; }
        public bool ShowInitializationAnim => m_showInitializationAnim;

        private void Start()
        {
            if (ZapparCamera.Instance != null)
                ZapparCamera.Instance.RegisterCameraListener(this, true);

            if (ZapparCamera.Instance.CameraSourceInitialized && !m_pipelineInitialized)
            {
                OnMirroringUpdate(ZapparCamera.Instance.MirrorCamera);
                OnZapparCameraPaused(ZapparCamera.Instance.CameraSourcePaused);
                OnZapparInitialized(ZapparCamera.Instance.GetPipeline);
            }
            GroundAnchor.gameObject.SetActive(false);
        }

        public void OnZapparInitialized(IntPtr pipeline)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WorldTracker = Z.WorldTrackerCreate(pipeline);
#endif
            m_pipelineInitialized = true;
        }

        public void OnZapparCameraPaused(bool pause) { m_isPaused = pause; }

        public void OnMirroringUpdate(bool mirrored)
        {
            m_isMirrored = mirrored;
        }

        void UpdateTargetPose()
        {
            Matrix4x4 cameraPose = ZapparCamera.Instance.CameraPose;
            Matrix4x4 worldTrackerPose =
#if UNITY_WEBGL && !UNITY_EDITOR
                Z.WorldTrackerGroundAnchorPose(WorldTracker.Value, cameraPose, m_isMirrored);
#else
                Matrix4x4.identity;
#endif
            Matrix4x4 targetPose = Z.ConvertToUnityPose(worldTrackerPose);

            transform.localPosition = Z.GetPosition(targetPose);
            transform.localRotation = Z.GetRotation(targetPose);
            transform.localScale = Z.GetScale(targetPose);
        }

        private void ShowPlacementIndicator()
        {
            if (m_showPlacementIndicator)
            {
                if (m_placementIndicator == null)
                {
                    GameObject go = new GameObject("PlacementIndicator", new[] { typeof(ZPlacementIndicator) });
                    go.transform.SetParent(transform);

                    m_placementIndicator = go.GetComponent<ZPlacementIndicator>();
                    m_placementIndicator.InitializeUI(this, m_zCamera.TrackerAtOrigin!=null);
                    if (m_showAnchorWithIndicator)
                        GroundAnchor.gameObject.SetActive(true);
                }
                else if (!m_placementIndicator.gameObject.activeSelf)
                {
                    m_placementIndicator.InitializeUI(this, m_zCamera.TrackerAtOrigin != null);
                    if (m_showAnchorWithIndicator)
                        GroundAnchor.gameObject.SetActive(true);
                }

                if (m_zCamera != null)
                {
                    m_zCameraRay.origin = m_zCamera.transform.position;
                    m_zCameraRay.direction = m_zCamera.transform.forward;
                    m_placementIndicator.UpdatePose(ref m_zCameraRay);
                    if (m_showAnchorWithIndicator)
                    {
                        GroundAnchor.localPosition = m_placementIndicator.transform.localPosition;
                    }
                }
                else
                {
                    Debug.LogError("ZapparCamera not assigned under Placement Indicator properties!");
                }
            }
            else
            {
                PlaceTrackerAnchor();
            }
        }

        private void Update()
        {
            if (!m_pipelineInitialized || WorldTracker == null || m_isPaused)
            {
#if UNITY_EDITOR  //For simulation purposes
                if (TrackerInitialized && !UserHasPlaced)
                    ShowPlacementIndicator();
#endif
                return;
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            // Tracker is in initialization state
            if (Z.WorldTrackerQuality(WorldTracker.Value) == (int)Z.WorldTrackerState.WORLD_TRACKER_QUALITY_INITIALIZING)
            {
                TrackerInitialized = false;
                return;
            } else if(!TrackerInitialized) { //WORLD_TRACKER either GOOD or ORIENTATION_ONLY
                TrackerInitialized=true;
                OnInitializationComplete?.Invoke();
            }

            if (!UserHasPlaced)
            {
                ShowPlacementIndicator();
            }

            if (m_placeOnTouch && !UserHasPlaced && Input.touchCount > 0)
            {
                PlaceTrackerAnchor();
            }

            UpdateTargetPose();
#endif
        }

        Vector2 scanXPos = new Vector2(150, Screen.width - 350);
        float dt;
        private void OnGUI()
        {
            if (!m_showInitializationAnim || TrackerInitialized) return;
            if (m_placementIndicator != null && m_placementIndicator.IsActive)
            {
                m_placementIndicator.ResetUIAndDisableObj();
                GroundAnchor.gameObject.SetActive(false);
            }

            if ((Time.time - dt) / InitTextureTime > 1f)
            {
                dt = Time.time;
            }
            GUI.DrawTexture(new Rect(Mathf.Lerp(scanXPos.y,scanXPos.x,(Time.time-dt)/InitTextureTime), Screen.height/2, 300, 300), InitScreenTexture);
        }

        private void OnDestroy()
        {
            if (m_pipelineInitialized)
            {
                if (WorldTracker != null)
                {
#if UNITY_WEBGL && !UNITY_EDITOR
                    Z.WorldTrackerDestroy(WorldTracker.Value);
#endif
                    WorldTracker = null;
                }
                TrackerInitialized = false;
            }
            if (ZapparCamera.Instance != null)
                ZapparCamera.Instance.RegisterCameraListener(this, false);
        }

        public override Matrix4x4 AnchorPoseCameraRelative()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return Z.WorldTrackerGroundAnchorPoseCameraRelative(WorldTracker.Value, m_isMirrored);
#else
            return Matrix4x4.identity;
#endif
        }

        public void PlaceTrackerAnchor()
        {
            if (!TrackerInitialized)
            {
                Debug.LogError("Tracker is still initializing!");
                return;
            }
            GroundAnchor.localPosition = m_placementIndicator.transform.localPosition;
            m_placementIndicator.ResetUIAndDisableObj();
            GroundAnchor.gameObject.SetActive(true);
            UserHasPlaced = true;
            OnGroundAnchorPlaced?.Invoke();
        }

        public void ResetTrackerAnchor()
        {
            if (WorldTracker == null)
            {
                Debug.LogError("World tracking is not initialized to call reset!");
                return;
            }
            GroundAnchor.gameObject.SetActive(false);
            UserHasPlaced = false;
            TrackerInitialized = false;
#if UNITY_WEBGL && !UNITY_EDITOR
            Z.WorldTrackerReset(WorldTracker.Value);
#endif
            OnTrackingReset?.Invoke();
            Debug.Log("Reseting world tracker");
        }

#if UNITY_EDITOR
        public void SimulatePlacementIndicator()
        {
            TrackerInitialized = true;//scanning state switched to good
            OnInitializationComplete?.Invoke();
        }
#endif
    }
}