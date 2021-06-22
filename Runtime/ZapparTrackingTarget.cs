using UnityEngine;

namespace Zappar
{
    public class ZapparTrackingTarget : MonoBehaviour
    {

        // All tracking targets must implement this in order 
        // to drive the camera pose.
        public virtual Matrix4x4 AnchorPoseCameraRelative()
        {
            return Matrix4x4.identity;
        }
    }
}