using UnityEngine;

namespace TimeTravelBanana.Game
{
    public static class BananaFactory
    {
        public static Banana Create(Vector2 pos)
        {
            var go = new GameObject("Banana");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.WhiteCircle;
            sr.color = new Color(1f, 0.85f, 0.1f);
            sr.sortingOrder = 5;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.bodyType = RigidbodyType2D.Dynamic;

            return go.AddComponent<Banana>();
        }
    }
}
