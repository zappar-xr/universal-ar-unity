using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.Rendering;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
#if ZAPPAR_SRP
using UnityEngine.Rendering.Universal;
#endif

namespace Zappar.Editor
{
    public class ZapparMenu : MonoBehaviour
    {
        private static ListRequest s_packageListRequest = null;
        private static AddRequest s_importRequest = null;
        private delegate void PackageListUpdated(ListRequest request);
        private static PackageListUpdated OnPackageListUpdated;

        [MenuItem("Zappar/Camera")]
        static void ZapparCreateCamera()
        {
            GameObject camera = (GameObject)Resources.Load("Prefabs/Zappar Camera Rear");
            GameObject go = Instantiate(camera, Vector3.zero, Quaternion.identity);
            Undo.RegisterCreatedObjectUndo(go, "New camera added");

            var settings = AssetDatabase.LoadAssetAtPath<ZapparUARSettings>(ZapparUARSettings.MySettingsPath);
            if(settings.EnableRealtimeReflections)
            {
                GameObject rp = new GameObject("ZReflectionProbe");
                rp.transform.SetParent(go.transform);
                rp.AddComponent<ZapparReflectionProbe>();
            }

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

        [MenuItem("Zappar/Utilities/Full Head Model", false, 0)]
        static void ZapparCreateFullHeadModel()
        {
            GameObject headModel = (GameObject)Resources.Load("Prefabs/Zappar Full Head Model");
            GameObject go = Instantiate(headModel, Vector3.zero, Quaternion.identity);
            Undo.RegisterCreatedObjectUndo(go, "New head model");
        }

        [MenuItem("Zappar/Utilities/Full Head Depth Mask", false, 0)]
        static void ZapparCreateFullHeadDepthMask()
        {
            GameObject headDepthMask = (GameObject)Resources.Load("Prefabs/Zappar Face Depth Mask");
            GameObject go = Instantiate(headDepthMask, Vector3.zero, Quaternion.identity);
            Undo.RegisterCreatedObjectUndo(go, "New depth mask");
        }

        [MenuItem("Zappar/Utilities/Documentation", false, 0)]
        static void ZapparOpenDocumentation()
        {
            Application.OpenURL("https://docs.zap.works/universal-ar/unity");
        }

        [MenuItem("Zappar/Editor/Open Universal AR Settings", false, 1)]
        static void ZapparOpenUarSettings()
        {
            SettingsService.OpenProjectSettings("Project/ZapparUARSettings");
        }

#if ZAPPAR_SRP
        [MenuItem("Zappar/Editor/Update Project For StandardPipeline", false, 1)]
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

        [MenuItem("Zappar/Editor/Update Zappar Scene For SRP", false, 1)]
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
        [MenuItem("Zappar/Editor/Update Project For SRP", false, 1)]
        static void ZapparUpdateProjectToSRP()
        {
            Debug.Log("Updating zappar project to use Unity-SRP");
            const string zapparSrp = "ZAPPAR_SRP";
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

            if (symbols.Contains(zapparSrp))
            {
                Debug.Log("Zappar SRP is already set");
                return;
            }

            //Check if Universal Rendering package has been imported already
            s_packageListRequest = Client.List(true, true);
            OnPackageListUpdated = UnityRenderPipelineCheck;
            EditorApplication.update += PackageProgress;
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, symbols + ";" + zapparSrp);
            Debug.Log("Done!");
        }
#endif

        private static void UnityRenderPipelineCheck(ListRequest request)
        {
            if (request.Status == StatusCode.Success)
            {
                bool avail = false;
                foreach (var pack in request.Result)
                {
                    if (pack.packageId.ToLower().Contains("com.unity.render-pipelines.universal"))
                    { avail = true; break; }
                }

                if (!avail)
                {
                    //Raise dialog to import URP
                    EditorUtility.DisplayDialog("Zappar Notification", "Missing Unity Universal Rendering Pipeline package! Please add the package to your project!", "OK");
                }
            }
            else if (request.Status >= StatusCode.Failure)
            {
                Debug.LogError("Failed to check for Universal Rendering package. Error: " + request.Error.message);
            }
        }

