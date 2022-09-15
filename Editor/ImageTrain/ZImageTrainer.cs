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
        public float AspectRatio = 1f;
        public int PixelWidth = 0;
        public int PixelHeight = 0;
        public float PhysicalWidth = 0f;
        public float PhysicalHeight = 0f;
        public float DPI = 0f;
        public int PhysicalUnitType = 0;  //px, mm, cm, inch, m
        public int LengthUnitType = 0;  //default(odle), mm, cm, inch, m
        public float TopRadius = 0f;
        public float BottomRadius = 0f;
        public float SideLength = 0f;
        public float PhysicalScaleFactor = 1f;
        public int MaxTrainWidth = 512;
        public int MaxTrainHeight = 512;
        public bool CurvedImageTarget = false;
        public bool AllowOverwrite = false;
        public bool EncodePreviewImage = true;
    }

    internal static class ZStyles
    {
        public static GUIContent Radius = new GUIContent("Cylindrical radius", "Radius, provided 1 unit equals half the height of image in default unit");
        public static GUIContent PhysicalSize = new GUIContent("Physical Size (WxH)", "Physical size of the image. Leave as default if not sure.");
        public static GUIContent DPI = new GUIContent("DPI", "Dots per inch. Used to estimate physical scale factor (automatically calculated if physical size in real unit is provided).");
        public static GUIContent TopRadius = new GUIContent("Top radius", "Top radius, provided 1 unit equals half the height of image in default unit");
        public static GUIContent BottomRadius = new GUIContent("Bottom radius", "Bottom radius, provided 1 unit equals half the height of image in default unit");
        public static GUIContent SideLength = new GUIContent("Side length", "Side length of conical target, provided 1 unit equals half the height of image in default unit");
        public static GUIContent InfoButton = new GUIContent(EditorGUIUtility.IconContent("console.infoicon.sml").image, "Click Me! To learn about curved training parameters.");
        public static Vector3[] RectanglePoly = new Vector3[] {
                            new Vector3(10, 10),
                            new Vector3(10, 80),
                            new Vector3(50, 80),
                            new Vector3(50, 10),
                            new Vector3(10,10)
                        };
        public static Vector3[] ConePoly = new Vector3[] {
                            new Vector3(30, 10),
                            new Vector3(50, 80),
                            new Vector3(10, 80),
                            new Vector3(30,10)
                        };
    }

    public class ZImageTrainer : EditorWindow
    {
        [SerializeField]
        private static ZImageTrainerParams s_myParams = null;
        private static bool s_cylindricalImageTarget = true;
        private static bool s_showAdvanced = false;
        private static string s_zptPath;
        private bool m_trainingInProgress = false;

        private Vector2 m_scrollPos;

        private void OnEnable()
        {
            s_zptPath = Application.streamingAssetsPath;
        }

        [MenuItem("Zappar/Editor/Open Image Trainer", false, 14)]
        private static void InitializeTrainerWindows()
        {
            ZImageTrainer window = (ZImageTrainer)EditorWindow.GetWindow(typeof(ZImageTrainer));
            window.titleContent = new GUIContent() { text = "Zappar Image Trainer" };
            window.minSize = new Vector2(400, 500);
            if (s_myParams == null)
            {
                s_myParams = new ZImageTrainerParams();
            }

            window.Show();
        }

        private void OnGUI()
        {
            if (s_myParams == null) s_myParams = new ZImageTrainerParams();

            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
            GUILayout.Label("Image Trainer Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            s_myParams.ImagePath = EditorGUILayout.TextField("Source image path", s_myParams.ImagePath);
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                EditorCoroutineUtility.StartCoroutine(ReadImageFromFile(), this);
                EditorGUI.FocusTextInControl("");
                string dir = s_myParams.ImagePath.Length > 0 ? Path.GetDirectoryName(s_myParams.ImagePath) : Application.dataPath;
                string path = EditorUtility.OpenFilePanel("Select an image", dir, "png,jpg,jpeg");
                if (path.Length > 0)
                {
                    s_myParams.ImagePath = path;
                    s_myParams.DPI = -1f;
                }
            }
            EditorGUILayout.EndHorizontal();

            s_myParams.AllowOverwrite = EditorGUILayout.Toggle("Allow ZPT overwrite", s_myParams.AllowOverwrite);
            s_myParams.EncodePreviewImage = EditorGUILayout.Toggle("Add image preview", s_myParams.EncodePreviewImage);
            // TODO: additional preview image resolution and channel optimization option
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(ZStyles.PhysicalSize);
            EditorGUI.BeginChangeCheck();
            s_myParams.PhysicalWidth = EditorGUILayout.FloatField(s_myParams.PhysicalWidth);
            s_myParams.PhysicalHeight = s_myParams.PhysicalWidth / s_myParams.AspectRatio;
            EditorGUILayout.LabelField("x", GUILayout.Width(10));
            s_myParams.PhysicalHeight = EditorGUILayout.FloatField(s_myParams.PhysicalHeight);
            s_myParams.PhysicalWidth = s_myParams.PhysicalHeight * s_myParams.AspectRatio;
            s_myParams.PhysicalUnitType = EditorGUILayout.Popup(s_myParams.PhysicalUnitType, new string[] { "px", "mm", "cm", "inch", "m" }, GUILayout.Width(50));
            if (EditorGUI.EndChangeCheck())
            {
                s_myParams.DPI = CalculateDPI(s_myParams);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(s_myParams.PhysicalUnitType != 0);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(ZStyles.DPI);
            s_myParams.DPI = EditorGUILayout.FloatField(s_myParams.DPI);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            s_myParams.CurvedImageTarget = EditorGUILayout.Toggle("Curved image target", s_myParams.CurvedImageTarget);

            if (s_myParams.CurvedImageTarget)
            {
                if (s_myParams.PhysicalUnitType == 0)
                    EditorGUILayout.HelpBox("You may want to define physical size of image target for correct scaling!", MessageType.Warning);

                EditorGUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(s_cylindricalImageTarget);
                if (GUILayout.Button("Cylindrical Target"))
                {
                    s_cylindricalImageTarget = true;
                    s_myParams.BottomRadius = s_myParams.TopRadius;
                    s_myParams.SideLength = -1f; //side length for cylindrical target is always 2. default=-1
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(!s_cylindricalImageTarget);
                if (GUILayout.Button("Conical Target")) { s_cylindricalImageTarget = false; }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Curve parameters", EditorStyles.boldLabel);
                if (GUILayout.Button(ZStyles.InfoButton, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                {
                    Application.OpenURL("https://forum.zap.works/t/get-started-with-curved-tracking-in-zapworks-studio/8703");
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                Rect rect = EditorGUILayout.GetControlRect(false, 100);
                //Rect rect = GUILayoutUtility.GetRect(100, 100);
                if (Event.current.type == EventType.Repaint)
                {
                    GUI.BeginClip(rect);
                    Handles.color = Color.red;
                    Handles.DrawPolyLine(s_cylindricalImageTarget ? ZStyles.RectanglePoly : ZStyles.ConePoly);
                    GUI.EndClip();
                }

                if (s_cylindricalImageTarget)
                {
                    EditorGUILayout.BeginHorizontal();
                    s_myParams.TopRadius = s_myParams.BottomRadius = EditorGUILayout.FloatField(ZStyles.Radius, s_myParams.TopRadius);
                    s_myParams.LengthUnitType = EditorGUILayout.Popup(s_myParams.LengthUnitType, new string[] { "default", "mm", "cm", "inch", "m" }, GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    s_myParams.TopRadius = EditorGUILayout.FloatField(ZStyles.TopRadius, s_myParams.TopRadius);
                    s_myParams.LengthUnitType = EditorGUILayout.Popup(s_myParams.LengthUnitType, new string[] { "default", "mm", "cm", "inch", "m" }, GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    s_myParams.BottomRadius = EditorGUILayout.FloatField(ZStyles.BottomRadius, s_myParams.BottomRadius);
                    s_myParams.LengthUnitType = EditorGUILayout.Popup(s_myParams.LengthUnitType, new string[] { "default", "mm", "cm", "inch", "m" }, GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    s_myParams.SideLength = EditorGUILayout.FloatField(ZStyles.SideLength, s_myParams.SideLength);
                    s_myParams.LengthUnitType = EditorGUILayout.Popup(s_myParams.LengthUnitType, new string[] { "default", "mm", "cm", "inch", "m" }, GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }

            if (s_showAdvanced)
            {
                EditorGUILayout.Space(5);
                GUILayout.Label("Image train parameters", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                s_myParams.MaxTrainWidth = EditorGUILayout.IntField("Max Width", s_myParams.MaxTrainWidth);
                s_myParams.MaxTrainHeight = EditorGUILayout.IntField("Max Height", s_myParams.MaxTrainHeight);
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
            EditorGUILayout.EndScrollView();
        }

        IEnumerator ReadImageFromFile()
        {
            if (string.IsNullOrEmpty(s_myParams.ImagePath) || !File.Exists(s_myParams.ImagePath))
            {
                yield break;
            }

            yield return new WaitForEndOfFrame();
            byte[] data = File.ReadAllBytes(s_myParams.ImagePath);
            Texture2D tempTex = new Texture2D(2, 2);
            if (tempTex.LoadImage(data))
            {
                s_myParams.PhysicalWidth = s_myParams.PixelWidth = tempTex.width;
                s_myParams.PhysicalHeight = s_myParams.PixelHeight = tempTex.height;
                s_myParams.AspectRatio = tempTex.width / (float)tempTex.height;
                s_myParams.DPI = 0;
                s_myParams.PhysicalUnitType = 0;    //px
                s_myParams.PhysicalScaleFactor = -1;
                s_myParams.LengthUnitType = 0; //default(odle)
                s_myParams.TopRadius = s_myParams.BottomRadius = s_myParams.SideLength = -1f;
            }
            else
            {
                Debug.LogError("Failed to load image data to unity texture2D");
                yield break;
            }
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

            float trainTopRadius, trainBottomRadius, trainSideLength;

            s_myParams.PhysicalScaleFactor = (s_myParams.PhysicalUnitType == 0) ? -1f : (0.5f * s_myParams.PixelHeight * (1f / 39.3701f)) / s_myParams.DPI;

            if (s_myParams.CurvedImageTarget)
            {
                float lengthScaleFactor = 1f;   //convert to inch

                if (s_myParams.LengthUnitType == 0) lengthScaleFactor = -1f;   //default (odle)
                else if (s_myParams.LengthUnitType == 1) lengthScaleFactor = 1f / 25.4f;  //mm
                else if (s_myParams.LengthUnitType == 2) lengthScaleFactor = 1f / 2.54f;  //cm
                else if (s_myParams.LengthUnitType == 3) lengthScaleFactor = 1f;  //inch
                else if (s_myParams.LengthUnitType == 4) lengthScaleFactor = 1f / 0.0254f;  //m

                //1 default(odle) unit equals half the height of input image
                trainTopRadius = (lengthScaleFactor == -1f) ? s_myParams.TopRadius : (s_myParams.TopRadius * lengthScaleFactor * s_myParams.DPI) / (0.5f * s_myParams.PixelHeight);
                trainBottomRadius = (lengthScaleFactor == -1f) ? s_myParams.BottomRadius : (s_myParams.BottomRadius * lengthScaleFactor * s_myParams.DPI) / (0.5f * s_myParams.PixelHeight);
                trainSideLength = (lengthScaleFactor == -1f || s_cylindricalImageTarget) ? s_myParams.SideLength : (s_myParams.SideLength * lengthScaleFactor * s_myParams.DPI) / (0.5f * s_myParams.PixelHeight);
            }
            else
            {
                trainTopRadius = trainBottomRadius = trainSideLength = -1f;
            }
            Debug.Log("TR:" + trainTopRadius + "/" + s_myParams.TopRadius
                + " BR:" + trainBottomRadius + "/" + s_myParams.BottomRadius
                + " SL:" + trainSideLength + "/" + s_myParams.SideLength
                + " PS:" + s_myParams.PhysicalScaleFactor);
            int success = Z.TrainImageCompressedWithMaxCurved(ref src, ref zpt, ref preview,
                FileIsJpg(s_myParams.ImagePath) ? 1 : 0, s_myParams.MaxTrainWidth,
                s_myParams.MaxTrainHeight, trainTopRadius, trainBottomRadius,
                trainSideLength, s_myParams.PhysicalScaleFactor);

            if (success == 1)
            {
                Debug.Log("Success in training image");

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
                Debug.LogError("Failed to train image. Check that training parameters are correct!");
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
            string filename = Path.GetFileNameWithoutExtension(s_myParams.ImagePath) + (s_myParams.CurvedImageTarget ? "_curved" : "") + ".zpt";
            string zptFile = Path.Combine(s_zptPath, filename);
            return zptFile;
        }

        private static bool FileIsJpg(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            return (ext == "jpg") || (ext == "jpeg");
        }

        private static float CalculateDPI(ZImageTrainerParams param, bool floorDPI = false)
        {
            if (param.PixelWidth == 0 || param.PixelHeight == 0) return 0f;
            if (param.PhysicalUnitType == 0) return param.DPI; //provided by user

            float convertToInch = 1f;
            if (param.PhysicalUnitType == 1) convertToInch = 1f / 25.4f;  //mm
            else if (param.PhysicalUnitType == 2) convertToInch = 1f / 2.54f;  //cm
            else if (param.PhysicalUnitType == 3) convertToInch = 1f;  //inch
            else if (param.PhysicalUnitType == 4) convertToInch = 1f / 0.0254f;  //m
            float ppi = (float)s_myParams.PixelHeight / (s_myParams.PhysicalHeight * convertToInch); //ppi
            return floorDPI ? Mathf.Floor(ppi) : ppi;
        }
    }
}