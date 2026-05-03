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
        [SerializeField] private float ripeStartT = 0.30f;
        [SerializeField] private float ripeEndT = 0.55f;
        [SerializeField] private float rottenStartT = 0.85f;

        private Rigidbody2D rb;
        private SpriteRenderer sr;
        private float launchTime;
        private bool launched;

        private static readonly Color ColorGreen  = new Color(0.30f, 0.95f, 0.05f);
        private static readonly Color ColorRipe   = new Color(1.00f, 0.90f, 0.10f);
        private static readonly Color ColorBrown  = new Color(0.45f, 0.27f, 0.10f);
        private static readonly Color ColorRotten = new Color(0.00f, 0.00f, 0.00f);

        public static event System.Action<int, Vector3> OnAnyBananaScored;

        public bool Launched => launched;
        public bool Resolved { get; private set; }
        public bool IsCooked { get; private set; }
        public bool AutoDestroyOnResolve { get => autoDestroyOnResolve; set => autoDestroyOnResolve = value; }

        private static readonly Color ColorCooked = new Color(0.55f, 0.32f, 0.12f);

        public void Cook()
        {
            if (IsCooked || Resolved) return;
            IsCooked = true;
            if (sr != null) sr.color = ColorCooked;
        }

        public float NormalizedAge
        {
            get
            {
                if (!launched || maxFlightTime <= 0f) return 0f;
                return Mathf.Clamp01((Time.time - launchTime) / maxFlightTime);
            }
        }

        public int ScoreValue
        {
            get
            {
                float t = NormalizedAge;
                if (t >= rottenStartT) return 1;
                if (t >= ripeStartT && t < ripeEndT) return 3;
                return 2;
            }
        }

        public event System.Action OnWin;
        public event System.Action OnLose;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
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
            int points = ScoreValue;
            Vector3 pos = transform.position;
            Resolved = true;
            OnWin?.Invoke();
            OnAnyBananaScored?.Invoke(points, pos);
            if (autoDestroyOnResolve) Destroy(gameObject);
        }

        private void Update()
        {
            if (!launched || Resolved) return;

            float age = Time.time - launchTime;
            if (sr != null && !IsCooked)
            {
                float t = Mathf.Clamp01(age / maxFlightTime);
                if (t < 0.5f)
                    sr.color = Color.Lerp(ColorGreen, ColorRipe, t / 0.5f);
                else if (t < 0.8f)
                    sr.color = Color.Lerp(ColorRipe, ColorBrown, (t - 0.5f) / 0.3f);
                else
                    sr.color = Color.Lerp(ColorBrown, ColorRotten, (t - 0.8f) / 0.2f);
            }

            Vector3 p = transform.position;
            if (age > maxFlightTime || p.y < offscreenY || Mathf.Abs(p.x) > offscreenXAbs)
            {
                Resolved = true;
                OnLose?.Invoke();
                OnAnyBananaScored?.Invoke(-1, p);
                if (autoDestroyOnResolve) Destroy(gameObject);
            }
        }
    }
}
