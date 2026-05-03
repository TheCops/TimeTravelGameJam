using UnityEngine;

namespace TimeTravelBanana.Timeline
{
    public abstract class TimelineRecorderBase : MonoBehaviour, ITimelineAffected
    {
        [SerializeField] protected TimelineManager timeline;

        private float birthTime;
        private float deathTime = float.PositiveInfinity;
        private bool currentlyAlive = true;

        public TimelineManager Timeline
        {
            get => timeline;
            set => timeline = value;
        }

        public float BirthTime => birthTime;
        public float DeathTime => deathTime;
        public bool IsCurrentlyAlive => currentlyAlive;

        public void SetBirthTime(float time)
        {
            birthTime = time;
            RecomputeAliveness();
        }

        public void MarkDeath(float time)
        {
            deathTime = time;
            RecomputeAliveness();
        }

        private void RecomputeAliveness()
        {
            if (timeline == null) return;
            bool alive = IsAliveAt(timeline.CurrentTime);
            if (alive != currentlyAlive)
            {
                currentlyAlive = alive;
                OnLifecycleChanged(alive);
            }
        }

        public void ConnectToTimeline(TimelineManager tm)
        {
            if (tm == null) return;
            timeline = tm;
            OnConnected();
            timeline.Register(this);
            if (timeline.Mode != TimelineMode.Recording) OnEnterScrubbing();
        }

        protected virtual void OnEnable()
        {
            if (timeline == null) return;
            OnConnected();
            timeline.Register(this);
            if (timeline.Mode != TimelineMode.Recording) OnEnterScrubbing();
        }

        protected virtual void OnDisable()
        {
            if (timeline != null) timeline.Unregister(this);
        }

        public abstract void CaptureState(float time);
        public abstract void RestoreState(float time);
        public abstract void TruncateFuture(float time);

        public void OnTimelineModeChanged(TimelineMode previous, TimelineMode next)
        {
            bool wasFrozen = previous != TimelineMode.Recording;
            bool isFrozen = next != TimelineMode.Recording;
            if (!wasFrozen && isFrozen) OnEnterScrubbing();
            else if (wasFrozen && !isFrozen) OnExitScrubbing();
        }

        protected virtual void OnConnected() { }
        protected virtual void OnEnterScrubbing() { }
        protected virtual void OnExitScrubbing() { }
        protected virtual void OnLifecycleChanged(bool isAlive) { }

        protected bool IsAliveAt(float time)
        {
            return time >= birthTime && time <= deathTime;
        }

        protected bool RestoreLifecycle(float time)
        {
            bool alive = IsAliveAt(time);
            if (alive != currentlyAlive)
            {
                currentlyAlive = alive;
                OnLifecycleChanged(alive);
            }
            return alive;
        }

        protected void TruncateLifecycle(float time)
        {
            if (deathTime > time) deathTime = float.PositiveInfinity;
        }
    }

    public abstract class TimelineRecorderBase<TSnapshot> : TimelineRecorderBase
    {
        protected TimelineSnapshotBuffer<TSnapshot> buffer;

        protected override void OnConnected()
        {
            if (buffer == null) buffer = new TimelineSnapshotBuffer<TSnapshot>(timeline.BufferCapacity);
        }

        public override void CaptureState(float time)
        {
            if (buffer == null) return;
            if (!IsAliveAt(time)) return;
            buffer.TryAppend(time, CaptureSnapshot());
        }

        public override void RestoreState(float time)
        {
            if (!RestoreLifecycle(time)) return;
            if (buffer == null) return;
            if (!buffer.TrySample(time, out var sample)) return;
            ApplySnapshot(Interpolate(sample.Before, sample.After, sample.Alpha));
        }

        public override void TruncateFuture(float time)
        {
            buffer?.Truncate(time);
            TruncateLifecycle(time);
        }

        protected abstract TSnapshot CaptureSnapshot();
        protected abstract void ApplySnapshot(TSnapshot snapshot);
        protected abstract TSnapshot Interpolate(TSnapshot before, TSnapshot after, float alpha);
    }
}
