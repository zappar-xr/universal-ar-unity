using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;

using Zappar;
public class ZapparCameraBackground : MonoBehaviour
{

    [Tooltip("Material must have an Unlit/CameraBackgroundShader shader attached")]
    public Material cameraMaterial;

    private bool m_hasInitialised = false;
    private Texture2D m_currentTexture = null;

    void Point(float x, float y)
    {
        GL.TexCoord2(x, y);
        GL.Vertex3(x, y, -1);
    }

    void OnPostRender()
    {
        cameraMaterial.SetPass(0);
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
        if (!m_hasInitialised || cameraMaterial == null)
        {
            if (Z.HasInitialized())
                m_hasInitialised = true;
            return;
        }

        IntPtr pipeline = ZapparCamera.Instance.GetPipeline();
        bool isMirrored = ZapparCamera.Instance.IsMirrored();

        GetComponent<Camera>().projectionMatrix = Z.PipelineProjectionMatrix(pipeline, Screen.width, Screen.height);

        Matrix4x4 textureMatrix = Z.PipelineCameraFrameTextureMatrix(pipeline, Screen.width, Screen.height, isMirrored);
        cameraMaterial.SetMatrix("_nativeTextureMatrix", textureMatrix);

        m_currentTexture = Z.PipelineCameraFrameTexture(pipeline);
        if (m_currentTexture != null)
            cameraMaterial.mainTexture = m_currentTexture;

    }
}
