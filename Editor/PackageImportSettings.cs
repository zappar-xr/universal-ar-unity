using System;
using System.IO;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class PackageImportSettings
{
    static PackageImportSettings()
    {
#if UNITY_EDITOR
        //Copy WebGL templates from package to project
        string srcDir = "Packages/com.zappar.uar/WebGLTemplates";
        string destDir = Application.dataPath + "/WebGLTemplates";
        DirectoryCopy(srcDir, destDir, true);
#endif
    }

    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            Debug.Log("Source directory does not exist or could not be found: " + sourceDirName);
            return;
        }

        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the destination directory doesn't exist, create it.       
        Directory.CreateDirectory(destDirName);

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, false);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
            }
        }
    }
}
