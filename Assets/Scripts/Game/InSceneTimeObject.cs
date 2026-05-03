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
    [RequireComponent(typeof(AudioSource))]
    public class InSceneTimeObject : MonoBehaviour
    {
        [SerializeField] private TimeFlowDirection timeFlowDirection = TimeFlowDirection.Switch;
        [SerializeField] private float reverseSpeed = -1f;

        private static AudioClip cachedRewindSound;
        private ObjectTimeController timeController;
        private AudioSource audioSource;
        private bool playingRewindSound;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            if (GameManager.Instance != null)
                timeController = GameManager.Instance.TimeController;
        }

        private void Update()
        {
            if (playingRewindSound && audioSource.isPlaying &&
                timeController != null && !timeController.IsReverseScrubbing)
            {
                audioSource.Stop();
                playingRewindSound = false;
            }
        }

        private void PlayRewindSound()
        {
            if (cachedRewindSound == null) cachedRewindSound = Resources.Load<AudioClip>("Audio/rewind");
            if (cachedRewindSound == null) return;
            audioSource.clip = cachedRewindSound;
            audioSource.Play();
            playingRewindSound = true;
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
                    PlayRewindSound();
                    break;
                case TimeFlowDirection.Forwards:
                    timeController.ResumeRecording();
                    break;
                case TimeFlowDirection.Switch:
                    if (timeController.IsReverseScrubbing)
                        timeController.ResumeRecording();
                    else
                    {
                        timeController.BeginReverseScrub(reverseSpeed);
                        PlayRewindSound();
                    }
                    break;
            }
        }
    }
}
