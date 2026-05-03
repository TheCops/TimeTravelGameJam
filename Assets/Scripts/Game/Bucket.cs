using UnityEngine;

namespace TimeTravelBanana.Game
{
    [RequireComponent(typeof(Collider2D))]
    public class Bucket : MonoBehaviour
    {
        [SerializeField] private Vector2 visualLocalPosition = new Vector2(0f, 0.7f);
        [SerializeField] private float visualScale = 0.5f;

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

            if (parent.Find("BasketVisual") == null)
            {
                var visual = new GameObject("BasketVisual");
                visual.transform.SetParent(parent, false);
                visual.transform.localPosition = new Vector3(visualLocalPosition.x, visualLocalPosition.y, 0f);
                visual.transform.localScale = new Vector3(visualScale, visualScale, 1f);
                var sr = visual.AddComponent<SpriteRenderer>();
                sr.sprite = Resources.Load<Sprite>("Art/basket");
                sr.sortingOrder = 1;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var banana = other.GetComponentInParent<Banana>();
            if (banana == null || banana.Resolved) return;
            banana.HitBucket();
        }
    }
}
