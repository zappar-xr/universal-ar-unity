using UnityEditor;
using UnityEngine;
using System.IO;

namespace Zappar
{
    public class ZapparUtilities
    {
        private const int maxLayers = 100;
        private const int zapStartLayer = 15;

        public static Mesh GetAnIcoSphere(float radius, int rows=25, int cols=50, float width=8, float height=6)
        {
            Mesh mesh = new Mesh();
            mesh.name = "ZapparIcoSphere";
            int m = rows;
            int n = cols;
            Vector3[] vertices = new Vector3[(m + 1) * (n + 1)];//the positions of vertices 
            Vector2[] uv = new Vector2[(m + 1) * (n + 1)];
            Vector3[] normals = new Vector3[(m + 1) * (n + 1)];
            int[] triangles = new int[6 * m * n];
            for (int i = 0; i < vertices.Length; i++)
            {
                float x = i % (n + 1);
                float y = i / (n + 1);
                float x_pos = x / n * width;
                float y_pos = y / m * height;
                vertices[i] = new Vector3(x_pos, y_pos, 0);
                float u = x / n;
                float v = y / m;
                uv[i] = new Vector2(u, v);
            }
            for (int i = 0; i < 2 * m * n; i++)
            {
                int[] triIndex = new int[3];
                if (i % 2 == 0)
                {
                    triIndex[0] = i / 2 + i / (2 * n);
                    triIndex[1] = triIndex[0] + 1;
                    triIndex[2] = triIndex[0] + (n + 1);
                }
                else
                {
                    triIndex[0] = (i + 1) / 2 + i / (2 * n);
                    triIndex[1] = triIndex[0] + (n + 1);
                    triIndex[2] = triIndex[1] - 1;

                }
                triangles[i * 3] = triIndex[0];
                triangles[i * 3 + 1] = triIndex[1];
                triangles[i * 3 + 2] = triIndex[2];
            }

            float r = radius;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v;
                v.x = r * Mathf.Cos(vertices[i].x / width * 2 * Mathf.PI) * Mathf.Cos(vertices[i].y / height * Mathf.PI - Mathf.PI / 2);
                v.y = r * Mathf.Sin(vertices[i].x / width * 2 * Mathf.PI) * Mathf.Cos(vertices[i].y / height * Mathf.PI - Mathf.PI / 2);
                v.z = r * Mathf.Sin(vertices[i].y / height * Mathf.PI - Mathf.PI / 2);
                //v = vertices[i];

                vertices[i] = v;
                normals[i] = v.normalized;
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = triangles;

            return mesh;
        }

        public static void InvertMeshSurface(ref Mesh mesh)
        {
            // Reverse the triangles
            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int t = triangles[i];
                triangles[i] = triangles[i + 2];
                triangles[i + 2] = t;
            }
            mesh.triangles = triangles;

            // Reverse the normals;
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }

