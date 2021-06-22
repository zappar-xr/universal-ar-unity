using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Zappar.Editor
{
    [ExecuteInEditMode]
    [CustomPropertyDrawer(typeof(TargetFileListPopupAttribute))]
    public class TargetFileListPopupDrawer : PropertyDrawer
    {
        private DateTime lastFileCheck = new DateTime();
        List<string> zptFiles;

        private void UpdateFiles()
        {
            zptFiles = new List<string>();
            try
            {
                DirectoryInfo directory = new DirectoryInfo(Application.streamingAssetsPath);
                FileInfo[] files = directory.GetFiles("*.zpt");
                foreach (FileInfo file in files)
                {
                    zptFiles.Add(file.Name);
                }
            }
            catch (Exception e)
            {
                // Unable to check streaming assets path
                Debug.LogError("Unable to check streaming assets path: " + e.Message);
            }
            lastFileCheck = DateTime.Now;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (DateTime.Now.Second > (lastFileCheck.Second + 1))
            {
                UpdateFiles();
            }

            TargetFileListPopupAttribute atb = attribute as TargetFileListPopupAttribute;

            if (zptFiles != null && zptFiles.Count > 0)
            {
                int index = Mathf.Max(zptFiles.IndexOf(property.stringValue), 0);
                index = EditorGUI.Popup(position, property.name, index, zptFiles.ToArray());
                property.stringValue = zptFiles[index];
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}