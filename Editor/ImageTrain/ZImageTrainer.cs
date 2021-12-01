using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.IO;
using System.Runtime.InteropServices;

namespace Zappar.Editor
{
    [Serializable]
    public class ZImageTrainerParams
    {
        public string ImagePath = "";
        public int MaxWidth = 512;
        public int MaxHeight = 512;
        public bool AllowOverwrite = false;
        public bool EncodePreviewImage = true;
    }

    public class ZImageTrainer : EditorWindow
    {
        [SerializeField]
        private static ZImageTrainerParams s_myParams = null;
        private static bool s_showAdvanced = false;
        private static string s_zptPath;
        private bool m_trainingInProgress = false;

        private void OnEnable()
        {
            s_zptPath = Application.streamingAssetsPath;
        }

        [MenuItem("Zappar/Editor/Open Image Trainer", false, 1)]
        private static void InitializeTrainerWindows()
        {
            ZImageTrainer window = (ZImageTrainer)EditorWindow.GetWindow(typeof(ZImageTrainer));
            window.titleContent = new GUIContent() { text = "Zappar Image Trainer" };

            if (s_myParams == null)
            {
                s_myParams = new ZImageTrainerParams();
            }

            window.Show();
        }

        private void OnGUI()
        {
            if (s_myParams == null) s_myParams = new ZImageTrainerParams();

            GUILayout.Label("Image Trainer Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            s_myParams.ImagePath = EditorGUILayout.TextField("Source image path", s_myParams.ImagePath);
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                EditorGUI.FocusTextInControl("");
                string dir = s_myParams.ImagePath.Length > 0 ? Path.GetDirectoryName(s_myParams.ImagePath) : Application.dataPath;
                string path = EditorUtility.OpenFilePanel("Select an image", dir, "png,jpg,jpeg");
                if (path.Length > 0)
                {
                    s_myParams.ImagePath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            s_myParams.AllowOverwrite = EditorGUILayout.Toggle("Allow ZPT overwrite", s_myParams.AllowOverwrite);
            s_myParams.EncodePreviewImage = EditorGUILayout.Toggle("Add image preview", s_myParams.EncodePreviewImage);

            if (s_showAdvanced)
            {
                EditorGUILayout.Space(15);
                GUILayout.Label("Image train parameters", EditorStyles.label);
                EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                s_myParams.MaxWidth = EditorGUILayout.IntField("Max Width", s_myParams.MaxWidth);
                s_myParams.MaxHeight = EditorGUILayout.IntField("Max Height", s_myParams.MaxHeight);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(15);

            if (GUILayout.Button((s_showAdvanced ? "Hide" : "Show") + " advanced settings"))
            {
                s_showAdvanced = !s_showAdvanced;
            }

            EditorGUI.BeginDisabledGroup(m_trainingInProgress);
            if (GUILayout.Button("Start"))
            {
                m_trainingInProgress = true;
                string zptFile = GetZptFileFullPath();
                if (FileWriteCheck(zptFile))
                {
                    EditorCoroutineUtility.StartCoroutine(StartTraining(), this);
                }
                else
                {
                    Debug.LogError("File already exists! Please enable overwrite option if you wish to replace the older version.\n" + zptFile);
                    EditorUtility.DisplayDialog("Zappar Notification", "File already exists! Please enable overwrite option if you wish to replace the older version.\n" + zptFile, "OK");
                    m_trainingInProgress = false;
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        IEnumerator StartTraining()
        {
            if (string.IsNullOrEmpty(s_myParams.ImagePath) || !File.Exists(s_myParams.ImagePath))
            {
                m_trainingInProgress = false;
                Debug.LogError("Invalid image path: " + s_myParams.ImagePath);
                EditorUtility.DisplayDialog("Zappar Notification", "Invalid image path\n" + s_myParams.ImagePath, "OK");
                yield break;
            }

            Debug.Log("Starting image train process");

            yield return new WaitForEndOfFrame();
            byte[] data = File.ReadAllBytes(s_myParams.ImagePath);
            Z.FileData src = new Z.FileData();
            Z.FileData zpt = new Z.FileData();
            Z.FileData preview = new Z.FileData();
            //Debug.Log("source image data len: " + data.Length);

            src.length = data.Length;
            src.data = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * data.Length);
            Marshal.Copy(data, 0, src.data, data.Length);

            yield return new WaitForEndOfFrame();

            if (s_myParams.EncodePreviewImage)
            {
                preview.data = src.data;
                preview.length = src.length;
            }

            if (Z.TrainImageCompressedWithMax(ref src, ref zpt, ref preview, FileIsJpg(s_myParams.ImagePath) ? 1 : 0, s_myParams.MaxWidth, s_myParams.MaxHeight) == 1)
            {
                //Debug.Log("Success in training image");

                yield return new WaitForEndOfFrame();

                if (zpt.data != null && zpt.length > 0)
                {
                    byte[] zdata = new byte[zpt.length];
                    Marshal.Copy(zpt.data, zdata, 0, zpt.length);

                    string zptFile = GetZptFileFullPath();
                    File.WriteAllBytes(zptFile, zdata);

                    Debug.Log("zpt saved at: " + zptFile);
                    EditorUtility.DisplayDialog("Zappar Notification", "Finished training.\n ZPT saved at: " + zptFile, "OK");
                }
                else
                {
                    Debug.Log("No zpt file generated!");
                    EditorUtility.DisplayDialog("Zappar Notification", "No zpt file generated!", "OK");
                }

            }
            else
            {
                Debug.LogError("Failed to train image");
                EditorUtility.DisplayDialog("Zappar Notification", "Failed to train with image!", "OK");
            }

            yield return new WaitForEndOfFrame();

            if (zpt.data != null && zpt.length > 0)
                Z.TrainImageFreeFileData(ref zpt);

            Marshal.FreeHGlobal(src.data);

            m_trainingInProgress = false;
            Debug.Log("Finished image train process");
        }

        private static bool FileWriteCheck(string filePath)
        {
            return s_myParams.AllowOverwrite || !File.Exists(filePath);
        }

        private static string GetZptFileFullPath()
        {
            string filename = Path.GetFileNameWithoutExtension(s_myParams.ImagePath) + ".zpt";
            string zptFile = Path.Combine(s_zptPath, filename);
            return zptFile;
        }

        private static bool FileIsJpg(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            return (ext == "jpg") || (ext == "jpeg");
        }
    }
}