using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
using System.IO;

[ExecuteInEditMode]
public class TargetFileListPopupAttribute : PropertyAttribute
{
    public TargetFileListPopupAttribute()
    {
        
    }
}

[ExecuteInEditMode]
[CustomPropertyDrawer(typeof(TargetFileListPopupAttribute))]
public class TargetFileListPopupDrawer : PropertyDrawer
{
    private DateTime lastFileCheck = new DateTime();
    List<String> files = new List<String>();

    private void UpdateFiles()
    {
        files = new List<String>();
        try {
            DirectoryInfo directory = new DirectoryInfo(Application.streamingAssetsPath);
            FileInfo[] f = directory.GetFiles("*.zpt");
            foreach (FileInfo fileHandle in f)
            {
                files.Add(fileHandle.Name);
            }
        } catch {
            // Unable to check streaming assets path
        }
        lastFileCheck = DateTime.Now;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (DateTime.Now.Second > (lastFileCheck.Second + 1)) {
            UpdateFiles();
        }
        TargetFileListPopupAttribute atb = attribute as TargetFileListPopupAttribute;

        if (files != null && files.Count != 0)
        {
            int index = Mathf.Max(files.IndexOf(property.stringValue), 0);
            index = EditorGUI.Popup(position, property.name, index, files.ToArray());
            property.stringValue = files[index];
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}

#endif