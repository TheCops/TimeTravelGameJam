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
        [SerializeField] private float throwDurationSec = 0.55f;
        [SerializeField] private float throwKickDeg = 45f;
        [SerializeField] private Camera aimCamera;

        private Transform monkeyTr;
        private Transform armTr;
        private Transform handTr;
        private float worldAimDeg;
        private float throwStartTime = -1f;
        private float intrinsicForwardDeg;

        public Transform Hand => handTr;
        public float AimAngleDeg => worldAimDeg;
        public Vector2 AimDirection
        {
            get
            {
                float r = worldAimDeg * Mathf.Deg2Rad;
                return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
            }
        }
        public Vector2 ShoulderPosition => armTr != null ? (Vector2)armTr.position : (Vector2)transform.position;
        public float HandReach
        {
            get
            {
                if (handTr == null) return 0f;
                Vector3 h = handTr.localPosition;
                return new Vector2(h.x, h.y).magnitude;
            }
        }
        public Vector2 LaunchOrigin => ShoulderPosition + AimDirection * HandReach;

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
            if (aimCamera == null) aimCamera = Camera.main;
        }

        public void PlayThrow()
        {
            throwStartTime = Time.time;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            bool playing = gm == null || gm.State == GameState.Playing;
            if (!playing) return;
            UpdateVertical();
            UpdateAim();
            ApplyArmRotation();
        }

        public void RefreshAim()
        {
            UpdateAim();
            ApplyArmRotation();
        }

        private void UpdateVertical()
        {
            if (monkeyTr == null) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            float v = 0f;
            if (kb.upArrowKey.isPressed || kb.wKey.isPressed) v += 1f;
            if (kb.downArrowKey.isPressed || kb.sKey.isPressed) v -= 1f;
            if (v == 0f) return;
            var lp = monkeyTr.localPosition;
            lp.y = Mathf.Clamp(lp.y + v * verticalSpeed * Time.deltaTime, minLocalY, maxLocalY);
            monkeyTr.localPosition = lp;
        }

        private void UpdateAim()
        {
            if (armTr == null) return;
            var cam = aimCamera != null ? aimCamera : Camera.main;
            var mouse = Mouse.current;
            if (cam == null || mouse == null) return;

            Vector3 screen = mouse.position.ReadValue();
            screen.z = -cam.transform.position.z;
            Vector3 world = cam.ScreenToWorldPoint(screen);
            Vector2 from = armTr.position;
            Vector2 dir = (Vector2)world - from;
            if (dir.sqrMagnitude < 0.0001f) return;
            float deg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            worldAimDeg = ClampToRange(deg);
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
