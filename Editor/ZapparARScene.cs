using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Zappar.Editor
{
    [InitializeOnLoad]
    internal class ZapparARScene : UnityEditor.Editor
    {
        static ZapparARScene()
        {
            EditorSceneManager.newSceneCreated += EditorSceneManager_newSceneCreated;
        }

        private static void EditorSceneManager_newSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            if (!ZapparMenu.CreateNewARScene) return;
            ZapparMenu.CreateNewARScene = false;

            GameObject cam = Camera.main.gameObject;
            DestroyImmediate(cam);
            ZapparMenu.ZapparCreateCamera(); //create rear facing camera

            RenderSettings.skybox = null;
            RenderSettings.ambientIntensity = 0f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(150f / 255f, 150f / 255f, 150f / 255f); //ambientIntensity=0
            Lightmapping.bakedGI = false;
        }
    }
}