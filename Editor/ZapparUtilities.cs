using UnityEditor;
using UnityEngine;

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
    }
}