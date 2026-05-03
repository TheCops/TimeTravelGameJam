using UnityEngine;

namespace TimeTravelBanana.Game
{
    [RequireComponent(typeof(Collider2D))]
    public class ClockObject : MonoBehaviour
    {
        [SerializeField] private float rewindDuration = 5f;
        [SerializeField] private float reverseSpeed = -1f;
        [SerializeField] private bool deactivateAfterUse = true;

        private ObjectTimeController timeController;
        private bool used;

        private void Start()
        {
            if (GameManager.Instance != null)
                timeController = GameManager.Instance.TimeController;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (used) return;
            if (timeController == null) return;
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.State != GameState.Playing) return;
            if (other.GetComponentInParent<Banana>() == null) return;

            timeController.BeginReverseScrub(reverseSpeed);
            used = true;
            if (deactivateAfterUse)
                gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            used = false;
        }
    }
}
