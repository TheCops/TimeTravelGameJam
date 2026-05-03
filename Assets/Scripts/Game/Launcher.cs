using UnityEngine;

namespace TimeTravelBanana.Game
{
    public class Launcher : MonoBehaviour
    {
        [SerializeField] private float launchSpeed = 12f;
        [SerializeField] private GameObject bananaPrefab;
        [SerializeField] private float bananaScale = 0.5f;
        [SerializeField] private MonkeyController monkey;
        [SerializeField, Tooltip("Seconds to wait after entering Playing state before launching the banana.")]
        private float launchDelay = 1f;

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
            gm.UnregisterLauncher(this);
            gm.OnPlayingState -= HandlePlayingState;
        }

        private void HandlePlayingState()
        {
            StartCoroutine(LaunchAfterDelay());
        }

        private System.Collections.IEnumerator LaunchAfterDelay()
        {
            if (launchDelay > 0f)
                yield return new WaitForSeconds(launchDelay);
            SpawnAndLaunchInstance(autoDestroyOnResolve: true);
        }

        public Banana SpawnAndLaunchInstance(bool autoDestroyOnResolve)
        {
            if (monkey == null)
            {
                Debug.LogWarning("Launcher: monkey reference is not set; cannot launch.");
                return null;
            }
            Vector2 spawnPos = monkey.Hand != null
                ? (Vector2)monkey.Hand.position
                : (Vector2)transform.position;
            GameObject banana = Instantiate(bananaPrefab);
            banana.transform.localScale = new Vector3(bananaScale, bananaScale, bananaScale);
            Banana bscript = banana.GetComponent<Banana>();
            bscript.AutoDestroyOnResolve = autoDestroyOnResolve;

            Vector2 velocity = monkey.AimDirection * launchSpeed;
            bscript.Launch(spawnPos, velocity);
            monkey.PlayThrow();
            return bscript;
        }
    }
}
