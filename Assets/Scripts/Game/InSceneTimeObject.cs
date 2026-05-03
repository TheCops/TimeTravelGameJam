using UnityEngine;

namespace TimeTravelBanana.Game
{
    public enum TimeFlowDirection
    {
        Backwards,
        Forwards,
        Switch,
    }

    [RequireComponent(typeof(Collider2D))]
    public class InSceneTimeObject : MonoBehaviour
    {
        [SerializeField] private TimeFlowDirection timeFlowDirection = TimeFlowDirection.Switch;
        [SerializeField] private float reverseSpeed = -1f;

        private ObjectTimeController timeController;

        private void Start()
        {
            if (GameManager.Instance != null)
                timeController = GameManager.Instance.TimeController;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (timeController == null) return;
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.State != GameState.Playing) return;
            if (other.GetComponentInParent<Banana>() == null) return;

            switch (timeFlowDirection)
            {
                case TimeFlowDirection.Backwards:
                    timeController.BeginReverseScrub(reverseSpeed);
                    break;
                case TimeFlowDirection.Forwards:
                    timeController.ResumeRecording();
                    break;
                case TimeFlowDirection.Switch:
                    if (timeController.IsReverseScrubbing)
                        timeController.ResumeRecording();
                    else
                        timeController.BeginReverseScrub(reverseSpeed);
                    break;
            }
        }
    }
}
