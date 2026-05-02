using UnityEngine;

namespace TimeTravelBanana.Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Banana : MonoBehaviour
    {
        [SerializeField] private float maxFlightTime = 8f;
        [SerializeField] private float offscreenY = -20f;
        [SerializeField] private float offscreenXAbs = 30f;

        private Rigidbody2D rb;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private float launchTime;
        private bool launched;

        public bool Launched => launched;
        public bool Resolved { get; private set; }

        public event System.Action OnWin;
        public event System.Action OnLose;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            startPosition = transform.position;
            startRotation = transform.rotation;
            FreezeAtRest();
            gameObject.SetActive(false);
        }

        public void Launch(Vector2 spawnPos, Vector2 velocity)
        {
            transform.position = spawnPos;
            transform.rotation = startRotation;
            gameObject.SetActive(true);
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = velocity;
            rb.angularVelocity = 0f;
            launchTime = Time.time;
            launched = true;
            Resolved = false;
        }

        public void ResetBanana()
        {
            launched = false;
            Resolved = false;
            FreezeAtRest();
            transform.position = startPosition;
            transform.rotation = startRotation;
            gameObject.SetActive(false);
        }

        public void HitBucket()
        {
            if (Resolved) return;
            Resolved = true;
            OnWin?.Invoke();
        }

        private void Update()
        {
            if (!launched || Resolved) return;
            Vector3 p = transform.position;
            if (Time.time - launchTime > maxFlightTime || p.y < offscreenY || Mathf.Abs(p.x) > offscreenXAbs)
            {
                Resolved = true;
                OnLose?.Invoke();
            }
        }

        private void FreezeAtRest()
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}
