using UnityEngine;


    [RequireComponent(typeof(Rigidbody2D))]
    public class Banana : MonoBehaviour
    {
        [SerializeField] private float maxFlightTime = 8f;

        private Rigidbody2D rb;
        private float launchTime;

        public bool Resolved { get; private set; }

        public event System.Action OnWin;
        public event System.Action OnLose;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Launch()
        {
            
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if( other.GetComponentInParent<Bucket>())
                OnWin?.Invoke();
        }


        private void Update()
        {
            if (Time.time - launchTime > maxFlightTime )
                OnLose?.Invoke();
        }

    }

