using UnityEngine;

namespace TimeTravelBanana.Timeline
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class RigidbodyTimelineRecorder : MonoBehaviour, ITimelineAffected
    {
        [SerializeField] private TimelineManager timeline;

        private struct Snapshot
        {
            public Vector2 Position;
            public float Rotation;
            public Vector2 LinearVelocity;
            public float AngularVelocity;
        }

        private Rigidbody2D rb;
        private TimelineSnapshotBuffer<Snapshot> timelineBuffer;
        private RigidbodyType2D defaultBodyType;
        private Vector2 pendingLinearVelocity;
        private float pendingAngularVelocity;

        public TimelineManager Timeline
        {
            get => timeline;
            set => timeline = value;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            defaultBodyType = rb.bodyType;
        }

        private void OnEnable()
        {
            if (timeline == null) return;
            timelineBuffer = new TimelineSnapshotBuffer<Snapshot>(timeline.BufferCapacity);
            timeline.Register(this);
            if (timeline.Mode == TimelineMode.Scrubbing)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        private void OnDisable()
        {
            if (timeline != null) timeline.Unregister(this);
        }

        public void CaptureState(float time)
        {
            timelineBuffer.Append(time, new Snapshot
            {
                Position = rb.position,
                Rotation = rb.rotation,
                LinearVelocity = rb.linearVelocity,
                AngularVelocity = rb.angularVelocity
            });
        }

        public void RestoreState(float time)
        {
            if (!timelineBuffer.TrySample(time, out var sample)) return;

            Vector2 pos = Vector2.Lerp(sample.Before.Position, sample.After.Position, sample.Alpha);
            float rot = Mathf.Lerp(sample.Before.Rotation, sample.After.Rotation, sample.Alpha);
            pendingLinearVelocity = Vector2.Lerp(sample.Before.LinearVelocity, sample.After.LinearVelocity, sample.Alpha);
            pendingAngularVelocity = Mathf.Lerp(sample.Before.AngularVelocity, sample.After.AngularVelocity, sample.Alpha);

            rb.position = pos;
            rb.rotation = rot;
            if (timeline.Mode == TimelineMode.Recording)
            {
                rb.linearVelocity = pendingLinearVelocity;
                rb.angularVelocity = pendingAngularVelocity;
            }
        }

        public void TruncateFuture(float time)
        {
            timelineBuffer?.Truncate(time);
        }

        public void OnTimelineModeChanged(TimelineMode previous, TimelineMode next)
        {
            if (previous == TimelineMode.Recording && next == TimelineMode.Scrubbing)
                rb.bodyType = RigidbodyType2D.Kinematic;
            else if (previous == TimelineMode.Scrubbing && next == TimelineMode.Recording)
            {
                rb.bodyType = defaultBodyType;
                if (rb.bodyType == RigidbodyType2D.Dynamic)
                {
                    rb.linearVelocity = pendingLinearVelocity;
                    rb.angularVelocity = pendingAngularVelocity;
                }
            }
        }
    }
}
