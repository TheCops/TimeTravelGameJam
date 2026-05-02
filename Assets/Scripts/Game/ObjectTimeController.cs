using UnityEngine;
using UnityEngine.InputSystem;
using TimeTravelBanana.Timeline;

namespace TimeTravelBanana.Game
{
    public class ObjectTimeController : MonoBehaviour
    {
        [SerializeField] private TimelineManager timeline;
        [SerializeField] private float defaultRate = 1f;
        [SerializeField] private float fastForwardRate = 2.5f;
        [SerializeField] private float rewindRate = -2f;

        private bool active;
        private float objectTime;

        public float ObjectTime => objectTime;
        public bool Active => active;

        public void SetTimeline(TimelineManager tm) => timeline = tm;
        public void SetRates(float defaultRate, float fastForwardRate, float rewindRate)
        {
            this.defaultRate = defaultRate;
            this.fastForwardRate = fastForwardRate;
            this.rewindRate = rewindRate;
        }

        public void BeginPlayback()
        {
            objectTime = 0f;
            if (timeline != null) timeline.CurrentTime = 0f;
            active = true;
        }

        public void StopPlayback()
        {
            active = false;
        }

        private void Update()
        {
            if (!active || timeline == null) return;

            float rate = defaultRate;
            var kb = Keyboard.current;
            if (kb != null)
            {
                bool right = kb.rightArrowKey.isPressed;
                bool left = kb.leftArrowKey.isPressed;
                if (right && !left) rate = fastForwardRate;
                else if (left && !right) rate = rewindRate;
            }

            objectTime += rate * Time.deltaTime;
            objectTime = Mathf.Clamp(objectTime, 0f, timeline.RecordedDuration);
            timeline.CurrentTime = objectTime;
        }
    }
}
