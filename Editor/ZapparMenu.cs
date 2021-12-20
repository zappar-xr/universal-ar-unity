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

        public delegate void PackageListUpdated(ListRequest request);
        public static PackageListUpdated OnPackageListUpdated;

#region UtilitiesMenu
        [MenuItem("Zappar/Utilities/Full Head Model", false, 100)]
        public static void ZapparCreateFullHeadModel()
        {
            GameObject head = ZAssistant.GetZapparFullHeadModel();
            Undo.RegisterCreatedObjectUndo(head, "New head model");
            Selection.activeGameObject = head;
        }

        [MenuItem("Zappar/Utilities/Full Head Depth Mask", false, 100)]
        public static void ZapparCreateFullHeadDepthMask()
        {
            GameObject head = ZAssistant.GetZapparFullHeadDepthMask();
            Undo.RegisterCreatedObjectUndo(head, "New depth mask");
            Selection.activeGameObject = head;
        }

        [MenuItem("Zappar/Utilities/Documentation", false, 120)]
        public static void ZapparOpenDocumentation()
        {
            Application.OpenURL("https://docs.zap.works/universal-ar/unity");
        }

        [MenuItem("Zappar/Utilities/Install CLI", false, 120)]
        public static void ZapparOpenZapworksCLI()
        {
            Application.OpenURL("https://www.npmjs.com/package/@zappar/zapworks-cli");
        }

        #endregion

        #region EditorMenu

        [MenuItem("Zappar/Editor/Open Universal AR Settings", false, 15)]
        public static void ZapparOpenUarSettings()
        {
            SettingsService.OpenProjectSettings("Project/ZapparUARSettings");
        }

#if ZAPPAR_SRP
        [MenuItem("Zappar/Editor/Update Project For StandardPipeline", false, 1)]
        public static void ZapparUpdateProjectToNonSRP()
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

        [MenuItem("Zappar/Editor/Update Zappar Scene For SRP", false, 2)]
        public static void ZapparUpdateSceneToSRP()
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
        public static void ZapparUpdateProjectToSRP()
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

        [MenuItem("Zappar/Editor/Update Project Settings To Publish", false, 3)]
        public static void ZapparPublishSettings()
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

        [MenuItem("Zappar/Editor/Re-Import Universal AR Git Package", false, 28)]
        public static void ReimportUARPackage()
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

        #endregion

        #region CameraMenu

        [MenuItem("Zappar/Camera/Front Facing Camera", false, 30)]
        public static void ZapparCreateUserCamera()
        {
            GameObject go = ZAssistant.GetZapparCamera(true);
            UpdateZapparCamera(go);
        }

        [MenuItem("Zappar/Camera/Rear Facing Camera", false, 30)]
        public static void ZapparCreateCamera()
        {
            GameObject go = ZAssistant.GetZapparCamera(false);
            UpdateZapparCamera(go);
        }

        private static void UpdateZapparCamera(GameObject go)
        {
            Undo.RegisterCreatedObjectUndo(go, "New camera added");

            var settings = AssetDatabase.LoadAssetAtPath<ZapparUARSettings>(ZapparUARSettings.MySettingsPathInPackage);
            if (settings.EnableRealtimeReflections)
            {
                GameObject rp = new GameObject("ZReflectionProbe");
                rp.transform.SetParent(go.transform);
                rp.AddComponent<ZapparReflectionProbe>();
            }

#if ZAPPAR_SRP
            ZapparUpdateSceneToSRP();
#endif
            Selection.activeGameObject = go;
        }

        #endregion

        #region FaceMenu

        [MenuItem("Zappar/Face Tracker/Face Tracking Target", false, 30)]
        public static void ZapparCreateFaceTrackingTarget()
        {
            GameObject go = ZAssistant.GetZapparMultiFaceTrackingTarget();
            Undo.RegisterCreatedObjectUndo(go, "New face target");
            Selection.activeGameObject = go;
        }

        [MenuItem("Zappar/Face Tracker/Face Mesh", false, 31)]
        public static void ZapparCreateFaceMeshTarget()
        {
            GameObject go = ZAssistant.GetZapparFaceMeshTarget();
            Undo.RegisterCreatedObjectUndo(go, "New face mesh");
            Selection.activeGameObject = go;
        }

        [MenuItem("Zappar/Face Tracker/Face Landmark", false, 32)]
        public static void ZapparCreateFaceLandmark()
        {
            GameObject go = ZAssistant.GetZapparFaceLandmark();
            Undo.RegisterCreatedObjectUndo(go, "New face landmark");
            Selection.activeGameObject = go;
        }

        #endregion

        #region ImageMenu

        [MenuItem("Zappar/Image Tracking Target", false, 31)]
        public static void ZapparCreateImageTrackingTarget()
        {
            GameObject go = ZAssistant.GetZapparImageTrackingTarget();
            Undo.RegisterCreatedObjectUndo(go, "New image tracker");
            Selection.activeGameObject = go;
        }

        #endregion

        #region InstantMenu

        [MenuItem("Zappar/Instant Tracking Target", false, 33)]
        public static void ZapparCreateInstantTrackingTarget()
        {
            GameObject go = ZAssistant.GetZapparInstantTrackingTarget();
            Undo.RegisterCreatedObjectUndo(go, "New instant tracker");
            Selection.activeGameObject = go;
        }

        #endregion

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

        [MenuItem("Zappar/Realtime Reflection Probe", false, 34)]
        public static void CreateZapparReflectionProbe()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ZapparUARSettings>(ZapparUARSettings.MySettingsPathInPackage);
            if (!settings.EnableRealtimeReflections)
            {
                Debug.Log("Please enable the realtime reflection from UAR settings! Zappar/Editor/Open Universal AR Settings");
                return;
            }
            ZapparCamera zCam = GameObject.FindObjectOfType<ZapparCamera>();
            if (zCam==null)
            {
                Debug.LogError("Can't find zappar camera in scene!");
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
        public static void InvertSelectedMeshSurface(MenuCommand menuCommand)
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