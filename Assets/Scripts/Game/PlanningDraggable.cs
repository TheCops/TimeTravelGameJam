using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeTravelBanana.Game
{
    [RequireComponent(typeof(Collider2D))]
    public class PlanningDraggable : MonoBehaviour
    {
        [SerializeField] private Camera dragCamera;
        [SerializeField] private float snapSize = 0.5f;
        [SerializeField] private float rotationStep = 15f;

        private static PlanningDraggable activeDragger;

        private Collider2D col;
        private Rigidbody2D rb;
        private bool dragEnabled;
        private bool dragging;
        private float scrollAccum;

        public bool IsDragging => dragging;

        public event System.Action<PlanningDraggable> OnRequestDestroy;

        public void SetDragEnabled(bool enabled)
        {
            dragEnabled = enabled;
            if (!enabled) StopDragging();
        }

        private void StopDragging()
        {
            if (dragging && activeDragger == this) activeDragger = null;
            dragging = false;
            scrollAccum = 0f;
        }

        private void StartDragging()
        {
            dragging = true;
            activeDragger = this;
            SetSimulating(false);
        }

        public void SetSimulating(bool simulating)
        {
            if (rb == null) return;
            rb.bodyType = simulating ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
            if (!simulating)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        public void BeginDragFromSpawn()
        {
            if (dragCamera == null) dragCamera = Camera.main;
            dragEnabled = true;
            StartDragging();
            SnapToMouseNow();
        }

        private void OnDestroy()
        {
            if (activeDragger == this) activeDragger = null;
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnPlacingState  -= HandlePlacing;
                gm.OnPlayingState  -= HandlePlayingOrLocked;
                gm.OnResolvedState -= HandlePlayingOrLocked;
                gm.OnPausedState   -= HandlePlayingOrLocked;
                gm.UnregisterDraggable(this);
            }
        }

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            rb = GetComponent<Rigidbody2D>();
            if (dragCamera == null) dragCamera = Camera.main;
        }

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.RegisterDraggable(this);
            gm.OnPlacingState  += HandlePlacing;
            gm.OnPlayingState  += HandlePlayingOrLocked;
            gm.OnResolvedState += HandlePlayingOrLocked;
            gm.OnPausedState   += HandlePlayingOrLocked;
        }

        private void HandlePlacing() => SetDragEnabled(true);
        private void HandlePlayingOrLocked() => SetDragEnabled(false);

        private void Update()
        {
            if (!dragEnabled || dragCamera == null) return;
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector3 mouseWorld = MouseWorld(mouse);
            bool overMe = col.OverlapPoint(mouseWorld);

            if (mouse.rightButton.wasPressedThisFrame)
            {
                bool destroyMe = dragging || (overMe && activeDragger == null);
                if (destroyMe)
                {
                    StopDragging();
                    OnRequestDestroy?.Invoke(this);
                    return;
                }
            }

            if (mouse.leftButton.wasPressedThisFrame && !dragging && overMe && activeDragger == null)
            {
                StartDragging();
            }
            else if (mouse.leftButton.wasReleasedThisFrame && dragging)
            {
                StopDragging();
            }

            if (dragging)
            {
                Vector3 snapped = Snap(mouseWorld);
                snapped.z = transform.position.z;
                transform.position = snapped;

                scrollAccum += mouse.scroll.ReadValue().y;
                while (scrollAccum >= 1f)
                {
                    transform.Rotate(0f, 0f, rotationStep);
                    scrollAccum -= 1f;
                }
                while (scrollAccum <= -1f)
                {
                    transform.Rotate(0f, 0f, -rotationStep);
                    scrollAccum += 1f;
                }
            }
        }

        private void SnapToMouseNow()
        {
            var mouse = Mouse.current;
            if (mouse == null || dragCamera == null) return;
            Vector3 world = MouseWorld(mouse);
            Vector3 snapped = Snap(world);
            snapped.z = transform.position.z;
            transform.position = snapped;
        }

        private Vector3 MouseWorld(Mouse mouse)
        {
            Vector3 screen = mouse.position.ReadValue();
            Vector3 world = dragCamera.ScreenToWorldPoint(screen);
            world.z = transform.position.z;
            return world;
        }

        private Vector3 Snap(Vector3 world)
        {
            if (snapSize <= 0f) return world;
            world.x = Mathf.Round(world.x / snapSize) * snapSize;
            world.y = Mathf.Round(world.y / snapSize) * snapSize;
            return world;
        }
    }
}
