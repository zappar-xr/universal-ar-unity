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
        private DateTime m_lastFileCheck = new DateTime();
        List<string> m_zptFiles = new List<string>();

        private void UpdateFiles()
        {
            m_zptFiles.Clear();
            try
            {
                DirectoryInfo directory = new DirectoryInfo(Application.streamingAssetsPath);
                FileInfo[] files = directory.GetFiles("*.zpt");
                foreach (FileInfo file in files)
                {
                    m_zptFiles.Add(file.Name);
                }
            }
            catch (Exception e)
            {
                // Unable to check streaming assets path
                Debug.LogError("Unable to check streaming assets path: " + e.Message);
            }
            m_lastFileCheck = DateTime.Now;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (DateTime.Now.Second > (m_lastFileCheck.Second + 1))
            {
                UpdateFiles();
            }

            TargetFileListPopupAttribute atb = attribute as TargetFileListPopupAttribute;

            if (m_zptFiles != null && m_zptFiles.Count > 0)
            {
                int index = Mathf.Max(m_zptFiles.IndexOf(property.stringValue), 0);
                index = EditorGUI.Popup(position, property.name, index, m_zptFiles.ToArray());
                property.stringValue = m_zptFiles[index];
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}