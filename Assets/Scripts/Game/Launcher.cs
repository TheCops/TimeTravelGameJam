using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TimeTravelBanana.Game
{
    public class Launcher : MonoBehaviour
    {
        [SerializeField] private float launchAngleDegrees = 60f;
        [SerializeField] private float launchSpeed = 12f;
        [SerializeField] private MonkeyController monkey;

        [Header("Fire Input")]
        [SerializeField] private float autoFireInterval = 0.22f;
        [SerializeField] private float holdToRepeatDelay = 0.22f;
        [SerializeField] private int maxActiveBananas = 240;

        private readonly Queue<Banana> active = new Queue<Banana>();
        private float holdTimer;
        private float repeatTimer;
        private bool repeating;

        public void SetLaunchAngle(float degrees) => launchAngleDegrees = degrees;
        public void SetLaunchSpeed(float speed) => launchSpeed = speed;

        public Banana SpawnAndLaunchInstance(bool autoDestroyOnResolve)
        {
            Vector2 spawnPos;
            Vector2 dir;
            if (monkey != null)
            {
                monkey.RefreshAim();
                spawnPos = monkey.LaunchOrigin;
                dir = monkey.AimDirection;
            }
            else
            {
                spawnPos = (Vector2)transform.position;
                float rad = launchAngleDegrees * Mathf.Deg2Rad;
                dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            }

            var b = BananaFactory.Create(spawnPos);
            b.AutoDestroyOnResolve = autoDestroyOnResolve;
            b.Launch(spawnPos, dir * launchSpeed);
            if (monkey != null) monkey.PlayThrow();
            return b;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing)
            {
                ResetHoldState();
                return;
            }
            var mouse = Mouse.current;
            if (mouse == null) return;

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            if (mouse.leftButton.wasPressedThisFrame && !overUI)
            {
                Fire();
                holdTimer = 0f;
                repeatTimer = 0f;
                repeating = false;
            }
            else if (mouse.leftButton.isPressed && !overUI)
            {
                holdTimer += Time.deltaTime;
                if (!repeating)
                {
                    if (holdTimer >= holdToRepeatDelay)
                    {
                        repeating = true;
                        repeatTimer = 0f;
                        Fire();
                    }
                }
                else
                {
                    repeatTimer += Time.deltaTime;
                    while (repeatTimer >= autoFireInterval)
                    {
                        repeatTimer -= autoFireInterval;
                        Fire();
                    }
                }
            }
            else
            {
                ResetHoldState();
            }

            while (active.Count > 0 && active.Peek() == null) active.Dequeue();
        }

        private void Fire()
        {
            var b = SpawnAndLaunchInstance(autoDestroyOnResolve: true);
            if (b != null) active.Enqueue(b);

            while (active.Count > 0 && active.Peek() == null) active.Dequeue();
            while (active.Count > maxActiveBananas)
            {
                var oldest = active.Dequeue();
                if (oldest != null) Destroy(oldest.gameObject);
            }
        }

        private void ResetHoldState()
        {
            holdTimer = 0f;
            repeatTimer = 0f;
            repeating = false;
        }

        public void ClearActiveBananas()
        {
            while (active.Count > 0)
            {
                var b = active.Dequeue();
                if (b != null) Destroy(b.gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            float deg = monkey != null ? monkey.AimAngleDeg : launchAngleDegrees;
            float rad = deg * Mathf.Deg2Rad;
            Vector3 origin = monkey != null && monkey.Hand != null ? monkey.Hand.position : transform.position;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + dir * 2f);
            Gizmos.DrawWireSphere(origin, 0.25f);
        }
    }
}
