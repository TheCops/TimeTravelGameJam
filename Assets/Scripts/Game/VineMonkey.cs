using UnityEngine;
using TimeTravelBanana.Timeline;

namespace TimeTravelBanana.Game
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class VineMonkey : MonoBehaviour
    {
        [Header("Motion")]
        [SerializeField] private float amplitude = 2.5f;
        [SerializeField] private float speed = 1.5f;
        [SerializeField] private float phaseOffsetDeg = 0f;

        [Header("Vine")]
        [SerializeField] private float vineTopOffsetY = 6f;
        [SerializeField] private float vineWidth = 0.08f;
        [SerializeField] private Color vineColor = new Color(0.30f, 0.18f, 0.06f);

        private Vector3 anchor;
        private float fallbackStartTime;
        private LineRenderer vine;
        private TimelineManager timeline;

        private void Awake()
        {
            anchor = transform.position;
            fallbackStartTime = Time.time;
            timeline = FindFirstObjectByType<TimelineManager>();
            EnsureSprite();
            EnsureCollider();
            EnsureVine();
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.State != GameState.Playing) return;
            float clock = timeline != null ? timeline.CurrentTime : (Time.time - fallbackStartTime);
            float t = clock * speed + phaseOffsetDeg * Mathf.Deg2Rad;
            Vector3 p = anchor;
            p.y = anchor.y + Mathf.Sin(t) * amplitude;
            transform.position = p;
            UpdateVine();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var banana = other.GetComponentInParent<Banana>();
            if (banana == null || banana.Resolved) return;
            banana.Eat();
        }

        private void EnsureSprite()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            if (sr.sprite == null) sr.sprite = Resources.Load<Sprite>("Art/monkey");
            sr.flipX = true;
            sr.sortingOrder = 4;
        }

        private void EnsureCollider()
        {
            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            if (col.radius < 0.01f) col.radius = 0.6f;
        }

        private void EnsureVine()
        {
            var existing = transform.Find("Vine");
            GameObject vineGo;
            if (existing != null) vineGo = existing.gameObject;
            else
            {
                vineGo = new GameObject("Vine");
                vineGo.transform.SetParent(transform, false);
            }
            vine = vineGo.GetComponent<LineRenderer>();
            if (vine == null) vine = vineGo.AddComponent<LineRenderer>();
            vine.useWorldSpace = true;
            vine.positionCount = 2;
            vine.startWidth = vineWidth;
            vine.endWidth = vineWidth;
            vine.numCapVertices = 2;
            vine.sortingOrder = 3;
            if (vine.sharedMaterial == null)
                vine.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            vine.startColor = vineColor;
            vine.endColor = vineColor;
            UpdateVine();
        }

        private void UpdateVine()
        {
            if (vine == null) return;
            Vector3 top = new Vector3(anchor.x, anchor.y + amplitude + vineTopOffsetY, transform.position.z);
            Vector3 hand = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            vine.SetPosition(0, top);
            vine.SetPosition(1, hand);
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            if (vine != null) UpdateVine();
        }
    }
}
