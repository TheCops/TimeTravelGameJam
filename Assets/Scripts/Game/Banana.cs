using UnityEngine;

namespace TimeTravelBanana.Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Banana : MonoBehaviour
    {
        [SerializeField] private float maxFlightTime = 8f;
        [SerializeField] private float offscreenY = -20f;
        [SerializeField] private float offscreenXAbs = 30f;
        [SerializeField] private bool autoDestroyOnResolve;

        private Rigidbody2D rb;
        private float launchTime;
        private bool launched;

        public bool Launched => launched;
        public bool Resolved { get; private set; }
        public bool AutoDestroyOnResolve { get => autoDestroyOnResolve; set => autoDestroyOnResolve = value; }

        public event System.Action OnWin;
        public event System.Action OnLose;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Launch(Vector2 spawnPos, Vector2 velocity)
        {
            transform.position = spawnPos;
            gameObject.SetActive(true);
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = velocity;
            rb.angularVelocity = 0f;
            launchTime = Time.time;
            launched = true;
            Resolved = false;
        }

        public void HitBucket()
        {
            if (Resolved) return;
            Resolved = true;
            OnWin?.Invoke();
            if (autoDestroyOnResolve) Destroy(gameObject);
        }

        private void Update()
        {
            if (!launched || Resolved) return;
            Vector3 p = transform.position;
            if (Time.time - launchTime > maxFlightTime || p.y < offscreenY || Mathf.Abs(p.x) > offscreenXAbs)
            {
                Resolved = true;
                OnLose?.Invoke();
                if (autoDestroyOnResolve) Destroy(gameObject);
            }
        }
    }
}
