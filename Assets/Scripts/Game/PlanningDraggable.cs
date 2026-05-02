using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeTravelBanana.Game
{
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

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            if (dragCamera == null) dragCamera = Camera.main;
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
}
