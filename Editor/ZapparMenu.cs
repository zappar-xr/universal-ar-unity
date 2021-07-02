using UnityEngine;
using UnityEditor;
#if ZAPPAR_SRP
using UnityEngine.Rendering.Universal;
#endif

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
#if ZAPPAR_SRP
            ZapparUpdateSceneToSRP();
#endif
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

#if ZAPPAR_SRP
        [MenuItem("Zappar/Editor/Update Project For StandardPipeline")]
        static void ZapparUpdateProjectToNonSRP()
        {
            Debug.Log("Updating zappar to use standard pipeline");
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
            const string zapparSrp = "ZAPPAR_SRP";
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

            if (symbols.Contains(zapparSrp))
            {
                symbols = symbols.Remove(symbols.IndexOf(zapparSrp), 10);
            }
            else
            {
                Debug.Log("Standard pipeline is zappar is already set");
                return;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, symbols);
        }

        [MenuItem("Zappar/Editor/Update Zappar Scene For SRP")]
        static void ZapparUpdateSceneToSRP()
        {
            Debug.Log("Updating current scene to use Unity-SRP");
            Camera uCam = GameObject.FindObjectOfType<ZapparCamera>().gameObject.GetComponent<Camera>();
            Camera bCam = GameObject.FindObjectOfType<ZapparCameraBackground>().gameObject.GetComponent<Camera>();

            EditorGUI.BeginChangeCheck();

            if (uCam != null && bCam != null)
            {
                var camData = uCam.GetUniversalAdditionalCameraData();
                camData.renderType = CameraRenderType.Overlay;
                Undo.RecordObject(uCam, "updated overlay camera");

                camData = bCam.GetUniversalAdditionalCameraData();
                camData.renderType = CameraRenderType.Base;
                camData.renderShadows = false;
                bCam.depth = -1;
                bCam.useOcclusionCulling = false;
                camData.cameraStack.Clear();
                camData.cameraStack.Add(uCam);
                Undo.RecordObject(bCam, "update background camera");

                EditorUtility.SetDirty(uCam.gameObject);
                EditorUtility.SetDirty(bCam.gameObject);
            }
            else
            {
                Debug.Log("Zappar camera not properly configured");
                return;
            }
        }
#else
        [MenuItem("Zappar/Editor/Update Project For SRP")]
        static void ZapparUpdateProjectToSRP()
        {
            Debug.Log("Updating zappar project to use Unity-SRP");
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
            const string zapparSrp = "ZAPPAR_SRP";
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

            if (symbols.Contains(zapparSrp))
            {
                Debug.Log("Zappar SRP is already set");
                return;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, symbols + ";" + zapparSrp);
        }
#endif


        [MenuItem("Zappar/Editor/Update Project Settings To Publish")]
        static void ZapparPublishSettings()
        {
#if UNITY_WEBGL
            // Other Settings
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP); //default is IL2CPP
            PlayerSettings.stripEngineCode = true;

            //Publishing settings
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.dataCaching = true;

            //Build Settings
            EditorUserBuildSettings.development = false;
#if UNITY_2020_1_OR_NEWER
            PlayerSettings.WebGL.template = "Zappar2020";
#elif UNITY_2018_1_OR_NEWER
            PlayerSettings.WebGL.template = "Zappar";
#else
            Debug.LogError("Please upgrade to newer versions of Unity");
#endif
#else
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.High);
            PlayerSettings.stripEngineCode = true;

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