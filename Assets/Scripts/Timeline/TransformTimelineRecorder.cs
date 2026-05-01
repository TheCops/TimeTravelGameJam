using UnityEngine;

namespace TimeTravelBanana.Timeline
{
    public class TransformTimelineRecorder : MonoBehaviour, ITimelineAffected
    {
        [SerializeField] private TimelineManager timeline;

        private struct Snapshot
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 LocalScale;
        }

        private TimelineSnapshotBuffer<Snapshot> _buffer;

        public TimelineManager Timeline
        {
            get => timeline;
            set => timeline = value;
        }

        private void OnEnable()
        {
            if (timeline == null) return;
            _buffer = new TimelineSnapshotBuffer<Snapshot>(timeline.BufferCapacity);
            timeline.Register(this);
        }

        private void OnDisable()
        {
            if (timeline != null) timeline.Unregister(this);
        }

        public void CaptureState(float time)
        {
            _buffer.Append(time, new Snapshot
            {
                Position = transform.position,
                Rotation = transform.rotation,
                LocalScale = transform.localScale
            });
        }

        public void RestoreState(float time)
        {
            if (!_buffer.TrySample(time, out var sample)) return;
            transform.position = Vector3.Lerp(sample.Before.Position, sample.After.Position, sample.Alpha);
            transform.rotation = Quaternion.Slerp(sample.Before.Rotation, sample.After.Rotation, sample.Alpha);
            transform.localScale = Vector3.Lerp(sample.Before.LocalScale, sample.After.LocalScale, sample.Alpha);
        }

        public void TruncateFuture(float time)
        {
            _buffer?.Truncate(time);
        }

        public void OnTimelineModeChanged(TimelineMode previous, TimelineMode next)
        {
        }
    }
}
