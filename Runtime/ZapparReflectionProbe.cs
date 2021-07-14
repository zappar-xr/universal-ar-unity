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
        private int mapResolution = 32;
        [SerializeField]
        private int probeSize = 50;

        [SerializeField]
        private LayerMask cullingMask = 0;

        [SerializeField]
        private ReflectionProbeTimeSlicingMode timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;

        private Transform cameraTransform = null;
        private MeshRenderer camTextureProjectionSurface = null;
        private ReflectionProbe reflectionProbe = null;
        private ZapparCameraBackground cameraBackground = null;

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
            if(cullingMask==0)
            {
                Debug.Log("Please define the culling mask for reflection");
                cullingMask = LayerMask.GetMask(new[] { ReflectionLayer });
            }
        }

        private void Start()
        {
            if (mapResolution == 0 || (mapResolution & (mapResolution - 1)) != 0)
            {
                mapResolution = (int)Mathf.Pow(2, (int)Mathf.Log(mapResolution, 2) + 1);
            }

            if (ZapparCamera.Instance != null)
                ZapparCamera.Instance.RegisterCameraListener(this);

            //Ignore the ReflectionLayer from main zappar camera
            Camera zapCam = ZapparCamera.Instance.gameObject.GetComponent<Camera>();
            zapCam.cullingMask &= ~cullingMask;
            cameraTransform = ZapparCamera.Instance.gameObject.transform;
            cameraBackground = cameraTransform.GetComponentInChildren<ZapparCameraBackground>();

            camTextureProjectionSurface = gameObject.transform.GetComponentInChildren<MeshRenderer>(true);
            if(camTextureProjectionSurface==null)
            {
                GameObject go = GetTextureProjectionSurface();
                go.transform.SetParent(this.transform);
                go.transform.localScale = new Vector3(100, 100, 100);
                camTextureProjectionSurface = go.GetComponent<MeshRenderer>();
                go.layer = LayerMask.NameToLayer(ReflectionLayer);
            }
            camTextureProjectionSurface.gameObject.SetActive(true);
            //camTextureProjectionSurface.material.mainTextureScale = new Vector2(1, -1);

            UpdateReflectionProbe();
        }

        private void UpdateReflectionProbe()
        {
            reflectionProbe = gameObject.GetComponentInChildren<ReflectionProbe>() ?? gameObject.AddComponent<ReflectionProbe>();

            reflectionProbe.mode = ReflectionProbeMode.Realtime;
            reflectionProbe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
            reflectionProbe.resolution = mapResolution;
            reflectionProbe.clearFlags = ReflectionProbeClearFlags.SolidColor;
            reflectionProbe.backgroundColor = Color.black;
            reflectionProbe.size = Vector3.one * probeSize;
            reflectionProbe.cullingMask = cullingMask;
            reflectionProbe.timeSlicingMode = timeSlicingMode;
        }

        private void OnDestroy()
        {
            Destroy(reflectionProbe);
        }

        private void LateUpdate()
        {
            if (!m_hasInitialised)
            {
                return;
            }

            camTextureProjectionSurface.material.SetMatrix("_nativeTextureMatrix", cameraBackground.GetTextureMatrix);
            camTextureProjectionSurface.material.mainTexture = cameraBackground.GetCameraTexture;
            camTextureProjectionSurface.transform.rotation = cameraTransform.rotation * Quaternion.AngleAxis(90, Vector3.up);

            reflectionProbe?.RenderProbe();
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
            Gizmos.DrawWireCube(transform.position, Vector3.one * probeSize);
        }
    }
}