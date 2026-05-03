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

        [SerializeField] private Launcher launcher;
        [SerializeField, Min(1)] private int tickRate = 50;
        [SerializeField, Min(1)] private int maxBufferSeconds = 30;
        [SerializeField] private float defaultRate = 1f;
        [SerializeField] private float fastForwardRate = 2.5f;
        [SerializeField] private float rewindRate = -2f;

        private readonly List<PlanningDraggable> draggables = new List<PlanningDraggable>();

        public GameState State { get; private set; } = GameState.Placing;
        public Launcher Launcher => launcher;
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

            var timelineGo = new GameObject("TimelineManager");
            timelineGo.transform.SetParent(transform, false);
            Timeline = timelineGo.AddComponent<TimelineManager>();
            Timeline.Configure(tickRate, maxBufferSeconds);

            var timeControllerGo = new GameObject("ObjectTimeController");
            timeControllerGo.transform.SetParent(transform, false);
            TimeController = timeControllerGo.AddComponent<ObjectTimeController>();
            TimeController.SetTimeline(Timeline);
            TimeController.SetRates(defaultRate, fastForwardRate, rewindRate);

            ApplyStateToTimeline(State);

            ActivateHiddenCanvases();
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
            EnterPlanning(force: true);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.spaceKey.wasPressedThisFrame)
            {
                if (State == GameState.Placing) EnterPlaytest();
                else if (State == GameState.Playing) EnterPlanning();
            }
            else if (kb.rKey.wasPressedThisFrame && State != GameState.Placing)
            {
                EnterPlanning();
            }
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

        public void EnterPlanning() => EnterPlanning(force: false);

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

        private void EnterPlanning(bool force)
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
        }

        private void ApplyStateToTimeline(GameState s)
        {
            if (Timeline == null) return;
            switch (s)
            {
                case GameState.Placing:
                    Timeline.ResetTimeline();
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
