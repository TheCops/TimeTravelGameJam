using UnityEngine;
using TimeTravelBanana.Timeline;

namespace TimeTravelBanana.Game
{
    public static class SceneBootstrap
    {
        private static void Awake()
        {
            if (Object.FindFirstObjectByType<GameStateController>() != null) return;

            var timelineGo = new GameObject("TimelineManager");
            var timeline = timelineGo.AddComponent<TimelineManager>();

            CreateBucket(new Vector2(8f, -3.0f));

            var banana = CreateBanana(new Vector2(-8f, -2.5f));

            var launcherGo = new GameObject("Launcher");
            launcherGo.transform.position = new Vector3(-8f, -2.5f, 0f);
            var launcher = launcherGo.AddComponent<Launcher>();
            launcher.SetBanana(banana);
            launcher.SetLaunchAngle(60f);
            launcher.SetLaunchSpeed(13f);

            var trampoline = CreateTrampoline(new Vector2(-2f, 1f), timeline);
            var rocket     = CreateRocket(new Vector2(3f, -2f), timeline);

            var timeControlGo = new GameObject("ObjectTimeController");
            var timeControl = timeControlGo.AddComponent<ObjectTimeController>();
            timeControl.SetTimeline(timeline);

            var stateGo = new GameObject("GameStateController");
            var state = stateGo.AddComponent<GameStateController>();
            state.Configure(timeline, launcher, timeControl, new[] { trampoline, rocket }, bake: 10f);
        }

        private static void CreateStaticBox(string name, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.WhiteSquare;
            sr.color = color;
            go.AddComponent<BoxCollider2D>();
        }

        private static void CreateBucket(Vector2 pos)
        {
            var root = new GameObject("Bucket");
            root.transform.position = pos;

            CreateChildBox(root, "Bottom", new Vector3(0f,    0f,   0f), new Vector2(2.0f, 0.3f), Color.cyan);
            CreateChildBox(root, "Left",   new Vector3(-0.85f, 0.7f, 0f), new Vector2(0.3f, 1.4f), Color.cyan);
            CreateChildBox(root, "Right",  new Vector3(0.85f,  0.7f, 0f), new Vector2(0.3f, 1.4f), Color.cyan);

            var trigger = new GameObject("Trigger");
            trigger.transform.SetParent(root.transform, false);
            trigger.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var triggerCol = trigger.AddComponent<BoxCollider2D>();
            triggerCol.size = new Vector2(1.4f, 0.6f);
            triggerCol.isTrigger = true;
            trigger.AddComponent<Bucket>();
        }

        private static void CreateChildBox(GameObject parent, string name, Vector3 localPos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.WhiteSquare;
            sr.color = color;
            go.AddComponent<BoxCollider2D>();
        }

        private static Banana CreateBanana(Vector2 pos)
        {
            var go = new GameObject("Banana");
            go.SetActive(false);
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

            var banana = go.AddComponent<Banana>();
            go.SetActive(true);
            return banana;
        }

        private static PlanningDraggable CreateTrampoline(Vector2 pos, TimelineManager timeline)
        {
            var go = new GameObject("Trampoline");
            go.SetActive(false);
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
            rb.bodyType = RigidbodyType2D.Dynamic;

            var rec = go.AddComponent<RigidbodyTimelineRecorder>();
            rec.Timeline = timeline;

            var drag = go.AddComponent<PlanningDraggable>();
            go.SetActive(true);
            return drag;
        }

        private static PlanningDraggable CreateRocket(Vector2 pos, TimelineManager timeline)
        {
            var go = new GameObject("Rocket");
            go.SetActive(false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.6f, 1.2f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.WhiteSquare;
            sr.color = new Color(1f, 0.4f, 0.3f);
            sr.sortingOrder = 2;

            go.AddComponent<BoxCollider2D>();

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.bodyType = RigidbodyType2D.Dynamic;

            var force = go.AddComponent<ConstantForce2D>();
            force.relativeForce = new Vector2(0f, 12f);

            var rec = go.AddComponent<RigidbodyTimelineRecorder>();
            rec.Timeline = timeline;

            var drag = go.AddComponent<PlanningDraggable>();
            go.SetActive(true);
            return drag;
        }
    }
}
