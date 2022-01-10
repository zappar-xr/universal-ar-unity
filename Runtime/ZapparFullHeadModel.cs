#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Zappar
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ZapparFullHeadModel : MonoBehaviour
    {
        public Material HeadMaterial;

        private IntPtr? m_faceMesh = null;
        private const int NumIdentityCoefficients = 50;
        private const int NumExpressionCoefficients = 29;

        private void Start()
        {
            // We want to run in the editor but not in Play Mode.
            if (Application.isPlaying)
            {
                gameObject.SetActive(false);
                return;
            }

            if(m_faceMesh == null)
            {
                //Create new head model
                m_faceMesh = Z.FaceMeshCreate();

                MeshFilter filter = gameObject.GetComponent<MeshFilter>();
                filter.sharedMesh = new Mesh();

                string filename = Z.FaceMeshFullHeadSimplifiedModelPath();
                byte[] data = Z.LoadRawBytes(filename);

                float[] identity = new float[NumIdentityCoefficients];
                float[] expression = new float[NumExpressionCoefficients];
                for (int i = 0; i < NumIdentityCoefficients; ++i) identity[i] = 0.0f;
                for (int i = 0; i < NumExpressionCoefficients; ++i) expression[i] = 0.0f;

                Z.FaceMeshLoadFromMemory(m_faceMesh.Value, data, false, false, false, true);
                Z.FaceMeshUpdate(m_faceMesh.Value, identity, expression, true);

                filter.sharedMesh.vertices = Z.UpdateFaceMeshVerticesForUnity(Z.FaceMeshVertices(m_faceMesh.Value));
                filter.sharedMesh.normals = Z.UpdateFaceMeshNormalsForUnity(Z.FaceMeshNormals(m_faceMesh.Value));
                filter.sharedMesh.triangles = Z.UpdateFaceMeshTrianglesForUnity(Z.FaceMeshIndices(m_faceMesh.Value));
                filter.sharedMesh.uv = Z.UpdateFaceMeshUVsForUnity(Z.FaceMeshUvs(m_faceMesh.Value));
                filter.sharedMesh.name = "ZHeadModel";
            }

            if (HeadMaterial != null)
                gameObject.GetComponent<MeshRenderer>().sharedMaterial = HeadMaterial;
        }
    }
}
#endif // UNITY_EDITOR