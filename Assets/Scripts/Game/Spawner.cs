using UnityEngine;

namespace TimeTravelBanana.Game
{
    public static class Spawner
    {
        public static PlanningDraggable Trampoline(Vector2 pos)
        {
            var go = new GameObject("Trampoline");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(2.4f, 0.4f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.WhiteSquare;
            sr.color = new Color(0.3f, 0.7f, 1f);
            sr.sortingOrder = 2;

            var col = go.AddComponent<BoxCollider2D>();
            col.sharedMaterial = new PhysicsMaterial2D("Bouncy") { bounciness = 0.95f, friction = 0.1f };

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            return go.AddComponent<PlanningDraggable>();
        }

        public static PlanningDraggable Block(Vector2 pos)
        {
            var go = new GameObject("Block");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Art/pan");
            sr.sortingOrder = 2;

            var col = go.AddComponent<PolygonCollider2D>();
            col.sharedMaterial = new PhysicsMaterial2D("Block") { bounciness = 0.05f, friction = 0.6f };

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            return go.AddComponent<PlanningDraggable>();
        }
    }
}
