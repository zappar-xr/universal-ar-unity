using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ZapparMenu : MonoBehaviour
{
#if (UNITY_EDITOR)

    [MenuItem("Zappar/Camera")]
    static void ZapparCreateCamera() {
        GameObject camera = (GameObject)Resources.Load("Prefabs/Zappar Camera Rear");
        Instantiate(camera, Vector3.zero, Quaternion.identity);
    }

    [MenuItem("Zappar/Face Tracker")]
    static void ZapparCreateFaceTrackingTarget() {
        GameObject faceTarget = (GameObject)Resources.Load("Prefabs/Zappar Face Tracker");
        Instantiate(faceTarget, Vector3.zero, Quaternion.identity);
    }

    [MenuItem("Zappar/Face Mesh")]
    static void ZapparCreateFaceMeshTarget() {
        GameObject faceMesh = (GameObject)Resources.Load("Prefabs/Zappar Face Mesh");
        Instantiate(faceMesh, Vector3.zero, Quaternion.identity);
    }

    [MenuItem("Zappar/Image Tracker")]
    static void ZapparCreateImageTrackingTarget() {
        GameObject imageTarget = (GameObject)Resources.Load("Prefabs/Zappar Image Tracker");
        Instantiate(imageTarget, Vector3.zero, Quaternion.identity);
    }

    [MenuItem("Zappar/Instant Tracker")]
    static void ZapparCreateInstantTrackingTarget() {
        GameObject instantTarget = (GameObject)Resources.Load("Prefabs/Zappar Instant Tracker");
        Instantiate(instantTarget, Vector3.zero, Quaternion.identity);
    }

    [MenuItem("Zappar/Utilities/Full Head Model")]
    static void ZapparCreateFullHeadModel() {
        GameObject headModel = (GameObject)Resources.Load("Prefabs/Zappar Full Head Model");
        Instantiate(headModel, Vector3.zero, Quaternion.identity);
    }

    [MenuItem("Zappar/Utilities/Full Head Depth Mask")]
    static void ZapparCreateFullHeadDepthMask() {
        GameObject headDepthMask = (GameObject)Resources.Load("Prefabs/Zappar Face Depth Mask");
        Instantiate(headDepthMask, Vector3.zero, Quaternion.identity);
    }

    [MenuItem("Zappar/Utilities/Documentation")]
    static void ZapparOpenDocumentation() {
        Application.OpenURL("https://docs.zap.works/universal-ar/unity");
    }

    [MenuItem("Zappar/Utilities/Install CLI")]
    static void ZapparOpenZapworksCLI() {
        Application.OpenURL("https://www.npmjs.com/package/@zappar/zapworks-cli");
    }
    
#endif
}
