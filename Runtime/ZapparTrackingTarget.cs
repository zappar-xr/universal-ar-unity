using UnityEngine;

namespace Zappar
{
    public abstract class ZapparTrackingTarget : MonoBehaviour
    {

        // All tracking targets must implement this in order 
        // to drive the camera pose.
        public abstract Matrix4x4 AnchorPoseCameraRelative();
    }
}