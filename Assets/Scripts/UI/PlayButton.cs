using UnityEngine;
using UnityEngine.UI;
using TimeTravelBanana.Game;

namespace TimeTravelBanana.UI
{
    public class PlayButton : MonoBehaviour
    {
        private Button button;
        private Text label;

        private void Awake()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) { Debug.LogWarning("PlayButton: no Canvas in scene; button not built."); return; }
            BuildButton(canvas.transform);
        }

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnPlacingState  += UpdateLabel;
            gm.OnPlayingState  += UpdateLabel;
            gm.OnResolvedState += UpdateLabel;
            gm.OnPausedState   += UpdateLabel;
            UpdateLabel();
        }

        private void OnDestroy()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnPlacingState  -= UpdateLabel;
            gm.OnPlayingState  -= UpdateLabel;
            gm.OnResolvedState -= UpdateLabel;
            gm.OnPausedState   -= UpdateLabel;
        }

        public void Toggle()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (gm.State == GameState.Playing) gm.EnterPlacing();
            else gm.EnterPlaytest();
        }

        private void UpdateLabel()
        {
            if (label == null) return;
            var gm = GameManager.Instance;
            label.text = (gm != null && gm.State == GameState.Playing) ? "STOP" : "PLAY";
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
            label = labelGo.GetComponent<Text>();
            label.text = "PLAY";
            label.alignment = TextAnchor.MiddleCenter;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 32;
            label.color = Color.white;

            button = btnGo.GetComponent<Button>();
            button.onClick.AddListener(Toggle);
        }
    }
}
