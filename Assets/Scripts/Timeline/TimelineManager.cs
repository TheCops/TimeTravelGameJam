using System.Collections.Generic;
using UnityEngine;

namespace TimeTravelBanana.Timeline
{
    public class TimelineManager : MonoBehaviour
    {
        [SerializeField] private string timelineId = "Default";
        [SerializeField, Min(1)] private int tickRate = 50;
        [SerializeField, Min(1)] private int maxBufferSeconds = 30;
        [SerializeField] private TimelineMode startMode = TimelineMode.Idle;

        private readonly List<ITimelineAffected> timeObjects = new List<ITimelineAffected>();
        private float currentTime;
        private float timelineLength;
        private TimelineMode currentTimelineMode;

        public string TimelineId => timelineId;
        public int TickRate => tickRate;
        public int MaxBufferSeconds => maxBufferSeconds;
        public int BufferCapacity => tickRate * maxBufferSeconds;
        public float RecordedDuration => timelineLength;

        public float CurrentTime
        {
            get => currentTime;
            set
            {
                currentTime = Mathf.Clamp(value, 0f, timelineLength);
                if (currentTimelineMode != TimelineMode.Scrubbing) Mode = TimelineMode.Scrubbing;
            }
        }

        public void Configure(int tickRate, int maxBufferSeconds)
        {
            this.tickRate = Mathf.Max(1, tickRate);
            this.maxBufferSeconds = Mathf.Max(1, maxBufferSeconds);
        }

        public void SetCurrentTimeWithoutModeChange(float t)
        {
            currentTime = Mathf.Clamp(t, 0f, Mathf.Max(timelineLength, t));
        }

        public TimelineMode Mode
        {
            get => currentTimelineMode;
            set
            {
                if (currentTimelineMode == value) return;
                TimelineMode previous = currentTimelineMode;
                currentTimelineMode = value;

                if (value == TimelineMode.Recording)
                {
                    for (int i = 0; i < timeObjects.Count; i++)
                    {
                        timeObjects[i].TruncateFuture(currentTime);
                        timeObjects[i].RestoreState(currentTime);
                    }
                    timelineLength = currentTime;
                }

                for (int i = 0; i < timeObjects.Count; i++)
                    timeObjects[i].OnTimelineModeChanged(previous, value);
            }
        }

        private void Awake()
        {
            currentTimelineMode = startMode;
        }

        public void Register(ITimelineAffected affected)
        {
            if (affected == null || timeObjects.Contains(affected)) return;
            timeObjects.Add(affected);
        }

        public void Unregister(ITimelineAffected affected)
        {
            timeObjects.Remove(affected);
        }

        public void ResetTimeline()
        {
            Mode = TimelineMode.Idle;
            currentTime = 0f;
            timelineLength = 0f;
            for (int i = 0; i < timeObjects.Count; i++)
                timeObjects[i].TruncateFuture(-1f);
        }

        public void BakeFor(float seconds)
        {
            var oldSimMode = Physics2D.simulationMode;
            Physics2D.simulationMode = SimulationMode2D.Script;

            currentTime = 0f;
            timelineLength = 0f;
            Mode = TimelineMode.Recording;

            for (int i = 0; i < timeObjects.Count; i++)
                timeObjects[i].CaptureState(currentTime);

            float dt = Time.fixedDeltaTime;
            int steps = Mathf.CeilToInt(seconds / dt);

            for (int s = 0; s < steps; s++)
            {
                Physics2D.Simulate(dt);
                currentTime += dt;
                timelineLength = currentTime;
                for (int i = 0; i < timeObjects.Count; i++)
                    timeObjects[i].CaptureState(currentTime);
            }

            Physics2D.simulationMode = oldSimMode;

            currentTime = 0f;
            Mode = TimelineMode.Scrubbing;
            for (int i = 0; i < timeObjects.Count; i++)
                timeObjects[i].RestoreState(currentTime);
        }

        private void FixedUpdate()
        {
            if (currentTimelineMode == TimelineMode.Recording) {
                if (currentTime >= maxBufferSeconds) return;
                currentTime = Mathf.Min(currentTime + Time.fixedDeltaTime, maxBufferSeconds);
                timelineLength = currentTime;
                for (int i = 0; i < timeObjects.Count; i++)
                    timeObjects[i].CaptureState(currentTime);
            }
        }

        private void LateUpdate()
        {
            if (currentTimelineMode == TimelineMode.Scrubbing) {
                for (int i = 0; i < timeObjects.Count; i++)
                    timeObjects[i].RestoreState(currentTime);
            }
        }
    }
}
