using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

        private readonly List<PlanningDraggable> draggables = new List<PlanningDraggable>();

        public GameState State { get; private set; } = GameState.Placing;
        public Launcher Launcher => launcher;

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
            ActivateHiddenCanvases();
            EnsureBackground();
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
            switch (s)
            {
                case GameState.Placing:  OnPlacingState?.Invoke();  break;
                case GameState.Playing:  OnPlayingState?.Invoke();  break;
                case GameState.Resolved: OnResolvedState?.Invoke(); break;
                case GameState.Paused:   OnPausedState?.Invoke();   break;
            }
        }
    }
}
