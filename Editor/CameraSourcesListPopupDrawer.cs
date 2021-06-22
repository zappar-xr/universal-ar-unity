using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Zappar.Editor
{
    [ExecuteInEditMode]
    [CustomPropertyDrawer(typeof(CameraSourcesListPopupAttribute))]
    public class CameraSourcesListPopupDrawer : PropertyDrawer
    {
        private static DateTime lastFileCheck = new DateTime();
        private static List<string> sources;
        private static Dictionary<string, string> idsByName;

        private static void UpdateList()
        {
            if (DateTime.Now.Second <= (lastFileCheck.Second + 1)) return;

            sources = new List<string>();
            idsByName = new Dictionary<string, string>();
            try
            {
                for (int i = 0; i < Z.CameraCount(); i++)
                {
                    string name = Z.CameraName(i);
                    if (idsByName.ContainsKey(name))
                    {
                        name = Z.CameraName(i) + " (" + Z.CameraId(i) + ")";
                    }
                    sources.Add(name);
                    idsByName.Add(name, Z.CameraId(i));
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to check camera list: " + e.Message);
            }
            lastFileCheck = DateTime.Now;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            UpdateList();
            CameraSourcesListPopupAttribute atb = attribute as CameraSourcesListPopupAttribute;

            if (sources != null && sources.Count != 0)
            {
                int index = Mathf.Max(sources.IndexOf(property.stringValue), 0);
                index = EditorGUI.Popup(position, property.name, index, sources.ToArray());
                property.stringValue = sources[index];
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}