using UnityEngine;

namespace TimeTravelBanana.Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Rocket : MonoBehaviour
    {
        [SerializeField] private float thrust = 20f;
        [SerializeField] private float explosionScale = 1f;
        [SerializeField] private Transform explosionLocation;

        private Rigidbody2D rb;
        private TimelineDestructible destructible;
        private Vector3 placedPosition;
        private Quaternion placedRotation;
        private bool flying;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.useFullKinematicContacts = true;
            destructible = GetComponent<TimelineDestructible>();
            if (GetComponent<Contraption>() == null) gameObject.AddComponent<Contraption>();
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
            flying = true;
        }

        private void HandlePlacing()
        {
            flying = false;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            transform.position = placedPosition;
            transform.rotation = placedRotation;
        }

        private void FixedUpdate()
        {
            if (!flying) return;
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;
            rb.AddForce((Vector2)(transform.up * thrust), ForceMode2D.Force);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!flying) return;
            flying = false;
            Explosion.Spawn(explosionLocation.position, transform.localScale.x * explosionScale);
            
            var target = collision.collider.GetComponentInParent<IDestructible>();
            if (target != null && (Object)target != this && (Object)target != destructible)
            {
                target.DestroyByImpact();
            }

            if (destructible != null) destructible.DestroyByImpact();
        }
    }
}
