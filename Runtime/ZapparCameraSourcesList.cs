using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;
using Zappar;

#if UNITY_EDITOR
using System.IO;

[ExecuteInEditMode]
public class CameraSourcesListPopupAttribute : PropertyAttribute
{
    public CameraSourcesListPopupAttribute()
    {
        
    }
}

[ExecuteInEditMode]
[CustomPropertyDrawer(typeof(CameraSourcesListPopupAttribute))]
public class CameraSourcesListPopupDrawer : PropertyDrawer
{
    private static DateTime lastFileCheck = new DateTime();
    private static List<String> sources = new List<String>();
    private static Dictionary<String, String> idsByName = new Dictionary<String, String>();

    public static String IdFromName(String name) {
        UpdateList();
        if (name != null && idsByName.ContainsKey(name)) return idsByName[name];
        return null;
    }

    private static void UpdateList()
    {
        if (DateTime.Now.Second <= (lastFileCheck.Second + 1)) return;

        sources = new List<String>();
        idsByName = new Dictionary<String, String>();
        try {
            for (int i = 0; i < Z.CameraCount(); i++) {
                String name = Z.CameraName(i);
                if (idsByName.ContainsKey(name)) {
                    name = Z.CameraName(i) + " (" + Z.CameraId(i) + ")";
                }
                sources.Add(name);
                idsByName.Add(name, Z.CameraId(i));
            }
        } catch {
            // Unable to check streaming assets path
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

#endif
