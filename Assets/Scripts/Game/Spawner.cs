using UnityEngine;

namespace TimeTravelBanana.Game
{
    public static class Spawner
    {
        public static PlanningDraggable Trampoline(Vector2 pos)
        {
            var go = new GameObject("Trampoline");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Art/trampoline");
            sr.sortingOrder = 2;

            var col = go.AddComponent<PolygonCollider2D>();
            col.sharedMaterial = new PhysicsMaterial2D("Bouncy") { bounciness = 0.95f, friction = 0.1f };

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            return go.AddComponent<PlanningDraggable>();
        }

        public static PlanningDraggable Pan(Vector2 pos)
        {
            var go = new GameObject("Pan");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Art/pan");
            sr.sortingOrder = 2;

            var col = go.AddComponent<PolygonCollider2D>();
            col.sharedMaterial = new PhysicsMaterial2D("Pan") { bounciness = 0.05f, friction = 0.6f };

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            go.AddComponent<Pan>();
            return go.AddComponent<PlanningDraggable>();
        }

        public static PlanningDraggable Rocket(Vector2 pos)
        {
            var go = new GameObject("Rocket");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Art/rocket");
            sr.sortingOrder = 2;

            var col = go.AddComponent<PolygonCollider2D>();
            col.sharedMaterial = new PhysicsMaterial2D("Rocket") { bounciness = 0.05f, friction = 0.6f };

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            go.AddComponent<Rocket>();
            return go.AddComponent<PlanningDraggable>();
        }
    }
}
