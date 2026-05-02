using UnityEngine;

namespace TimeTravelBanana.Timeline.Behaviors
{
    public class RotatingPlatform : MonoBehaviour
    {
        public enum Phase { Forward, Pause, Reverse }

        [SerializeField] private float degreesPerSecond = 90f;
        [SerializeField] private float forwardDuration = 3f;
        [SerializeField] private float pauseDuration = 1f;
        [SerializeField] private float reverseDuration = 3f;

        private Phase phase = Phase.Forward;
        private float phaseTimer;
        private float currentAngle;

        public Phase CurrentPhase { get => phase; set => phase = value; }
        public float PhaseTimer { get => phaseTimer; set => phaseTimer = value; }
        public float CurrentAngle
        {
            get => currentAngle;
            set
            {
                currentAngle = value;
                transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
            }
        }

        private void Update()
        {
            phaseTimer += Time.deltaTime;
            switch (phase)
            {
                case Phase.Forward:
                    currentAngle += degreesPerSecond * Time.deltaTime;
                    if (phaseTimer >= forwardDuration) { phase = Phase.Pause; phaseTimer = 0f; }
                    break;
                case Phase.Pause:
                    if (phaseTimer >= pauseDuration) { phase = Phase.Reverse; phaseTimer = 0f; }
                    break;
                case Phase.Reverse:
                    currentAngle -= degreesPerSecond * Time.deltaTime;
                    if (phaseTimer >= reverseDuration) { phase = Phase.Forward; phaseTimer = 0f; }
                    break;
            }
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
        }
    }
}
