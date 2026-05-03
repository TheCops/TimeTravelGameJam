using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TimeTravelBanana.Timeline;

namespace TimeTravelBanana.Game
{
    public enum GameState
    {
        Placing,
        Playing,
        Resolved,
        Paused,
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField, Min(1)] private int tickRate = 50;
        [SerializeField, Min(1)] private int maxBufferSeconds = 30;

        private readonly List<PlanningDraggable> draggables = new List<PlanningDraggable>();
        private AudioSource musicSource;

        public GameState State { get; private set; } = GameState.Placing;
        public Launcher Launcher { get; private set; }
        public Camera SceneCamera { get; private set; }
        public TimelineManager Timeline { get; private set; }
        public ObjectTimeController TimeController { get; private set; }

        public event System.Action OnPlacingState;
        public event System.Action OnPlayingState;
        public event System.Action OnResolvedState;
        public event System.Action OnPausedState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            SceneCamera = Camera.main != null
                ? Camera.main
                : UnityEngine.Object.FindFirstObjectByType<Camera>();

            Timeline = gameObject.AddComponent<TimelineManager>();
            Timeline.Configure(tickRate, maxBufferSeconds);

            TimeController = gameObject.AddComponent<ObjectTimeController>();
            TimeController.SetTimeline(Timeline);

            ApplyStateToTimeline(State);

            ActivateHiddenCanvases();
            EnsureBackground();
            SetupMusic();
        }

        private void SetupMusic()
        {
            var clip = Resources.Load<AudioClip>("Audio/banana_rift");
            if (clip == null) return;
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        private static void EnsureBackground()
        {
            if (GameObject.Find("LevelBackground") != null) return;
            var sprite = Resources.Load<Sprite>("Art/background");
            if (sprite == null) return;

            var cam = Camera.main;
            var go = new GameObject("LevelBackground");
            if (cam != null) go.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 10f);
            else go.transform.position = new Vector3(0f, 0f, 10f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -100;

            if (cam != null && cam.orthographic)
            {
                float worldH = cam.orthographicSize * 2f;
                float worldW = worldH * cam.aspect;
                float spriteW = sprite.bounds.size.x;
                float spriteH = sprite.bounds.size.y;
                float scale = Mathf.Max(worldW / spriteW, worldH / spriteH);
                go.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private static void ActivateHiddenCanvases()
        {
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < canvases.Length; i++)
            {
                var go = canvases[i].gameObject;
                if (!go.activeSelf) go.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            EnterPlacing(force: true);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.spaceKey.wasPressedThisFrame)
            {
                if (State == GameState.Placing) EnterPlaytest();
                else if (State == GameState.Playing) EnterPlacing();
            }
            else if (kb.rKey.wasPressedThisFrame && State != GameState.Placing)
            {
                EnterPlacing();
            }
        }

        public void RegisterLauncher(Launcher l)
        {
            if (l == null) return;
            if (Launcher != null && Launcher != l)
                Debug.LogWarning("GameManager: replacing already-registered Launcher.");
            Launcher = l;
        }

        public void UnregisterLauncher(Launcher l)
        {
            if (Launcher == l) Launcher = null;
        }

        public void RegisterDraggable(PlanningDraggable d)
        {
            if (d == null || draggables.Contains(d)) return;
            draggables.Add(d);
            d.SetDragEnabled(State == GameState.Placing);
        }

        public void UnregisterDraggable(PlanningDraggable d)
        {
            if (d == null) return;
            draggables.Remove(d);
        }

        public void EnterPlaytest()
        {
            if (State == GameState.Playing) return;
            for (int i = 0; i < draggables.Count; i++)
                if (draggables[i] != null) draggables[i].SetDragEnabled(false);
            SetState(GameState.Playing);
        }

        public void EnterPlacing() => EnterPlacing(force: false);

        public void EnterResolved()
        {
            if (State == GameState.Resolved) return;
            for (int i = 0; i < draggables.Count; i++)
                if (draggables[i] != null) draggables[i].SetDragEnabled(false);
            SetState(GameState.Resolved);
        }

        public void EnterPaused()
        {
            if (State == GameState.Paused) return;
            SetState(GameState.Paused);
        }

        private void EnterPlacing(bool force)
        {
            if (!force && State == GameState.Placing) return;
            for (int i = 0; i < draggables.Count; i++)
                if (draggables[i] != null) draggables[i].SetDragEnabled(true);
            SetState(GameState.Placing);
        }

        private void SetState(GameState s)
        {
            if (State == s) return;
            State = s;
            ApplyStateToTimeline(s);
            switch (s)
            {
                case GameState.Placing:  OnPlacingState?.Invoke();  break;
                case GameState.Playing:  OnPlayingState?.Invoke();  break;
                case GameState.Resolved: OnResolvedState?.Invoke(); break;
                case GameState.Paused:   OnPausedState?.Invoke();   break;
            }

            if (musicSource != null)
            {
                if (s == GameState.Playing && !musicSource.isPlaying)
                    musicSource.Play();
                else if (s != GameState.Playing)
                    musicSource.Stop();
            }
        }

        private void ApplyStateToTimeline(GameState s)
        {
            if (Timeline == null) return;
            switch (s)
            {
                case GameState.Placing:
                    if (TimeController != null) TimeController.ClearReverseScrub();
                    Timeline.RewindAndClear();
                    break;
                case GameState.Playing:
                    Timeline.SetCurrentTimeWithoutModeChange(0f);
                    Timeline.Mode = TimelineMode.Recording;
                    break;
                case GameState.Paused:
                case GameState.Resolved:
                    Timeline.Mode = TimelineMode.Idle;
                    break;
            }
        }
    }
}
