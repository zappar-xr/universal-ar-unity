using UnityEngine;
using UnityEditor;

namespace Zappar.Editor
{
    public class ZapparMenu : MonoBehaviour
    {
        [MenuItem("Zappar/Camera")]
        static void ZapparCreateCamera()
        {
            GameObject camera = (GameObject)Resources.Load("Prefabs/Zappar Camera Rear");
            GameObject go = Instantiate(camera, Vector3.zero, Quaternion.identity);
            Undo.RegisterCreatedObjectUndo(go, "New camera added");
        }

        [MenuItem("Zappar/Face Tracker")]
        static void ZapparCreateFaceTrackingTarget()
        {
            GameObject faceTarget = (GameObject)Resources.Load("Prefabs/Zappar Face Tracker");
            GameObject go = Instantiate(faceTarget, Vector3.zero, Quaternion.identity);
            Undo.RegisterCreatedObjectUndo(go,"New face target");
        }

        [MenuItem("Zappar/Face Mesh")]
        static void ZapparCreateFaceMeshTarget()
        {
            GameObject faceMesh = (GameObject)Resources.Load("Prefabs/Zappar Face Mesh");
            GameObject go = Instantiate(faceMesh, Vector3.zero, Quaternion.identity);
            Undo.RegisterCreatedObjectUndo(go, "New face mesh");
        }

        [MenuItem("Zappar/Image Tracker")]
        static void ZapparCreateImageTrackingTarget()
        {
            GameObject imageTarget = (GameObject)Resources.Load("Prefabs/Zappar Image Tracker");
            GameObject go = Instantiate(imageTarget, Vector3.zero, Quaternion.identity);
            Undo.RegisterCreatedObjectUndo(go, "New image tracker");
        }

        [MenuItem("Zappar/Instant Tracker")]
        static void ZapparCreateInstantTrackingTarget()
        {
            GameObject instantTarget = (GameObject)Resources.Load("Prefabs/Zappar Instant Tracker");
            GameObject go = Instantiate(instantTarget, Vector3.zero, Quaternion.identity);
            Undo.RegisterCreatedObjectUndo(go, "New instant tracker");
        }

        [MenuItem("Zappar/Utilities/Full Head Model")]
        static void ZapparCreateFullHeadModel()
        {
            GameObject headModel = (GameObject)Resources.Load("Prefabs/Zappar Full Head Model");
            GameObject go = Instantiate(headModel, Vector3.zero, Quaternion.identity);
            Undo.RegisterCreatedObjectUndo(go, "New head model");
        }

        [MenuItem("Zappar/Utilities/Full Head Depth Mask")]
        static void ZapparCreateFullHeadDepthMask()
        {
            GameObject headDepthMask = (GameObject)Resources.Load("Prefabs/Zappar Face Depth Mask");
            GameObject go = Instantiate(headDepthMask, Vector3.zero, Quaternion.identity);
            Undo.RegisterCreatedObjectUndo(go, "New depth mask");
        }

        [MenuItem("Zappar/Utilities/Documentation")]
        static void ZapparOpenDocumentation()
        {
            Application.OpenURL("https://docs.zap.works/universal-ar/unity");
        }

        [MenuItem("Zappar/Editor/OpenUARSettings")]
        static void ZapparOpenUarSettings()
        {
            SettingsService.OpenProjectSettings("Project/ZapparUARSettings");
        }

        [MenuItem("Zappar/Utilities/Update Project Settings To Publish")]
        static void ZapparPublishSettings()
        {
#if UNITY_WEBGL
            // Other Settings
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP); //default is IL2CPP
            //PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.High); can be disabled for IL2CPP backend
            PlayerSettings.stripEngineCode = true;

            //Publishing settings
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.dataCaching = true;

            //Build Settings
            EditorUserBuildSettings.development = false;
#endif

            Debug.Log("Done updating project setting for publish");
        }

        [MenuItem("Zappar/Utilities/Install CLI")]
        static void ZapparOpenZapworksCLI()
        {
            Application.OpenURL("https://www.npmjs.com/package/@zappar/zapworks-cli");
        }
    }
}