using UnityEngine;

namespace TimeTravelBanana.Timeline
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class RigidbodyTimelineRecorder : TimelineRecorderBase<RigidbodyTimelineRecorder.Snapshot>
    {
        public struct Snapshot
        {
            public Vector2 Position;
            public float Rotation;
            public Vector2 LinearVelocity;
            public float AngularVelocity;
        }

        private Rigidbody2D rb;
        private RigidbodyType2D defaultBodyType;
        private Vector2 pendingLinearVelocity;
        private float pendingAngularVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            defaultBodyType = rb.bodyType;
        }

        protected override Snapshot CaptureSnapshot() => new Snapshot
        {
            Position = rb.position,
            Rotation = rb.rotation,
            LinearVelocity = rb.linearVelocity,
            AngularVelocity = rb.angularVelocity
        };

        protected override void ApplySnapshot(Snapshot snapshot)
        {
            rb.position = snapshot.Position;
            rb.rotation = snapshot.Rotation;
            pendingLinearVelocity = snapshot.LinearVelocity;
            pendingAngularVelocity = snapshot.AngularVelocity;
            if (timeline.Mode == TimelineMode.Recording)
            {
                rb.linearVelocity = snapshot.LinearVelocity;
                rb.angularVelocity = snapshot.AngularVelocity;
            }
        }

        protected override Snapshot Interpolate(Snapshot before, Snapshot after, float alpha) => new Snapshot
        {
            Position = Vector2.Lerp(before.Position, after.Position, alpha),
            Rotation = Mathf.Lerp(before.Rotation, after.Rotation, alpha),
            LinearVelocity = Vector2.Lerp(before.LinearVelocity, after.LinearVelocity, alpha),
            AngularVelocity = Mathf.Lerp(before.AngularVelocity, after.AngularVelocity, alpha)
        };

        protected override void OnEnterScrubbing()
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        protected override void OnExitScrubbing()
        {
            if (!IsCurrentlyAlive) return;
            rb.bodyType = defaultBodyType;
            if (rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.linearVelocity = pendingLinearVelocity;
                rb.angularVelocity = pendingAngularVelocity;
            }
        }

        protected override void OnLifecycleChanged(bool isAlive)
        {
            if (!isAlive)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            else if (timeline != null && timeline.Mode != TimelineMode.Scrubbing)
            {
                rb.bodyType = defaultBodyType;
            }
        }
    }
}
