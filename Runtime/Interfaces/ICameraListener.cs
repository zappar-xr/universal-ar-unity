namespace Zappar
{
    public interface ICameraListener
    {
        /// <summary>
        /// On new Zappar pipeline created
        /// </summary>
        void OnZapparInitialized(System.IntPtr pipeline);

        /// <summary>
        /// Zappar camera source paused(pause=true) started(pause=false)
        /// </summary>
        void OnZapparCameraPaused(bool pause);

        /// <summary>
        /// Camera mirroring changed/set
        /// </summary>
        void OnMirroringUpdate(bool mirrored);
    }
}