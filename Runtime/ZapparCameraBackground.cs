using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Zappar
{
    [RequireComponent(typeof(Camera))]
    public class ZapparCameraBackground : MonoBehaviour
    {
        private Material m_CameraMaterial=null;

        private bool m_Initialised = false;
        private Texture2D m_CamTexture = null;
        private Matrix4x4 textureMatrix;
        private float[] textureMatElements = null;
        private Camera backgroundCamera = null;
        private ZapparCamera mainCamera = null;

        private float[] m_camerModel = null;

        public Texture2D GetCameraTexture => m_CamTexture;
        public Matrix4x4 GetTextureMatrix => textureMatrix;

        private void Awake()
        {
            m_CameraMaterial = new Material(Shader.Find("Zappar/CameraBackgroundShader"));
            if(m_CameraMaterial==null)
            {
                Debug.LogError("Can't render camera texture: Missing Zappar/CameraBackgroundShader!");
            }
            m_CameraMaterial.mainTexture = Texture2D.blackTexture;
            textureMatrix = new Matrix4x4();
            textureMatElements = new float[16];
            backgroundCamera = GetComponent<Camera>();
            mainCamera = GetComponentInParent<ZapparCamera>();
            m_camerModel = new float[] { 0, 0, 0, 0, 0, 0 };
        }

        void Point(float x, float y)
        {
            GL.TexCoord2(x, y);
            GL.Vertex3(x, y, -1);
        }

#if ZAPPAR_SRP
        private void Start()
        {
            RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
        }

        private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext arg1, Camera arg2)
        {
            if (arg2.depth != -1)
                return;
            m_CameraMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.LoadProjectionMatrix(Matrix4x4.Ortho(0, 1, 0, 1, 0, 1));
            GL.Begin(GL.QUADS);
            Point(0, 0);
            Point(0, 1);
            Point(1, 1);
            Point(1, 0);
            GL.End();
            GL.PopMatrix();
        }
#else
        void OnPostRender()
        {
            m_CameraMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.LoadProjectionMatrix(Matrix4x4.Ortho(0, 1, 0, 1, 0, 1));
            GL.Begin(GL.QUADS);
            Point(0, 0);
            Point(0, 1);
            Point(1, 1);
            Point(1, 0);
            GL.End();
            GL.PopMatrix();
        }

#endif
        void Update()
        {
            if (m_CameraMaterial == null || mainCamera == null)
                return;

            if (!m_Initialised)
            {
                m_Initialised = Z.HasInitialized() && mainCamera.CameraHasStarted;
                return;
            }

            if (mainCamera.CameraSourcePaused) return;

            backgroundCamera.projectionMatrix = Z.PipelineProjectionMatrix(ZapparCamera.Instance.GetPipeline, Screen.width, Screen.height, backgroundCamera.nearClipPlane, backgroundCamera.farClipPlane, ref m_camerModel);

            Z.PipelineCameraFrameTextureMatrix(ZapparCamera.Instance.GetPipeline, ref textureMatElements, Screen.width, Screen.height, ZapparCamera.Instance.IsMirrored);

            UpdateTextureMatrix();

            m_CameraMaterial.SetMatrix("_nativeTextureMatrix", textureMatrix);

            m_CamTexture = Z.PipelineCameraFrameTexture(ZapparCamera.Instance.GetPipeline);
            if (m_CamTexture != null)
                m_CameraMaterial.mainTexture = m_CamTexture;

        }

        private void UpdateTextureMatrix()
        {
            textureMatrix[0, 0] = textureMatElements[0];
            textureMatrix[1, 0] = textureMatElements[1];
            textureMatrix[2, 0] = textureMatElements[2];
            textureMatrix[3, 0] = textureMatElements[3];
            textureMatrix[0, 1] = textureMatElements[4];
            textureMatrix[1, 1] = textureMatElements[5];
            textureMatrix[2, 1] = textureMatElements[6];
            textureMatrix[3, 1] = textureMatElements[7];
            textureMatrix[0, 2] = textureMatElements[8];
            textureMatrix[1, 2] = textureMatElements[9];
            textureMatrix[2, 2] = textureMatElements[10];
            textureMatrix[3, 2] = textureMatElements[11];
            textureMatrix[0, 3] = textureMatElements[12];
            textureMatrix[1, 3] = textureMatElements[13];
            textureMatrix[2, 3] = textureMatElements[14];
            textureMatrix[3, 3] = textureMatElements[15];
        }

        private void OnDestroy()
        {
            textureMatElements = null;            
#if ZAPPAR_SRP
            RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
#endif
        }

    }
}