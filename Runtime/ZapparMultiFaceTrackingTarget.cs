using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zappar
{
    public class ZapparMultiFaceTrackingTarget : ZapparTrackingTarget, ICameraListener
    {
        public int NumberOfAnchors => FaceAnchors.Count;
        public bool HasInitialized { get; private set; }
        public bool IsMirrored { get; private set; }
        public bool IsPaused { get; private set; }

        private IntPtr? m_faceTrackingPipeline = null;

        [SerializeField]
        public List<ZapparFaceTrackingAnchor> FaceAnchors = new List<ZapparFaceTrackingAnchor>();
        private List<bool> m_trackerIsTracked = new List<bool>();

        public IntPtr? FaceTrackerPipeline
        {
            get { return m_faceTrackingPipeline; }
            private set
            {
                m_faceTrackingPipeline = value;
            }
        }

        public void OnZapparInitialized(IntPtr pipeline)
        {
            IntPtr faceTracker = Z.FaceTrackerCreate(pipeline);
            Z.FaceTrackerMaxFacesSet(faceTracker, NumberOfAnchors);

#if UNITY_EDITOR
            byte[] faceTrackerData = Z.LoadRawBytes(Z.FaceTrackingModelPath());
            Z.FaceTrackerModelLoadFromMemory(faceTracker, faceTrackerData);
#else
                Z.FaceTrackerModelLoadDefault(faceTracker);
#endif
            FaceTrackerPipeline = faceTracker;
            HasInitialized = true;

            foreach (var anchor in FaceAnchors)
            {
                anchor?.InitFaceTracker();
                m_trackerIsTracked.Add(false);
            }
        }

        public void OnZapparCameraPaused(bool pause) { IsPaused = pause; }

        public void OnMirroringUpdate(bool mirrored)
        {
            IsMirrored = mirrored;
        }

        private void Start()
        {
            if (ZapparCamera.Instance != null)
                ZapparCamera.Instance.RegisterCameraListener(this, true);

            if (ZapparCamera.Instance.CameraSourceInitialized && !HasInitialized)
            {
                OnMirroringUpdate(ZapparCamera.Instance.MirrorCamera);
                OnZapparCameraPaused(ZapparCamera.Instance.CameraSourcePaused);
                OnZapparInitialized(ZapparCamera.Instance.GetPipeline);
            }

            StartCoroutine(UpdateEndOfFrame());
        }

        private void Update()
        {
            if (!HasInitialized || FaceTrackerPipeline == null || IsPaused)
                return;

            int count = Z.FaceTrackerAnchorCount(FaceTrackerPipeline.Value);

            for (int i = 0; i < count; ++i)
            {
                if (Int32.TryParse(Z.FaceTrackerAnchorId(FaceTrackerPipeline.Value, i), out int id))
                {
                    var anchor = FaceAnchors.Find(ent => ent.FaceTrackerIndex == id);
                    if (anchor != null)
                    {
                        anchor.AnchorId = i;
                        m_trackerIsTracked[id] = true;
                    }
                }
            }

            for (int i = 0; i < NumberOfAnchors; ++i)
            {
                FaceAnchors[i].UpdateAnchor(m_trackerIsTracked[i]);
                m_trackerIsTracked[i] = false;
            }
        }

        private IEnumerator UpdateEndOfFrame()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                if (!HasInitialized) continue;

                Z.FaceMeshUpdateVertexBuffer();
            }
        }

        private void OnDestroy()
        {
            if(HasInitialized)
            {
                if (FaceTrackerPipeline != null)
                {
                    Z.FaceTrackerDestroy(FaceTrackerPipeline.Value);
                    FaceTrackerPipeline = null;
                }
                HasInitialized = false;
            }

            StopCoroutine(UpdateEndOfFrame());
        }

        public void RegisterAnchor(ZapparFaceTrackingAnchor anchor, bool add)
        {
            if (add && !FaceAnchors.Contains(anchor))
            {
                FaceAnchors.Add(anchor);
            }
            else if(!add && FaceAnchors.Contains(anchor))
            {
                FaceAnchors.Remove(anchor);
            }
        }

        public override Matrix4x4 AnchorPoseCameraRelative()
        {
            if (FaceAnchors.Count > 0) 
                return FaceAnchors[0].AnchorPoseCameraRelative();

            return Matrix4x4.identity;
        }

    }
}