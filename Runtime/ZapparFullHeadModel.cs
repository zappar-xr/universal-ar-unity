using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Zappar;

#if UNITY_EDITOR

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ZapparFullHeadModel : MonoBehaviour
{
    public Material material;

    private IntPtr m_faceMesh = IntPtr.Zero;
    private int m_numIdentityCoefficients = 50;
    private int m_numExpressionCoefficients = 29;

    void Start()
    {
        // We want to run in the editor but not in Play Mode.
        if (Application.isPlaying)
        {
            gameObject.GetComponent<MeshFilter>().sharedMesh = new Mesh();
            return;
        }

        m_faceMesh = Z.FaceMeshCreate();
        
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        filter.sharedMesh = new Mesh();

        string filename = Z.FaceMeshFullHeadSimplifiedModelPath();
        byte[] data = Z.LoadRawBytes(filename);

        float[] identity = new float[m_numIdentityCoefficients];
        float[] expression = new float[m_numExpressionCoefficients];
        for (int i = 0; i < m_numIdentityCoefficients; ++i) identity[i] = 0.0f;
        for (int i = 0; i < m_numExpressionCoefficients; ++i) expression[i] = 0.0f;
        
        Z.FaceMeshLoadFromMemory(m_faceMesh, data, false, false, false, true);
        Z.FaceMeshUpdate(m_faceMesh, identity, expression, true);

        filter.sharedMesh.vertices = Z.UpdateFaceMeshVerticesForUnity(Z.FaceMeshVertices(m_faceMesh));
        filter.sharedMesh.normals = Z.UpdateFaceMeshNormalsForUnity(Z.FaceMeshNormals(m_faceMesh));
        filter.sharedMesh.triangles = Z.UpdateFaceMeshTrianglesForUnity(Z.FaceMeshIndices(m_faceMesh));
        filter.sharedMesh.uv = Z.UpdateFaceMeshUVsForUnity(Z.FaceMeshUvs(m_faceMesh));

        if (material != null)
            gameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
    }
}

#endif // UNITY_EDITOR