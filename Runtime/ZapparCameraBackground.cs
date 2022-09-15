using UnityEngine;

namespace Zappar
{
    [RequireComponent(typeof(Camera))]
    public class ZapparCameraBackground : ZapparBaseCameraBackground
    {
        public override void Start()
        {
            if (ZapparCamera.Instance != null)
                ZapparCamera.Instance.RegisterCameraListener(this, true);

            if (ZapparCamera.Instance.CameraSourceInitialized && !m_hasInitialized)
            {
                OnMirroringUpdate(ZapparCamera.Instance.MirrorCamera);
                OnZapparCameraPaused(ZapparCamera.Instance.CameraSourcePaused);
                OnZapparInitialized(ZapparCamera.Instance.GetPipeline);
            }
            base.Start();
        }

    }
}