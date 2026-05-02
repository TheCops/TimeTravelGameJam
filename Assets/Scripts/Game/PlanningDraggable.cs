using UnityEngine;
using UnityEngine.InputSystem;


    [RequireComponent(typeof(Collider2D))]
    public class PlanningDraggable : MonoBehaviour
    {
        [SerializeField] private Camera dragCamera;

        private Collider2D col;
        private bool dragEnabled;
        private bool dragging;
        private Vector3 grabOffset;

        public void SetDragEnabled(bool enabled)
        {
            dragEnabled = enabled;
            if (!enabled) dragging = false;
        }

        public void ReactToPlacingState()  { SetDragEnabled(true);  }
        public void ReactToPlayingState()  { SetDragEnabled(false); }
        public void ReactToResolvedState() { SetDragEnabled(false); }
        public void ReactToPausedState()   { SetDragEnabled(false); }

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            if (dragCamera == null) dragCamera = Camera.main;
        }

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            gm.RegisterDraggable(this);
            gm.OnPlacingState  += ReactToPlacingState;
            gm.OnPlayingState  += ReactToPlayingState;
            gm.OnResolvedState += ReactToResolvedState;
            gm.OnPausedState   += ReactToPausedState;

            switch (gm.State)
            {
                case GameState.Placing:  ReactToPlacingState();  break;
                case GameState.Playing:  ReactToPlayingState();  break;
                case GameState.Resolved: ReactToResolvedState(); break;
                case GameState.Paused:   ReactToPausedState();   break;
            }
        }

        private void OnDestroy()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            gm.OnPlacingState  -= ReactToPlacingState;
            gm.OnPlayingState  -= ReactToPlayingState;
            gm.OnResolvedState -= ReactToResolvedState;
            gm.OnPausedState   -= ReactToPausedState;
            gm.UnregisterDraggable(this);
        }

        private void Update()
        {
            if (!dragEnabled || dragCamera == null) return;
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector3 mouseScreen = mouse.position.ReadValue();
            Vector3 mouseWorld = dragCamera.ScreenToWorldPoint(mouseScreen);
            mouseWorld.z = transform.position.z;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (col.OverlapPoint(mouseWorld))
                {
                    dragging = true;
                    grabOffset = transform.position - mouseWorld;
                }
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                dragging = false;
            }

            if (dragging)
            {
                transform.position = mouseWorld + grabOffset;
            }
        }
    }

