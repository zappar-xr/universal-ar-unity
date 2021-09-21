using System;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

namespace Zappar.Editor
{
    [CustomEditor(typeof(ZapparImageTrackingTarget))]
    public class ZapparImageTrackingTargetEditor : UnityEditor.Editor
    {

        ZapparImageTrackingTarget myScript = null;
        //--------------------

        private void OnEnable()
        {
            if (Application.isPlaying) return;
            
            var settings = AssetDatabase.LoadAssetAtPath<ZapparUARSettings>(ZapparUARSettings.MySettingsPath);
            myScript = (ZapparImageTrackingTarget)target;
            ToggleImagePreview(settings.ImageTargetPreviewEnabled);
        }

        private void ToggleImagePreview(bool enable)
        {
            if (enable)
            {
                if (!Z.HasInitialized() || myScript==null || (myScript.PreviewImagePlane != null && !RequirePreviewUpdate()))
                    return;

                myScript.m_Pipeline = Z.PipelineCreate();
                myScript.m_ImageTracker = Z.ImageTrackerCreate(myScript.m_Pipeline);
                OnZPTFilenameChange();
            }
            else
            {
                //clear the preview
                if (myScript.m_ImageTracker != IntPtr.Zero) Z.ImageTrackerDestroy(myScript.m_ImageTracker);
                if (myScript.m_Pipeline != IntPtr.Zero) Z.PipelineDestroy(myScript.m_Pipeline);

                if (myScript?.PreviewImagePlane != null)
                {
                    DestroyImmediate(myScript.PreviewImagePlane);
                    EditorUtility.SetDirty(myScript.gameObject);
                }
            }
        }

        private bool RequirePreviewUpdate()
        {
            if (myScript == null) return false;
            return !myScript.PreviewTarget.Equals(myScript.Target) || myScript.PreviewOrientation != myScript.Orientation;
        }

        private void OnZPTFilenameChange()
        {
            if (myScript==null || !myScript.gameObject.activeInHierarchy)
            {
                Debug.Log("Could not start LoadZPTTarget Coroutine as gameobject is inactive.");
                return;
            }

            if (myScript.Target == "No ZPT files available." || string.IsNullOrEmpty(myScript.Target))
                return;

            myScript.PreviewTarget = myScript.Target;
            myScript.PreviewOrientation = myScript.Orientation;

            DestroyImmediate(myScript?.PreviewImagePlane);
            EditorCoroutineUtility.StartCoroutine(Z.LoadZPTTarget(myScript.PreviewTarget, TargetDataAvailableCallback), myScript);
        }

        private void SetupImagePreview()
        {
            if (myScript==null) return;

            int previewWidth = Z.ImageTrackerTargetPreviewRgbaWidth(myScript.m_ImageTracker, 0);
            int previewHeight = Z.ImageTrackerTargetPreviewRgbaHeight(myScript.m_ImageTracker, 0);

            Debug.Log("Preview image res: " + previewWidth + "x" + previewHeight);

            if (previewWidth == 0 || previewHeight == 0)
                return;

            byte[] previewData = Z.ImageTrackerTargetPreviewRgba(myScript.m_ImageTracker, 0);

            if (myScript.PreviewImagePlane == null)
            {
                GameObject plane = null;
                for (int i = 0; i < myScript.transform.childCount; ++i)
                {
                    if (myScript.transform.GetChild(i).gameObject.name == "Preview Image")
                    {
                        plane = myScript.transform.GetChild(i).gameObject;
                        if (plane.GetComponent<MeshFilter>() == null) plane = null;
                    }
                }
                if (plane == null)
                {
                    myScript.PreviewImagePlane = GameObject.CreatePrimitive(PrimitiveType.Quad) as GameObject;
                    Undo.RegisterCreatedObjectUndo(myScript.PreviewImagePlane, "New preview object");
                    myScript.PreviewImagePlane.name = "Preview Image";
                    myScript.PreviewImagePlane.transform.SetParent(myScript.transform);
                }
                else
                {
                    myScript.PreviewImagePlane = plane;
                    EditorUtility.SetDirty(myScript.gameObject);
                }
            }

            myScript.PreviewImagePlane.transform.localEulerAngles = myScript.Orientation == ZapparImageTrackingTarget.PlaneOrientation.Flat ?
                new Vector3(90, 0, 180) : new Vector3(0,0,180);
            myScript.PreviewImagePlane.transform.localPosition = Vector3.zero;

            float aspectRatio = (float)previewWidth / (float)previewHeight;
            const float scaleFactor = 2f;   // check for better estimator than rough scaling
            myScript.PreviewImagePlane.transform.localScale = new Vector3(aspectRatio, 1.0f, 1.0f) * scaleFactor;

            Texture2D texture = new Texture2D(previewWidth, previewHeight, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(previewData);
            texture.Apply();

            Material material = new Material(Shader.Find("Unlit/Texture"));
#if ZAPPAR_SRP
            //material.SetTextureScale("_MainTex", new Vector2(-1, 1));
            material.mainTexture = texture;
            Vector3 scale = myScript.PreviewImagePlane.transform.localScale;
            myScript.PreviewImagePlane.transform.localScale = new Vector3(scale.x * -1, scale.y, scale.z);
#else
            material.SetTextureScale("_MainTex", new Vector2(-1, 1));
            material.mainTexture = texture;
#endif
            myScript.PreviewImagePlane.GetComponent<Renderer>().material = material;
        }

        private void TargetDataAvailableCallback(byte[] data)
        {
            if (myScript.m_ImageTracker!=IntPtr.Zero)
            {
                Z.ImageTrackerTargetLoadFromMemory(myScript.m_ImageTracker, data);
                SetupImagePreview();
            }
            else
            {
                Debug.LogError("No image tracker found to enable preview");
            }
        }
    }
}