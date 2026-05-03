using UnityEngine;

namespace TimeTravelBanana.Game
{
    public class Launcher : MonoBehaviour
    {
        [SerializeField] private float launchAngleDegrees = 60f;
        [SerializeField] private float launchSpeed = 12f;
        [SerializeField] private MonkeyController monkey;

        public void SetLaunchAngle(float degrees) => launchAngleDegrees = degrees;
        public void SetLaunchSpeed(float speed) => launchSpeed = speed;

        public Banana SpawnAndLaunchInstance(bool autoDestroyOnResolve)
        {
            Vector2 spawnPos = (monkey != null && monkey.Hand != null)
                ? (Vector2)monkey.Hand.position
                : (Vector2)transform.position;
            var b = BananaFactory.Create(spawnPos);
            b.AutoDestroyOnResolve = autoDestroyOnResolve;
            float rad = launchAngleDegrees * Mathf.Deg2Rad;
            Vector2 velocity = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * launchSpeed;
            b.Launch(spawnPos, velocity);
            if (monkey != null) monkey.PlayThrow();
            return b;
        }

        private void OnDrawGizmos()
        {
            float rad = launchAngleDegrees * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + dir * 2f);
            Gizmos.DrawWireSphere(transform.position, 0.25f);
        }
    }
}
