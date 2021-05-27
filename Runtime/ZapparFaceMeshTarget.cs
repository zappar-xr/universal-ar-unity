using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using Zappar;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ZapparFaceMeshTarget :  MonoBehaviour, ZapparCamera.ICameraListener
{
    private IntPtr m_faceMesh = IntPtr.Zero;
    private bool m_hasInitialised = false;

    private Mesh m_mesh;
    private bool m_haveInitialisedFaceMesh = false;

    private bool m_isMirrored;

    public Material faceMaterial;
    
    public bool useDefaultFullHead = true;

    public bool fillEyeLeft;
    public bool fillEyeRight;
    public bool fillMouth;
    public bool fillNeck;

    public ZapparFaceTrackingTarget faceTracker;
    private IntPtr m_faceTrackingTargetPtr;
    private bool m_haveFaceTracker = false;

    private int m_numIdentityCoefficients = 50;
    private int m_numExpressionCoefficients = 29;

    void Start() {
        if (ZapparCamera.Instance != null)
            ZapparCamera.Instance.RegisterCameraListener( this );

#if UNITY_EDITOR
        m_faceMesh = Z.FaceMeshCreate();
        CreateMesh();  
#endif
    }

    public void OnValidate() {
        if (faceMaterial != null)
            gameObject.GetComponent<MeshRenderer>().sharedMaterial = faceMaterial;
    }

    public void OnZapparInitialised(IntPtr pipeline) 
    {
        if (faceTracker != null) 
        {
            m_faceTrackingTargetPtr = faceTracker.m_faceTracker;
            m_haveFaceTracker = true;
        }
        else {
            Debug.Log("Warning: The face mesh will not update its vertices unless a Face Tracker is assigned.");
        }
        
        m_hasInitialised = true;
        m_haveInitialisedFaceMesh = false;

        m_faceMesh = Z.FaceMeshCreate();
        CreateMesh();  
    }

    public void OnMirroringUpdate(bool mirrored)
    {
        m_isMirrored = mirrored;
        m_haveInitialisedFaceMesh = false;
    }

    private void CreateMesh() {

        LoadMeshData();
        
        if (faceMaterial != null)
            gameObject.GetComponent<MeshRenderer>().sharedMaterial = faceMaterial;
        m_mesh = new Mesh();
        gameObject.GetComponent<MeshFilter>().sharedMesh = m_mesh;

        UpdateMeshData();
    }

    private void LoadMeshData() 
    {
#if UNITY_EDITOR
        string filename;
        if (useDefaultFullHead) 
            filename = Z.FaceMeshFullHeadSimplifiedModelPath();
        else    
            filename = Z.FaceMeshFaceModelPath();
        byte[] data = Z.LoadRawBytes(filename);
        Z.FaceMeshLoadFromMemory(m_faceMesh, data, fillMouth, fillEyeLeft, fillEyeRight, fillNeck);
#else
        if (useDefaultFullHead)
        {
            Z.FaceMeshLoadDefaultFullHeadSimplified(m_faceMesh, fillMouth, fillEyeLeft, fillEyeRight, fillNeck);
        } else {
            if (!fillEyeLeft && !fillEyeRight && !fillMouth) 
            {
                Z.FaceMeshLoadDefault(m_faceMesh);
            }
            else Z.FaceMeshLoadDefaultFace(m_faceMesh, fillEyeLeft, fillEyeRight, fillMouth);
        }
#endif
    }

    private void UpdateMeshData() 
    {
        if (m_faceMesh == null)
            return;

#if !UNITY_EDITOR
        if (m_haveFaceTracker && Z.FaceTrackerAnchorCount(m_faceTrackingTargetPtr) == 0)
        {
            return;
        }
#endif

        float[] identity;
        float[] expression;
        
        if (!m_haveFaceTracker) 
        {
            identity = new float[m_numIdentityCoefficients];
            expression = new float[m_numExpressionCoefficients];
            for (int i = 0; i < m_numIdentityCoefficients; ++i) identity[i] = 0.0f;
            for (int i = 0; i < m_numExpressionCoefficients; ++i) expression[i] = 0.0f;
        } else {
            identity = Z.FaceTrackerAnchorIdentityCoefficients(m_faceTrackingTargetPtr, 0);
            expression = Z.FaceTrackerAnchorExpressionCoefficients(m_faceTrackingTargetPtr, 0);
        }

        Z.FaceMeshUpdate(m_faceMesh, identity, expression, m_isMirrored);

        m_mesh.vertices = Z.UpdateFaceMeshVerticesForUnity(Z.FaceMeshVertices(m_faceMesh));
        m_mesh.normals = Z.UpdateFaceMeshNormalsForUnity(Z.FaceMeshNormals(m_faceMesh));

        if (!m_haveInitialisedFaceMesh)
        {
            m_mesh.triangles = Z.UpdateFaceMeshTrianglesForUnity(Z.FaceMeshIndices(m_faceMesh));
            m_mesh.uv = Z.UpdateFaceMeshUVsForUnity(Z.FaceMeshUvs(m_faceMesh));
            m_haveInitialisedFaceMesh = true;
        }
    }

    void Update()
    {
        if (!m_hasInitialised) 
            return;

        UpdateMeshData();            
    }

    void OnDestroy()
    {
        if (m_faceMesh != IntPtr.Zero) Z.FaceMeshDestroy(m_faceMesh);
        m_haveInitialisedFaceMesh = false;
        m_hasInitialised = false;
    }
}