using UnityEngine;

namespace TimeTravelBanana.Game
{
    public class Launcher : MonoBehaviour
    {
        [SerializeField] private float launchAngleDegrees = 60f;
        [SerializeField] private float launchSpeed = 12f;
        
        [SerializeField] private GameObject bananaPrefab;
        [SerializeField] private float bananaScale = 0.5f;
        [SerializeField] private MonkeyController monkey;

        public void SetLaunchAngle(float degrees) => launchAngleDegrees = degrees;
        public void SetLaunchSpeed(float speed) => launchSpeed = speed;
        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogWarning("Launcher: no GameManager.Instance found at Start; cannot register.");
                return;
            }
            gm.RegisterLauncher(this);
            gm.OnPlayingState += HandlePlayingState;
        }
        private void OnDestroy()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnPlayingState -= HandlePlayingState;
            gm.UnregisterLauncher(this);
        }

        private void HandlePlayingState()
        {
            SpawnAndLaunchInstance(autoDestroyOnResolve: true);
        }
        public Banana SpawnAndLaunchInstance(bool autoDestroyOnResolve)
        {
            
            Vector2 spawnPos = (monkey != null && monkey.Hand != null)
                ? (Vector2)monkey.Hand.position
                : (Vector2)transform.position;
            GameObject banana = Instantiate(bananaPrefab);
            banana.transform.localScale = new Vector3(bananaScale, bananaScale, bananaScale);
            Banana bscript = banana.GetComponent<Banana>();
            bscript.AutoDestroyOnResolve = autoDestroyOnResolve;

            float rad = launchAngleDegrees * Mathf.Deg2Rad;
            Vector2 velocity = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * launchSpeed;
            
            bscript.Launch(spawnPos, velocity);
            if (monkey != null) monkey.PlayThrow();
            return bscript;
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
