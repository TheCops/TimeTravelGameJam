using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeTravelBanana.Game
{
    public enum GameState
    {
        Planning,
        Playing,
    }

    public class GameStateController : MonoBehaviour
    {
        [SerializeField] private Launcher launcher;
        [SerializeField] private List<PlanningDraggable> draggables = new List<PlanningDraggable>();

        public GameState State { get; private set; } = GameState.Planning;
        public event System.Action<GameState> OnStateChanged;

        public void Configure(Launcher l, IEnumerable<PlanningDraggable> ds)
        {
            launcher = l;
            draggables = ds != null ? new List<PlanningDraggable>(ds) : new List<PlanningDraggable>();
        }

        public void RegisterDraggable(PlanningDraggable d)
        {
            if (d == null || draggables.Contains(d)) return;
            draggables.Add(d);
            d.SetDragEnabled(State == GameState.Planning);
        }

        public void UnregisterDraggable(PlanningDraggable d)
        {
            if (d == null) return;
            draggables.Remove(d);
        }

        public void EnterPlaytest()
        {
            if (State == GameState.Playing) return;
            for (int i = 0; i < draggables.Count; i++) draggables[i].SetDragEnabled(false);
            SetState(GameState.Playing);
        }

        public void EnterPlanning()
        {
            if (State == GameState.Planning) return;
            for (int i = 0; i < draggables.Count; i++) draggables[i].SetDragEnabled(true);
            SetState(GameState.Planning);
        }

        private void Start()
        {
            for (int i = 0; i < draggables.Count; i++) draggables[i].SetDragEnabled(true);
            SetState(GameState.Planning);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.spaceKey.wasPressedThisFrame)
            {
                if (State == GameState.Planning) EnterPlaytest();
                else EnterPlanning();
            }
            else if (State == GameState.Playing && kb.rKey.wasPressedThisFrame)
            {
                EnterPlanning();
            }
        }

        private void SetState(GameState s)
        {
            if (State == s) return;
            State = s;
            OnStateChanged?.Invoke(s);
        }
    }
}
