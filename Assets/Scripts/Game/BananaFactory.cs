using UnityEngine;

namespace TimeTravelBanana.Game
{
    public static class BananaFactory
    {
        public static Banana Create(Vector2 pos)
        {
            var go = new GameObject("Banana");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Art/banana");
            sr.sortingOrder = 5;

            go.AddComponent<PolygonCollider2D>();

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            return go.AddComponent<Banana>();
        }
    }
}
