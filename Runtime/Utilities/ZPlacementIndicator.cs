using UnityEngine;

namespace Zappar
{
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public class ZPlacementIndicator : MonoBehaviour
    {
        struct Plane
        {
            public Vector3 P0;
            public Vector3 Norm;
            public override string ToString() { return "Pos:" + P0 + " Norm:" + Norm; }
        };
        private bool m_initialized = false;
        public bool IsActive => m_initialized;

        private MeshFilter m_mf;
        private MeshRenderer m_renderer;
        private Material m_rendMat;

        private ZapparWorldTrackingTarget m_tracker;
        private bool m_anchorAtOrigin;
        private Plane m_anchorPlane;
        private Vector3 m_placementPos;


        private const float k_placementSmoothness = 2.5f;
#if ZAPPAR_SRP
        private const string k_shader = "Zappar/UnlitTexAndShadowSRP";
#else
        private const string k_shader = "Zappar/UnlitTexAndShadow";
#endif
        public void InitializeUI(ZapparWorldTrackingTarget tracker, bool anchorAtOrigin)
        {
            m_tracker = tracker;
            m_anchorAtOrigin = anchorAtOrigin;
            if (m_mf == null)
            {
                m_mf = GetComponent<MeshFilter>();
                m_mf.sharedMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            }
            if (m_renderer == null)
            {
                m_renderer = GetComponent<MeshRenderer>();
                m_rendMat = new Material(Shader.Find(k_shader));
            }
            m_rendMat.mainTexture = tracker.IndicatorProps.PlacementTexture;
            m_renderer.sharedMaterial = m_rendMat;

            if(!tracker.IndicatorProps.AddShadow)
                m_rendMat.SetFloat("_ShadowIntensity", 0);
            transform.localScale = Vector3.one * tracker.IndicatorProps.Scale;

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            m_anchorPlane.P0 = tracker.transform.position;
            m_anchorPlane.Norm = tracker.transform.up;
            
            //Debug.Log($"Plane pos:{m_anchorPlane.P0} and norm:{m_anchorPlane.Norm}");
            gameObject.SetActive(true);
            m_initialized = true;
        }

        public void UpdatePose(ref Ray camDir)
        {
            if (!m_initialized)
            {
                Debug.LogError("Placement indicator not initialized");
                return;
            }

            if (!m_anchorAtOrigin)
            {
                m_anchorPlane.P0 = m_tracker.transform.position;
                m_anchorPlane.Norm = m_tracker.transform.up;
            }

            if(GetRayPlaneIntersectionPoint(ref m_anchorPlane,ref camDir, ref m_placementPos))
            {
                Vector3 localPos = m_tracker.transform.InverseTransformPoint(m_placementPos);
                //transform.localPosition = localPos;
                //Apply smoothness
                transform.localPosition = Vector3.Lerp(transform.localPosition, localPos, Time.deltaTime*k_placementSmoothness);
            }
        }

        public void ResetUIAndDisableObj()
        {
            m_initialized = false;
            m_tracker = null;
            gameObject.SetActive(false);
        }

        private bool GetRayPlaneIntersectionPoint(ref Plane plane, ref Ray ray, ref Vector3 pos)
        {
            float denom = Vector3.Dot(plane.Norm, ray.direction);
            if (Mathf.Abs(denom) < 0.0001f) return false; //parallel lines

            Vector3 p0r0 = plane.P0 - ray.origin;
            float dist = Vector3.Dot(p0r0, plane.Norm) / denom;
            dist = Mathf.Clamp(dist, m_tracker.MinDistance, m_tracker.MaxDistance);
            pos = ray.origin + ray.direction * dist;

            return true;
        }
    }
}