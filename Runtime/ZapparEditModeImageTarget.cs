using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Zappar;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class ZapparEditModeImageTarget : MonoBehaviour
{
    private bool initialized = false;
    private IntPtr pipeline;
    private IntPtr imageTracker;
    private String previewImageName = "Preview Image";

    //[HideInInspector]
    public string currentFilename;

    void OnEnable()
    {
        // todo handle init better
        if (!Z.HasInitialized())
            return;

        pipeline = Z.PipelineCreate();
        imageTracker = Z.ImageTrackerCreate(pipeline);
        initialized = true;
        OnZPTFilenameChange();

    }

    public void OnZPTFilenameChange()
    {
        if (!gameObject.activeInHierarchy){
            Debug.Log("Could not start LoadZPTTarget Coroutine as gameobject is inactive.");
            return;
        }
        currentFilename = GetComponent<ZapparImageTrackingTarget>().Target;
        if (currentFilename == "No ZPT files available.")
            return;
        StartCoroutine(Z.LoadZPTTarget(currentFilename, TargetDataAvailableCallback));
    }

    private void SetupImagePreview()
    {
        Debug.Log("setting image preview: " + initialized);
        if (!initialized) return;

        int previewWidth = Z.ImageTrackerTargetPreviewRgbaWidth(imageTracker, 0);
        int previewHeight = Z.ImageTrackerTargetPreviewRgbaHeight(imageTracker, 0);
        
        Debug.Log("Preview image res: " + previewWidth + "x" + previewHeight);

        if (previewWidth == 0 || previewHeight == 0)
            return;

        byte[] previewData = Z.ImageTrackerTargetPreviewRgba(imageTracker, 0);

        GameObject previewImage = GameObject.Find(previewImageName);
        GameObject mesh;
        if (previewImage != null)
            mesh = previewImage;
        else
            mesh = GameObject.CreatePrimitive(PrimitiveType.Quad) as GameObject;
        mesh.name = previewImageName;
        mesh.transform.SetParent(transform);

        float aspectRatio = (float)previewWidth / (float)previewHeight;
        mesh.transform.localScale = new Vector3(aspectRatio * 0.2f, 1.0f * 0.2f, 1.0f * 0.2f);

        Texture2D texture = new Texture2D(previewWidth, previewHeight, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(previewData);
        texture.Apply();

        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.SetTextureScale("_MainTex", new Vector2(-1,1));
        material.mainTexture = texture;

        mesh.GetComponent<Renderer>().material = material;
    }

    private void TargetDataAvailableCallback(byte[] data)
    {
        if (initialized) {
            Z.ImageTrackerTargetLoadFromMemory(imageTracker, data);
            SetupImagePreview();
        }
    }
}

#endif
