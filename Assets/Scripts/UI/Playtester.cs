using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TimeTravelBanana.Game;

namespace TimeTravelBanana.UI
{
    public class Playtester : MonoBehaviour
    {
        [SerializeField] private float spawnInterval = 1f;
        [SerializeField] private int maxActiveBananas = 10;

        private GameStateController gameState;
        private Launcher launcher;
        private Button toggleButton;
        private Text toggleLabel;

        private readonly Queue<Banana> active = new Queue<Banana>();
        private float timer;
        private bool playing;

        public void Configure(GameStateController state, Launcher l, Button btn, Text label)
        {
            gameState = state;
            launcher = l;
            toggleButton = btn;
            toggleLabel = label;
            if (toggleButton != null) toggleButton.onClick.AddListener(Toggle);
            if (gameState != null) gameState.OnStateChanged += HandleStateChanged;
            HandleStateChanged(gameState != null ? gameState.State : GameState.Planning);
        }

        private void OnDestroy()
        {
            if (gameState != null) gameState.OnStateChanged -= HandleStateChanged;
        }

        public void Toggle()
        {
            if (gameState == null) return;
            if (playing) gameState.EnterPlanning();
            else gameState.EnterPlaytest();
        }

        private void HandleStateChanged(GameState s)
        {
            bool wasPlaying = playing;
            playing = (s == GameState.Playing);
            if (toggleLabel != null) toggleLabel.text = playing ? "STOP" : "PLAY";
            if (playing && !wasPlaying) StartPlaytest();
            else if (!playing && wasPlaying) StopPlaytest();
        }

        private void StartPlaytest()
        {
            timer = 0f;
            SpawnOne();
        }

        private void StopPlaytest()
        {
            while (active.Count > 0)
            {
                var b = active.Dequeue();
                if (b != null) Destroy(b.gameObject);
            }
        }

        private void Update()
        {
            if (!playing) return;
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                timer -= spawnInterval;
                SpawnOne();
            }
        }

        private void SpawnOne()
        {
            if (launcher == null) return;
            var b = launcher.SpawnAndLaunchInstance(autoDestroyOnResolve: true);
            if (b != null) active.Enqueue(b);

            while (active.Count > 0 && active.Peek() == null) active.Dequeue();

            while (active.Count > maxActiveBananas)
            {
                var oldest = active.Dequeue();
                if (oldest != null) Destroy(oldest.gameObject);
            }
        }
    }
}
