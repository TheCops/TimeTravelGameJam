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
        private Vector2 lastRecordedLinearVelocity;
        private float lastRecordedAngularVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            defaultBodyType = rb.bodyType;
            rb.bodyType = RigidbodyType2D.Kinematic;
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
            lastRecordedLinearVelocity = snapshot.LinearVelocity;
            lastRecordedAngularVelocity = snapshot.AngularVelocity;

            if (timeline.Mode == TimelineMode.Scrubbing)
            {
                float dt = Time.fixedDeltaTime;
                Vector2 implicitLinearVelocity = (snapshot.Position - rb.position) / dt;
                float implicitAngularVelocity = Mathf.DeltaAngle(rb.rotation, snapshot.Rotation) / dt;

                rb.MovePosition(snapshot.Position);
                rb.MoveRotation(snapshot.Rotation);
                rb.linearVelocity = implicitLinearVelocity;
                rb.angularVelocity = implicitAngularVelocity;
            }
            else
            {
                rb.position = snapshot.Position;
                rb.rotation = snapshot.Rotation;
                if (timeline.Mode == TimelineMode.Recording)
                {
                    rb.linearVelocity = snapshot.LinearVelocity;
                    rb.angularVelocity = snapshot.AngularVelocity;
                }
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
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        protected override void OnExitScrubbing()
        {
            if (!IsCurrentlyAlive) return;
            if (timeline != null && timeline.Mode == TimelineMode.Recording)
            {
                rb.bodyType = defaultBodyType;
                if (rb.bodyType == RigidbodyType2D.Dynamic)
                {
                    rb.linearVelocity = lastRecordedLinearVelocity;
                    rb.angularVelocity = lastRecordedAngularVelocity;
                }
            }
            else
            {
                // Exiting Scrubbing into Idle (game left Playing). Hard-stop physics
                // and let higher-level systems (PlanningDraggable, lifecycle) decide
                // body type for the next state.
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
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
