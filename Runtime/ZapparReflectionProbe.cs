using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace Zappar
{
    public class ZapparReflectionProbe : MonoBehaviour, ZapparCamera.ICameraListener
    {
        public const string ReflectionLayer = "ZapparReflect";
        
        [Tooltip("Must be a power of 2")]
        [SerializeField]
        private int m_mapResolution = 32;
        [SerializeField]
        private int m_probeSize = 50;

        [SerializeField]
        private LayerMask m_cullingMask = 0;

        [SerializeField]
        private ReflectionProbeTimeSlicingMode m_timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;

        private Transform m_cameraTransform = null;
        private MeshRenderer m_camTextureProjectionSurface = null;
        private ReflectionProbe m_reflectionProbe = null;
        private ZapparCameraBackground m_cameraBackground = null;

        private bool m_hasInitialised = false;
        private bool m_isMirrored = false;

        public void OnZapparInitialised(IntPtr pipeline)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            m_hasInitialised = true;
        }

        public void OnMirroringUpdate(bool mirrored)
        {
            m_isMirrored = mirrored;
        }

        private void OnEnable()
        {
            if(m_cullingMask==0)
            {
                Debug.Log("Please define the culling mask for reflection. Using fallback layer: " + ReflectionLayer);
                m_cullingMask = LayerMask.GetMask(new[] { ReflectionLayer });
            }
        }

        private void Start()
        {
            if (m_mapResolution == 0 || (m_mapResolution & (m_mapResolution - 1)) != 0)
            {
                m_mapResolution = (int)Mathf.Pow(2, (int)Mathf.Log(m_mapResolution, 2) + 1);
            }

            if (ZapparCamera.Instance != null)
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
            if (ZapparCamera.Instance != null)
                ZapparCamera.Instance.RegisterCameraListener(this, false);
        }

        private void LateUpdate()
        {
            if (!m_hasInitialised)
            {
                return;
            }

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