        private static void PackageProgress()
        {
            if(s_packageListRequest!= null && s_packageListRequest.IsCompleted)
            {
                EditorApplication.update -= PackageProgress;
                OnPackageListUpdated.Invoke(s_packageListRequest);
                s_packageListRequest = null;
            }

            if (s_importRequest != null)
            {
                if (s_importRequest.Status == StatusCode.Failure)
                {
                    Debug.Log("Import ("+ s_importRequest.Result?.packageId + ") failed: " + s_importRequest.Error.message);
                    EditorApplication.update -= PackageProgress;
                    s_importRequest = null;
                }

                if (s_importRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Finished importing: " + s_importRequest.Result?.packageId);
                    EditorApplication.update -= PackageProgress;
                    s_importRequest = null;
                    PackageImportSettings.RefreshPackage();
                }
            }
        }

        [MenuItem("Zappar/Editor/Update Project Settings To Publish", false, 1)]
        static void ZapparPublishSettings()
        {
#if UNITY_WEBGL
            ZAssistant.UpdateUnityProjectSettings(ZProjectSettingsConfig.WebGLAll);
#elif UNITY_ANDROID
            ZAssistant.UpdateUnityProjectSettings(ZProjectSettingsConfig.AndroidAll);
#elif UNITY_IOS
            ZAssistant.UpdateUnityProjectSettings(ZProjectSettingsConfig.IosAll);
#endif
            Debug.Log("Done updating editor related project settings for publish");
        }

        [MenuItem("Zappar/Editor/Re-Import UAR Git Package", false, 3)]
        static void ReimportUARPackage()
        {
            s_packageListRequest = Client.List(true, true);
            OnPackageListUpdated = StartUARReimport;
            EditorApplication.update += PackageProgress;
        }

        private static void StartUARReimport(ListRequest request)
        {
            if (request.Status == StatusCode.Success)
            {
                bool avail = false;
                string packageId = "";
                foreach (var pack in request.Result)
                {
                    if (pack.packageId.ToLower().Contains("com.zappar.uar"))
                    { avail = true; packageId = pack.packageId; break; }
                }

                if (avail)
                {

                    s_importRequest = UnityEditor.PackageManager.Client.Add(packageId);
                    Debug.Log("Reimporting: " + packageId);
                    EditorApplication.update += PackageProgress;
                }
                else
                {
                    EditorUtility.DisplayDialog("Zappar Notification", "No Universal AR package found!", "OK");
                }
            }
            else if (request.Status >= StatusCode.Failure)
            {
                Debug.LogError("Failed to check for Universal AR zappar package. Error: " + request.Error.message);
            }
        }

        [MenuItem("Zappar/Utilities/Install CLI", false, 0)]
        static void ZapparOpenZapworksCLI()
        {
            Application.OpenURL("https://www.npmjs.com/package/@zappar/zapworks-cli");
        }

        [MenuItem("GameObject/Zappar/Add Realtime Reflection Probe", false, 10)]
        static void CreateZapparReflectionProbe(MenuCommand menuCommand)
        {
            var settings = AssetDatabase.LoadAssetAtPath<ZapparUARSettings>(ZapparUARSettings.MySettingsPath);
            if (!settings.EnableRealtimeReflections)
            {
                Debug.Log("Please enable the realtime reflection from UAR settings! Zappar/Editor/OpenUARSettings");
                return;
            }

            GameObject parent = menuCommand.context as GameObject;
            if(parent?.GetComponent<ZapparCamera>()==null)
            {
                Debug.LogError("Can't find zappar camera componnet on parent");
                return;
            }

            if(GameObject.FindObjectOfType<ZapparReflectionProbe>()!=null)
            {
                Debug.Log("A Reflection probe component is already present");
                Selection.activeGameObject = FindObjectOfType<ZapparReflectionProbe>().gameObject;
                return;
            }

            GameObject go = new GameObject("ZReflectionProbe");
            go.AddComponent<ZapparReflectionProbe>();
            //go.transform.SetParent(parent.transform);
            Undo.RegisterCreatedObjectUndo(go, "Zappar reflection probe added");
            Selection.activeObject = go;

            EditorUtility.SetDirty(go);
        }

        [MenuItem("GameObject/Zappar/Invert Mesh Surface", false, 10)]
        static void InvertSelectedMeshSurface(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            
            MeshFilter meshF = parent?.GetComponent<MeshFilter>();

            if (meshF == null || meshF.sharedMesh == null)
            {
                Debug.Log("No mesh filter or instantiated mesh found on object");
                return;
            }

            Mesh mesh = (Mesh)Instantiate(meshF.sharedMesh);
            ZapparUtilities.InvertMeshSurface(ref mesh);
            string name = mesh.name;
            name = name.EndsWith("_Inverted") ? name : name + "_Inverted";
            mesh.name = name;
            meshF.sharedMesh = mesh;

            EditorUtility.SetDirty(parent);
        }
    }
}