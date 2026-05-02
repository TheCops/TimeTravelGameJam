using UnityEngine;

namespace TimeTravelBanana.Game
{
    public class Launcher : MonoBehaviour
    {
        [SerializeField] private float launchAngleDegrees = 60f;
        [SerializeField] private float launchSpeed = 12f;

        public void SetLaunchAngle(float degrees) => launchAngleDegrees = degrees;
        public void SetLaunchSpeed(float speed) => launchSpeed = speed;

        public Banana SpawnAndLaunchInstance(bool autoDestroyOnResolve)
        {
            var b = BananaFactory.Create(transform.position);
            b.AutoDestroyOnResolve = autoDestroyOnResolve;
            float rad = launchAngleDegrees * Mathf.Deg2Rad;
            Vector2 velocity = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * launchSpeed;
            b.Launch(transform.position, velocity);
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
