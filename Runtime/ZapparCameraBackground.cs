using System;
using UnityEngine;

namespace Zappar
{
    public class ZapparCameraBackground : MonoBehaviour
    {

        //[Tooltip("Material must have an Unlit/CameraBackgroundShader shader attached")]
        private Material m_CameraMaterial;

        private bool m_Initialised = false;
        private Texture2D m_CamTexture = null;

        private void Awake()
        {
            m_CameraMaterial = new Material(Shader.Find("Zappar/CameraBackgroundShader"));
            if(m_CameraMaterial==null)
            {
                Debug.LogError("Can't render camera texture: Missing Zappar/CameraBackgroundShader!");
            }
        }

        void Point(float x, float y)
        {
            GL.TexCoord2(x, y);
            GL.Vertex3(x, y, -1);
        }

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

        void Update()
        {
            if (!m_Initialised || m_CameraMaterial == null)
            {
                if (Z.HasInitialized())
                    m_Initialised = true;
                return;
            }

            IntPtr pipeline = ZapparCamera.Instance.GetPipeline();
            bool isMirrored = ZapparCamera.Instance.IsMirrored();

            GetComponent<Camera>().projectionMatrix = Z.PipelineProjectionMatrix(pipeline, Screen.width, Screen.height);

            Matrix4x4 textureMatrix = Z.PipelineCameraFrameTextureMatrix(pipeline, Screen.width, Screen.height, isMirrored);
            m_CameraMaterial.SetMatrix("_nativeTextureMatrix", textureMatrix);

            m_CamTexture = Z.PipelineCameraFrameTexture(pipeline);
            if (m_CamTexture != null)
                m_CameraMaterial.mainTexture = m_CamTexture;

        }
    }
}