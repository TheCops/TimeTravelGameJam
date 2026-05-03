using UnityEngine;

namespace TimeTravelBanana.Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Rocket : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;

        private Rigidbody2D rb;
        private Vector3 placedPosition;
        private Quaternion placedRotation;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            placedPosition = transform.position;
            placedRotation = transform.rotation;

            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnPlacingState += HandlePlacing;
            gm.OnPlayingState += HandlePlaying;
        }

        private void OnDestroy()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnPlacingState -= HandlePlacing;
            gm.OnPlayingState -= HandlePlaying;
        }

        private void HandlePlaying()
        {
            placedPosition = transform.position;
            placedRotation = transform.rotation;
            rb.linearVelocity = (Vector2)(transform.up * speed);
        }

        private void HandlePlacing()
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            transform.position = placedPosition;
            transform.rotation = placedRotation;
        }
    }
}
