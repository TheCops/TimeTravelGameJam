using UnityEngine;

namespace TimeTravelBanana.Game
{
    public class windmill : MonoBehaviour
    {
        [SerializeField] private float rotationTorque = 20f;
        [SerializeField] private bool clockwise = true;
        bool rotating;

        private Rigidbody2D rb;
        private TimelineDestructible destructible;
        private Vector3 placedPosition;
        private Quaternion placedRotation;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.useFullKinematicContacts = true;
            destructible = GetComponent<TimelineDestructible>();
            if (GetComponent<Contraption>() == null) gameObject.AddComponent<Contraption>();
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
            rotating = true;
        }

        private void HandlePlacing()
        {
            rotating = false;
            rb.angularVelocity = 0f;
            transform.position = placedPosition;
            transform.rotation = placedRotation;
        }

        private void FixedUpdate()
        {
            if (!rotating) return;
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;
            float torqueForce = rotationTorque;
            if(!clockwise )
                torqueForce *= -1;
            rb.AddTorque(torqueForce);
        }
    }

}
