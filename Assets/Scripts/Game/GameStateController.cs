using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TimeTravelBanana.Timeline;

namespace TimeTravelBanana.Game
{
    public enum GameState
    {
        Planning,
        Baking,
        Playing,
        Resolved
    }

    public class GameStateController : MonoBehaviour
    {
        [SerializeField] private TimelineManager timeline;
        [SerializeField] private Launcher launcher;
        [SerializeField] private ObjectTimeController timeController;
        [SerializeField] private float bakeDuration = 10f;
        [SerializeField] private List<PlanningDraggable> draggables = new List<PlanningDraggable>();

        public GameState State { get; private set; } = GameState.Planning;
        public event System.Action<GameState> OnStateChanged;

        public void Configure(TimelineManager tm, Launcher l, ObjectTimeController c, IEnumerable<PlanningDraggable> ds, float bake = 10f)
        {
            timeline = tm;
            launcher = l;
            timeController = c;
            draggables = new List<PlanningDraggable>(ds);
            bakeDuration = bake;
        }

        private bool subscribedToBanana;

        private void Start()
        {
            SubscribeToBanana();
            EnterPlanning();
        }

        private void OnDisable()
        {
            UnsubscribeFromBanana();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (State == GameState.Planning && kb.spaceKey.wasPressedThisFrame)
            {
                LaunchSequence();
            }
            else if ((State == GameState.Resolved || State == GameState.Playing) && kb.rKey.wasPressedThisFrame)
            {
                EnterPlanning();
            }
        }

        private void LaunchSequence()
        {
            SetState(GameState.Baking);
            for (int i = 0; i < draggables.Count; i++) draggables[i].SetDragEnabled(false);
            timeline.BakeFor(bakeDuration);
            SetState(GameState.Playing);
            timeController.BeginPlayback();
            launcher.Launch();
        }

        private void EnterPlanning()
        {
            timeController?.StopPlayback();
            timeline?.ResetTimeline();
            if (launcher != null && launcher.Banana != null) launcher.Banana.ResetBanana();
            for (int i = 0; i < draggables.Count; i++) draggables[i].SetDragEnabled(true);
            SetState(GameState.Planning);
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

        private void SubscribeToBanana()
        {
            if (subscribedToBanana || launcher == null || launcher.Banana == null) return;
            launcher.Banana.OnWin += HandleWin;
            launcher.Banana.OnLose += HandleLose;
            subscribedToBanana = true;
        }

        private void UnsubscribeFromBanana()
        {
            if (!subscribedToBanana || launcher == null || launcher.Banana == null) return;
            launcher.Banana.OnWin -= HandleWin;
            launcher.Banana.OnLose -= HandleLose;
            subscribedToBanana = false;
        }

        private void SetState(GameState s)
        {
            State = s;
            OnStateChanged?.Invoke(s);
        }
    }
}
