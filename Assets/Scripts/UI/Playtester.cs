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
        [SerializeField] private int maxBananasPerRound = 100;

        private Button toggleButton;
        private Text toggleLabel;

        private readonly Queue<Banana> active = new Queue<Banana>();
        private float timer;
        private bool playing;
        private int totalSpawned;

        private void Awake()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) { Debug.LogWarning("Playtester: no Canvas in scene; PLAY button not built."); return; }
            BuildButton(canvas.transform);
        }

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnPlacingState  += HandlePlacing;
                gm.OnPlayingState  += HandlePlaying;
                gm.OnResolvedState += HandlePlacing;
                gm.OnPausedState   += HandlePlacing;
                if (gm.State == GameState.Playing) HandlePlaying(); else HandlePlacing();
            }
        }

        private void OnDestroy()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnPlacingState  -= HandlePlacing;
            gm.OnPlayingState  -= HandlePlaying;
            gm.OnResolvedState -= HandlePlacing;
            gm.OnPausedState   -= HandlePlacing;
        }

        public void Toggle()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (playing) gm.EnterPlanning();
            else gm.EnterPlaytest();
        }

        private void HandlePlacing()
        {
            bool wasPlaying = playing;
            playing = false;
            if (toggleLabel != null) toggleLabel.text = "PLAY";
            if (wasPlaying) StopPlaytest();
        }

        private void HandlePlaying()
        {
            bool wasPlaying = playing;
            playing = true;
            if (toggleLabel != null) toggleLabel.text = "STOP";
            if (!wasPlaying) StartPlaytest();
        }

        private void StartPlaytest()
        {
            timer = 0f;
            totalSpawned = 0;
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

            if (totalSpawned < maxBananasPerRound)
            {
                timer += Time.deltaTime;
                if (timer >= spawnInterval)
                {
                    timer -= spawnInterval;
                    SpawnOne();
                }
            }

            while (active.Count > 0 && active.Peek() == null) active.Dequeue();

            if (totalSpawned >= maxBananasPerRound && active.Count == 0)
            {
                var gm = GameManager.Instance;
                if (gm != null) gm.EnterPlanning();
            }
        }

        private void SpawnOne()
        {
            var gm = GameManager.Instance;
            var launcher = gm != null ? gm.Launcher : null;
            if (launcher == null) return;
            var b = launcher.SpawnAndLaunchInstance(autoDestroyOnResolve: true);
            if (b != null) active.Enqueue(b);
            totalSpawned++;

            while (active.Count > 0 && active.Peek() == null) active.Dequeue();
            while (active.Count > maxActiveBananas)
            {
                var oldest = active.Dequeue();
                if (oldest != null) Destroy(oldest.gameObject);
            }
        }

        private void BuildButton(Transform canvasTransform)
        {
            var btnGo = new GameObject("PlayButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = (RectTransform)btnGo.transform;
            rt.SetParent(canvasTransform, false);
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(220f, 70f);
            rt.anchoredPosition = new Vector2(0f, -20f);
            btnGo.GetComponent<Image>().color = new Color(0.15f, 0.5f, 0.25f, 0.95f);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.SetParent(rt, false);
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = labelRt.offsetMax = Vector2.zero;
            toggleLabel = labelGo.GetComponent<Text>();
            toggleLabel.text = "PLAY";
            toggleLabel.alignment = TextAnchor.MiddleCenter;
            toggleLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            toggleLabel.fontSize = 32;
            toggleLabel.color = Color.white;

            toggleButton = btnGo.GetComponent<Button>();
            toggleButton.onClick.AddListener(Toggle);
        }
    }
}
