using System.Collections.Generic;
using UnityEngine;

namespace TimeTravelBanana.Game
{
    public struct BananaScoreInfo
    {
        public int Points;
        public int Base;
        public int Hits;
        public string Label;
        public Vector3 WorldPos;
        public bool IsLoss;
        public bool IsEaten;
    }

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
        private readonly HashSet<int> hitContraptionIds = new HashSet<int>();

        private static readonly Color ColorGreen  = new Color(0.30f, 0.95f, 0.05f);
        private static readonly Color ColorRipe   = new Color(1.00f, 0.90f, 0.10f);
        private static readonly Color ColorBrown  = new Color(0.45f, 0.27f, 0.10f);
        private static readonly Color ColorRotten = new Color(0.00f, 0.00f, 0.00f);

        public static event System.Action<BananaScoreInfo> OnAnyBananaResolved;

        public bool Launched => launched;
        public bool Resolved { get; private set; }
        public bool IsCooked { get; private set; }
        public int HitCount => hitContraptionIds.Count;
        public bool AutoDestroyOnResolve { get => autoDestroyOnResolve; set => autoDestroyOnResolve = value; }

        public void Cook()
        {
            if (IsCooked || Resolved) return;
            IsCooked = true;
            if (sr != null)
            {
                var cooked = Resources.Load<Sprite>("Art/maduros");
                if (cooked != null) sr.sprite = cooked;
                sr.color = Color.white;
            }
        }

        public float NormalizedAge
        {
            get
            {
                if (!launched || maxFlightTime <= 0f) return 0f;
                return Mathf.Clamp01((Time.time - launchTime) / maxFlightTime);
            }
        }

        private static readonly string[] CookedLabels = { "Maduros!", "Extra Yum!" };

        public int BaseScore
        {
            get
            {
                float t = NormalizedAge;
                if (t >= rottenStartT) return 1;
                if (IsCooked) return 5;
                if (t >= ripeStartT && t < ripeEndT) return 3;
                return 2;
            }
        }

        public string RipenessLabel
        {
            get
            {
                float t = NormalizedAge;
                if (t >= rottenStartT) return "Rotten";
                if (IsCooked) return CookedLabels[Random.Range(0, CookedLabels.Length)];
                if (t >= ripeStartT && t < ripeEndT) return "Ripe!!";
                if (t < ripeStartT) return "Green";
                return "Brown";
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
            hitContraptionIds.Clear();
        }

        private static readonly string[] EatenLabels = { "NOM NOM!", "NOMNOM!", "Yummy!", "Tasty!", "Burp!", "Mine!" };

        public void Eat()
        {
            if (Resolved) return;
            Resolved = true;
            OnAnyBananaResolved?.Invoke(new BananaScoreInfo
            {
                Points = -3,
                Base = -3,
                Hits = HitCount,
                Label = EatenLabels[Random.Range(0, EatenLabels.Length)],
                WorldPos = transform.position,
                IsLoss = false,
                IsEaten = true,
            });
            if (autoDestroyOnResolve) Destroy(gameObject);
        }

        public void HitBucket()
        {
            if (Resolved) return;
            int basePts = BaseScore;
            int hits = HitCount;
            int total = basePts * (1 + hits);
            Resolved = true;
            OnWin?.Invoke();
            OnAnyBananaResolved?.Invoke(new BananaScoreInfo
            {
                Points = total,
                Base = basePts,
                Hits = hits,
                Label = RipenessLabel,
                WorldPos = transform.position,
                IsLoss = false,
            });
            if (autoDestroyOnResolve) Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            RegisterContraption(collision.collider);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            RegisterContraption(other);
        }

        private void RegisterContraption(Collider2D col)
        {
            if (col == null) return;
            var contraption = col.GetComponentInParent<Contraption>();
            if (contraption == null) return;
            hitContraptionIds.Add(contraption.GetInstanceID());
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
                OnAnyBananaResolved?.Invoke(new BananaScoreInfo
                {
                    Points = -1,
                    Base = -1,
                    Hits = HitCount,
                    Label = RipenessLabel,
                    WorldPos = p,
                    IsLoss = true,
                });
                if (autoDestroyOnResolve) Destroy(gameObject);
            }
        }
    }
}
