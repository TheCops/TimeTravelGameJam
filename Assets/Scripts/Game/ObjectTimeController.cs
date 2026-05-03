using UnityEngine;
using TimeTravelBanana.Timeline;

namespace TimeTravelBanana.Game
{
    public class ObjectTimeController : MonoBehaviour
    {
        [SerializeField] private TimelineManager timeline;

        private bool reverseScrubActive;
        private float reverseScrubSpeed;
        private float objectTime;

        public float ObjectTime => objectTime;
        public bool IsReverseScrubbing => reverseScrubActive;

        public void SetTimeline(TimelineManager tm) => timeline = tm;

        public void BeginReverseScrub(float reverseSpeed)
        {
            if (timeline == null) return;
            if (reverseScrubActive) return;
            if (timeline.Mode != TimelineMode.Recording) return;

            objectTime = timeline.CurrentTime;
            reverseScrubSpeed = reverseSpeed;
            reverseScrubActive = true;
            timeline.Mode = TimelineMode.Scrubbing;
        }

        public void ResumeRecording()
        {
            if (timeline == null) return;
            reverseScrubActive = false;
            timeline.Mode = TimelineMode.Recording;
        }

        public void ClearReverseScrub()
        {
            reverseScrubActive = false;
            reverseScrubSpeed = 0f;
            objectTime = 0f;
        }

        private void Update()
        {
            if (!reverseScrubActive || timeline == null) return;
            objectTime += reverseScrubSpeed * Time.deltaTime;
            objectTime = Mathf.Clamp(objectTime, 0f, timeline.RecordedDuration);
            timeline.CurrentTime = objectTime;
        }
    }
}
