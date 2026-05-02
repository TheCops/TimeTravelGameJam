using UnityEngine;

namespace TimeTravelBanana.Game
{
    public class Launcher : MonoBehaviour
    {
        [SerializeField] private float launchAngleDegrees = 60f;
        [SerializeField] private float launchSpeed = 12f;
        [SerializeField] private Banana banana;

        public Banana Banana => banana;

        public void SetBanana(Banana b) => banana = b;
        public void SetLaunchAngle(float degrees) => launchAngleDegrees = degrees;
        public void SetLaunchSpeed(float speed) => launchSpeed = speed;

        public void Launch()
        {
            if (banana == null) return;
            float rad = launchAngleDegrees * Mathf.Deg2Rad;
            Vector2 velocity = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * launchSpeed;
            banana.Launch(transform.position, velocity);
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
