using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace Zappar
{
    public class ZapparReflectionProbe : MonoBehaviour, ICameraListener
    {
        public const string ReflectionLayer = "ZapparReflect";
        [SerializeField, Tooltip("Must be a power of 2")]
        private int m_mapResolution = 32;
        [SerializeField, Tooltip("Ensure this size(Green Box) covers your model to apply the realtime reflection")]
        private int m_probeSize = 50;

        [SerializeField, Tooltip("GameObject layers to use for realtime reflection. Default is ZapparReflect.")]
        private LayerMask m_cullingMask = 0;

        [SerializeField]
        private ReflectionProbeTimeSlicingMode m_timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;

        private Transform m_cameraTransform = null;
        private MeshRenderer m_camTextureProjectionSurface = null;
        private ReflectionProbe m_reflectionProbe = null;
        private ZapparCameraBackground m_cameraBackground = null;

        private bool m_hasInitialized = false;
        //private bool m_isMirrored = false;
        private bool m_isPaused = false;

        public void OnZapparInitialized(IntPtr pipeline)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            m_hasInitialized = true;
        }

        public void OnZapparCameraPaused(bool pause) { m_isPaused = pause; }

        public void OnMirroringUpdate(bool mirrored)
        {
            //m_isMirrored = mirrored;
        }

        private void OnEnable()
        {
            if(m_cullingMask==0)
            {
                Debug.Log("Please define the culling mask for reflection. Using default layer: " + ReflectionLayer);
                m_cullingMask = LayerMask.GetMask(new[] { ReflectionLayer });
            }
        }

        private void Start()
        {
            if(ZapparCamera.Instance == null)
            {
                gameObject.SetActive(false);
                Debug.LogError("No Active Zappar Camera found in scene");
                return;
            }

            if (m_mapResolution == 0 || (m_mapResolution & (m_mapResolution - 1)) != 0)
            {
                m_mapResolution = (int)Mathf.Pow(2, (int)Mathf.Log(m_mapResolution, 2) + 1);
            }

            ZapparCamera.Instance.RegisterCameraListener(this, true);

            //Ignore the ReflectionLayer from main zappar camera
            Camera zapCam = ZapparCamera.Instance.gameObject.GetComponent<Camera>();
            zapCam.cullingMask &= ~m_cullingMask;
            m_cameraTransform = ZapparCamera.Instance.gameObject.transform;
            m_cameraBackground = m_cameraTransform.GetComponentInChildren<ZapparCameraBackground>();

            m_camTextureProjectionSurface = gameObject.transform.GetComponentInChildren<MeshRenderer>(true);
            if(m_camTextureProjectionSurface==null)
            {
                GameObject go = GetTextureProjectionSurface();
                go.transform.SetParent(this.transform);
                go.transform.localScale = new Vector3(100, 100, 100);
                m_camTextureProjectionSurface = go.GetComponent<MeshRenderer>();
                go.layer = LayerMask.NameToLayer(ReflectionLayer);
            }
            m_camTextureProjectionSurface.gameObject.SetActive(true);
            //camTextureProjectionSurface.material.mainTextureScale = new Vector2(1, -1);

            UpdateReflectionProbe();

            if (ZapparCamera.Instance.CameraSourceInitialized && !m_hasInitialized)
            {
                OnMirroringUpdate(ZapparCamera.Instance.MirrorCamera);
                OnZapparCameraPaused(ZapparCamera.Instance.CameraSourcePaused);
                OnZapparInitialized(ZapparCamera.Instance.GetPipeline);
            }
        }

        private void UpdateReflectionProbe()
        {
            m_reflectionProbe = gameObject.GetComponentInChildren<ReflectionProbe>() ?? gameObject.AddComponent<ReflectionProbe>();

            m_reflectionProbe.mode = ReflectionProbeMode.Realtime;
            m_reflectionProbe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
            m_reflectionProbe.resolution = m_mapResolution;
            m_reflectionProbe.clearFlags = ReflectionProbeClearFlags.SolidColor;
            m_reflectionProbe.backgroundColor = Color.black;
            m_reflectionProbe.size = Vector3.one * m_probeSize;
            m_reflectionProbe.cullingMask = m_cullingMask;
            m_reflectionProbe.timeSlicingMode = m_timeSlicingMode;
        }

        private void OnDestroy()
        {
            Destroy(m_reflectionProbe);
            ZapparCamera.Instance?.RegisterCameraListener(this, false);
        }

        private void LateUpdate()
        {
            if (!m_hasInitialized || m_isPaused) 
                return;

            m_camTextureProjectionSurface.material.SetMatrix("_nativeTextureMatrix", m_cameraBackground.GetTextureMatrix);
            m_camTextureProjectionSurface.material.mainTexture = m_cameraBackground.GetCameraTexture;
            m_camTextureProjectionSurface.transform.rotation = m_cameraTransform.rotation * Quaternion.AngleAxis(90, Vector3.up);

            m_reflectionProbe?.RenderProbe();
        }

        private GameObject GetTextureProjectionSurface()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "TextureProjection";
            Destroy(go.GetComponent<Collider>());
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = LightProbeUsage.Off;
            mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
            Material texMat = new Material(Shader.Find("Zappar/Unlit/InvertedSurface"));
            //texMat.mainTextureScale = new Vector2(1, -1);
            mr.material = texMat;
            return go;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.1f, 0.9f, 0, 0.5f);
            Gizmos.DrawWireCube(transform.position, Vector3.one * m_probeSize);
        }
    }
}