using UnityEngine;

namespace TimeTravelBanana.Game
{
    [RequireComponent(typeof(Collider2D))]
    public class Bucket : MonoBehaviour
    {
        [SerializeField] private Vector2 visualLocalPosition = new Vector2(0f, 0.7f);
        [SerializeField] private float visualScale = 0.5f;
        [SerializeField] private float innerWidth = 3.0f;
        [SerializeField] private float wallThickness = 0.3f;
        [SerializeField] private float wallHeight = 1.4f;
        [SerializeField] private float bottomThickness = 0.3f;
        [SerializeField] private float triggerHeight = 0.6f;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Awake()
        {
            var parent = transform.parent != null ? transform.parent : transform;
            for (int i = 0; i < parent.childCount; i++)
            {
                var sr = parent.GetChild(i).GetComponent<SpriteRenderer>();
                if (sr != null && sr.gameObject != gameObject) sr.enabled = false;
            }

            LayoutWalls(parent);
            LayoutTrigger();
            EnsureVisual(parent);
        }

        private void LayoutWalls(Transform parent)
        {
            float wallX = innerWidth * 0.5f + wallThickness * 0.5f;
            float bottomWidth = innerWidth + wallThickness * 2f;

            var bottom = parent.Find("Bottom");
            if (bottom != null)
            {
                bottom.localPosition = Vector3.zero;
                bottom.localScale = new Vector3(bottomWidth, bottomThickness, 1f);
            }
            var left = parent.Find("Left");
            if (left != null)
            {
                left.localPosition = new Vector3(-wallX, wallHeight * 0.5f, 0f);
                left.localScale = new Vector3(wallThickness, wallHeight, 1f);
            }
            var right = parent.Find("Right");
            if (right != null)
            {
                right.localPosition = new Vector3(wallX, wallHeight * 0.5f, 0f);
                right.localScale = new Vector3(wallThickness, wallHeight, 1f);
            }
        }

        private void LayoutTrigger()
        {
            transform.localPosition = new Vector3(0f, wallHeight * 0.5f, 0f);
            var box = GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = new Vector2(innerWidth, triggerHeight);
                box.offset = Vector2.zero;
                box.isTrigger = true;
            }
        }

        private void EnsureVisual(Transform parent)
        {
            var existing = parent.Find("BasketVisual");
            Transform visualTr;
            if (existing != null)
            {
                visualTr = existing;
            }
            else
            {
                var go = new GameObject("BasketVisual");
                go.transform.SetParent(parent, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Resources.Load<Sprite>("Art/basket");
                sr.sortingOrder = 1;
                visualTr = go.transform;
            }
            float scale = visualScale * (innerWidth / 1.4f);
            visualTr.localPosition = new Vector3(visualLocalPosition.x, visualLocalPosition.y, 0f);
            visualTr.localScale = new Vector3(scale, scale, 1f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var banana = other.GetComponentInParent<Banana>();
            if (banana == null || banana.Resolved) return;
            banana.HitBucket();
        }
    }
}
