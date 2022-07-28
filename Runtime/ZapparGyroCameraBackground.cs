using UnityEngine;

namespace Zappar
{
    [RequireComponent(typeof(Camera))]
    public class ZapparGyroCameraBackground : ZapparBaseCameraBackground
    {
        public override void Start()
        {
            if (ZapparGyroCamera.Instance != null)
                ZapparGyroCamera.Instance.RegisterCameraListener(this, true);

            if (ZapparGyroCamera.Instance.CameraSourceInitialized && !m_hasInitialized)
            {
                OnZapparCameraPaused(ZapparGyroCamera.Instance.CameraSourcePaused);
                OnZapparInitialized(ZapparGyroCamera.Instance.GetPipeline);
            }
            base.Start();
        }
    }
}