using UnityEngine;

namespace TimeTravelBanana.Game
{
    public class MonkeyController : MonoBehaviour
    {
        [SerializeField] private float climbAmplitude = 1.5f;
        [SerializeField] private float climbSpeed = 0.6f;
        [SerializeField] private float waveAmplitudeDeg = 25f;
        [SerializeField] private float waveSpeed = 4f;
        [SerializeField] private float throwDurationSec = 0.35f;
        [SerializeField] private float throwAngleDeg = 90f;

        private Transform monkeyTr;
        private Transform armTr;
        private Transform handTr;
        private float monkeyBaseY;
        private float throwStartTime = -1f;

        public Transform Hand => handTr;

        private void Awake()
        {
            monkeyTr = transform.Find("Monkey");
            armTr = monkeyTr != null ? monkeyTr.Find("Arm") : null;
            handTr = armTr != null ? armTr.Find("Hand") : null;
            if (monkeyTr != null) monkeyBaseY = monkeyTr.localPosition.y;
        }

        public void PlayThrow()
        {
            throwStartTime = Time.time;
        }

        private void Update()
        {
            if (monkeyTr != null)
            {
                float y = monkeyBaseY + Mathf.Sin(Time.time * climbSpeed) * climbAmplitude;
                var lp = monkeyTr.localPosition;
                lp.y = y;
                monkeyTr.localPosition = lp;
            }

            if (armTr != null)
            {
                float waveAngle = Mathf.Sin(Time.time * waveSpeed) * waveAmplitudeDeg;
                float throwAngle = 0f;
                if (throwStartTime > 0f)
                {
                    float t = (Time.time - throwStartTime) / throwDurationSec;
                    if (t >= 1f) throwStartTime = -1f;
                    else throwAngle = -Mathf.Sin(t * Mathf.PI) * throwAngleDeg;
                }
                armTr.localRotation = Quaternion.Euler(0f, 0f, waveAngle + throwAngle);
            }
        }
    }
}
