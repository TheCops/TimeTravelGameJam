using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    Placing,
    Playing,
    Resolved,
    Paused
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Launcher launcher;

    private TimelineManager timeline;
    private ObjectTimeController timeController;
    private readonly List<PlanningDraggable> draggables = new List<PlanningDraggable>();

    public GameState State { get; private set; } = GameState.Placing;
    public Camera SceneCamera { get; private set; }
    public TimelineManager Timeline => timeline;
    public ObjectTimeController TimeController => timeController;

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

        timeline = gameObject.AddComponent<TimelineManager>();
        timeController = gameObject.AddComponent<ObjectTimeController>();
        timeController.SetTimeline(timeline);

        SceneCamera = Camera.main;
    }

    private void Start()
    {
        EnterPlanning();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (State == GameState.Placing && kb.spaceKey.wasPressedThisFrame)
        {
            LaunchSequence();
        }
        else if ((State == GameState.Resolved || State == GameState.Playing) && kb.rKey.wasPressedThisFrame)
        {
            EnterPlanning();
        }
    }

    public void RegisterDraggable(PlanningDraggable d)
    {
        if (d == null || draggables.Contains(d)) return;
        draggables.Add(d);
    }

    public void UnregisterDraggable(PlanningDraggable d)
    {
        draggables.Remove(d);
    }

    private void LaunchSequence()
    {
        SetState(GameState.Playing);
        timeController.BeginPlayback();
        launcher.Launch();
    }

    private void EnterPlanning()
    {
        timeController?.StopPlayback();
        timeline?.ResetTimeline();
        SetState(GameState.Placing);
    }

    private void HandleWin()
    {
        if (State == GameState.Resolved) return;
        SetState(GameState.Resolved);
        timeController.StopPlayback();
        Debug.Log("Win! Press R to reset.");
    }

    private void HandleLose()
    {
        if (State == GameState.Resolved) return;
        SetState(GameState.Resolved);
        timeController.StopPlayback();
        Debug.Log("Lose. Press R to reset.");
    }

    private void SetState(GameState s)
    {
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
