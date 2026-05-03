using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeTravelBanana.Game
{
    public class MonkeyController : MonoBehaviour
    {
        [SerializeField] private float verticalSpeed = 4f;
        [SerializeField] private float minLocalY = -3f;
        [SerializeField] private float maxLocalY = 3f;
        [SerializeField] private float minAimDeg = -90f;
        [SerializeField] private float maxAimDeg = 90f;
        [SerializeField] private float startAimDeg = 45f;
        [SerializeField] private float aimRotateSpeedDegPerSec = 90f;
        [SerializeField] private float throwDurationSec = 0.55f;
        [SerializeField] private float throwKickDeg = 45f;

        private Transform monkeyTr;
        private Transform armTr;
        private Transform handTr;
        private float worldAimDeg;
        private float throwStartTime = -1f;
        private float intrinsicForwardDeg;

        public Transform Hand => handTr;
        public Vector2 AimDirection
        {
            get
            {
                float r = worldAimDeg * Mathf.Deg2Rad;
                return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
            }
        }

        private void Awake()
        {
            monkeyTr = transform.Find("Monkey");
            armTr = monkeyTr != null ? monkeyTr.Find("Arm") : null;
            handTr = armTr != null ? armTr.Find("Hand") : null;
            worldAimDeg = ClampToRange(startAimDeg);
            intrinsicForwardDeg = 0f;
            if (handTr != null)
            {
                Vector3 h = handTr.localPosition;
                if (h.sqrMagnitude > 0.0001f)
                    intrinsicForwardDeg = Mathf.Atan2(h.y, h.x) * Mathf.Rad2Deg;
            }
        }

        public void PlayThrow()
        {
            throwStartTime = Time.time;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            bool placing = gm == null || gm.State == GameState.Placing;
            if (placing)
            {
                UpdateVertical();
                UpdateAim();
            }
            ApplyArmRotation();
        }

        private void UpdateVertical()
        {
            if (monkeyTr == null) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            float v = 0f;
            if (kb.upArrowKey.isPressed) v += 1f;
            if (kb.downArrowKey.isPressed) v -= 1f;
            if (v == 0f) return;
            var lp = monkeyTr.localPosition;
            lp.y = Mathf.Clamp(lp.y + v * verticalSpeed * Time.deltaTime, minLocalY, maxLocalY);
            monkeyTr.localPosition = lp;
        }

        private void UpdateAim()
        {
            if (armTr == null) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            float sign = 0f;
            if (kb.leftArrowKey.isPressed) sign += 1f;
            if (kb.rightArrowKey.isPressed) sign -= 1f;
            if (sign == 0f) return;
            worldAimDeg = ClampToRange(worldAimDeg + sign * aimRotateSpeedDegPerSec * Time.deltaTime);
        }

        private float ClampToRange(float deg)
        {
            float lo = Mathf.DeltaAngle(0f, minAimDeg);
            float hi = Mathf.DeltaAngle(0f, maxAimDeg);
            float wrapped = Mathf.DeltaAngle(0f, deg);
            return Mathf.Clamp(wrapped, lo, hi);
        }

        private void ApplyArmRotation()
        {
            if (armTr == null) return;
            float throwOffset = 0f;
            if (throwStartTime > 0f)
            {
                float t = (Time.time - throwStartTime) / throwDurationSec;
                if (t >= 1f) throwStartTime = -1f;
                else throwOffset = -Mathf.Sin(t * Mathf.PI) * throwKickDeg;
            }
            armTr.rotation = Quaternion.Euler(0f, 0f, worldAimDeg - intrinsicForwardDeg + throwOffset);
        }
    }
}