        public static bool CreateLayer(string layerName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);            
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            if (!PropertyExists(layersProp, zapStartLayer, layersProp.arraySize, layerName))
            {
                SerializedProperty sp;
                for (int i = zapStartLayer; i < maxLayers; ++i)
                {
                    sp = layersProp.GetArrayElementAtIndex(i);
                    if (sp.stringValue == "")
                    {
                        sp.stringValue = layerName;
                        Debug.Log("New layer: " + layerName + " has been added");
                        tagManager.ApplyModifiedProperties();
                        return true;
                    }
                }
            }
            else
            {
                Debug.Log ("Layer: " + layerName + " already exists");
            }
            return false;
        }

        public static bool RemoveLayer(string layerName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            if (PropertyExists(layersProp, zapStartLayer, layersProp.arraySize, layerName))
            {
                SerializedProperty sp;

                for (int i = zapStartLayer; i < maxLayers; i++)
                {

                    sp = layersProp.GetArrayElementAtIndex(i);

                    if (sp.stringValue == layerName)
                    {
                        sp.stringValue = "";
                        Debug.Log("Layer: " + layerName + " has been removed");
                        tagManager.ApplyModifiedProperties();
                        return true;
                    }

                }
            }

            return false;
        }

        private static bool PropertyExists(SerializedProperty property, int start, int end, string value)
        {
            for (int i = start; i < end; ++i)
            {
                SerializedProperty t = property.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Convert mesh to an open cylindrical one with shared vertices.
        /// </summary>
        /// <param name="mesh">unity mesh</param>
        /// <param name="radius">radius</param>
        /// <param name="height">height</param>
        /// <param name="scale">scale</param>
        /// <param name="stripes">vertical stripes for surface divisions resulting in smooth or faceted curves</param>
        /// <param name="texWrap">normally texture map is created for covering full surface. Set the value between 0-1. </param>
        /// <returns></returns>
        public static bool ConvertMeshToCylinder(Mesh mesh, float radius, float height=2, float scale=1f, int stripes=20, float texWrap=1f)
        {
            float width = 2f * Mathf.PI * radius;
            float texLen = width * texWrap;
            if (mesh == null || texLen > width || texLen <= 0f)
            {
                Debug.Log("Inavlid cylinder params");
                return false;
            }

            //debug flag for testing only
            bool curve = true;

            int vcount = 2 * stripes + 2;
            int pcount = 2 * stripes;
            float texScale = width / texLen;

            Vector3[] verts = new Vector3[vcount];
            Vector3[] norms = new Vector3[vcount];
            int[] tris = new int[3 * pcount];
            Vector2[] uvs = new Vector2[vcount];

            float dtheta = Mathf.Deg2Rad * (360f / pcount);
            float theta = 0f;

            for (int i = 0; i < vcount; ++i)
            {
                if (i % 2 == 0)
                {
                    //bottom strip
                    if (curve)
                    {
                        verts[i] = new Vector3(Mathf.Cos(theta) * radius, 0f, Mathf.Sin(theta) * radius) * scale;
                        norms[i] = verts[i].normalized;
                        theta += dtheta;
                    }
                    else
                    {
                        verts[i] = new Vector3(width * ((float)i / (vcount - 2f)), 0f, 0.0f) * scale;
                        norms[i] = new Vector3(0f, 0f, -1f);
                    }
                    uvs[i] = new Vector2((i / (vcount - 2f)) * texScale, 0f);
                }
                else
                {
                    //top strip
                    if (curve)
                    {
                        verts[i] = new Vector3(Mathf.Cos(theta - dtheta) * radius, height, Mathf.Sin(theta - dtheta) * radius) * scale;
                        norms[i] = verts[i].normalized;
                        theta += dtheta;
                    }
                    else
                    {
                        verts[i] = new Vector3(width * ((float)i - 1f) / (vcount - 2f), height, 0.0f) * scale;
                        norms[i] = new Vector3(0f, 0f, -1f);
                    }
                    uvs[i] = new Vector2(((i - 1f) / (vcount - 2f)) * texScale, 1f);
                }
            }

            int ind = 0;
            for (int i = 0; i < pcount; ++i)
            {
                tris[ind++] = i + 0;
                tris[ind++] = i + (i % 2 == 0 ? 1 : 2);
                tris[ind++] = i + (i % 2 == 0 ? 2 : 1);
            }

            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.triangles = tris;
            mesh.uv = uvs;
            return true;
        }

        public static bool ConvertMeshToQuad(Mesh mesh, Vector3 faceDir, float scale=1f)
        {
            if (mesh == null || faceDir == Vector3.zero) return false;
            faceDir.Normalize();

            const int vcount = 4;
            const int pcount = 2;
            Vector3[] verts = new Vector3[vcount];
            Vector3[] norms = new Vector3[vcount];
            int[] tris = new int[3 * pcount];
            Vector2[] uvs = new Vector2[vcount];

            Quaternion rot = Quaternion.FromToRotation(Vector3.up, faceDir);
            verts[0] = rot * new Vector3(-0.5f, 0, -0.5f) * scale;
            verts[1] = rot * new Vector3(-0.5f, 0, 0.5f) * scale;
            verts[2] = rot * new Vector3(0.5f, 0, 0.5f) * scale;
            verts[3] = rot * new Vector3(0.5f, 0, -0.5f) * scale;

            norms[0] = norms[1] = norms[2] = norms[3] = faceDir;

            tris[0] = 0; tris[1] = 1; tris[2] = 3;
            tris[3] = 1; tris[4] = 2; tris[5] = 3;

            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(0, 1);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(1, 0);

            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.triangles = tris;
            mesh.uv = uvs;
            return true;
        }
    }

    namespace Editor.Utils
    {
        internal static class ZUtils
        {
            internal static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
            {
                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);

                if (!dir.Exists)
                {
                    //Debug.Log("Source directory does not exist or could not be found: " + sourceDirName);
                    return;
                }

                DirectoryInfo[] dirs = dir.GetDirectories();

                // If the destination directory doesn't exist, create it.       
                Directory.CreateDirectory(destDirName);

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (file.Extension == ".meta")
                        continue;

                    string tempPath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(tempPath, true);
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
    }
}