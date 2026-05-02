using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TimeTravelBanana.Game;

namespace TimeTravelBanana.UI
{
    public class ScoreTracker : MonoBehaviour
    {
        private static int sessionTopScore;

        [SerializeField] private float popupDuration = 1.1f;
        [SerializeField] private float popupRise = 110f;
        [SerializeField] private float worldYOffset = 0.7f;
        [SerializeField] private float punchScale = 1.7f;
        [SerializeField] private float punchTime = 0.12f;
        [SerializeField] private float fadeStart = 0.6f;

        private Text scoreLabel;
        private Text topLabel;
        private Canvas canvas;
        private RectTransform canvasRect;
        private RectTransform popupLayer;
        private int score;

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) { Debug.LogWarning("ScoreTracker: no Canvas in scene; score UI not built."); return; }
            canvasRect = canvas.transform as RectTransform;

            scoreLabel = CreateScoreText(canvas.transform, "ScoreText", new Vector2(-20f, -20f), 36, "Score: 0");
            topLabel   = CreateScoreText(canvas.transform, "TopScoreText", new Vector2(-20f, -68f), 28, "Best: 0");
            EnsurePopupLayer();
        }

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm != null) gm.OnPlayingState += HandlePlayingStarted;
            Banana.OnAnyBananaScored += HandleBananaScored;
            Refresh();
        }

        private void OnDestroy()
        {
            var gm = GameManager.Instance;
            if (gm != null) gm.OnPlayingState -= HandlePlayingStarted;
            Banana.OnAnyBananaScored -= HandleBananaScored;
        }

        private void HandlePlayingStarted()
        {
            score = 0;
            Refresh();
        }

        private void HandleBananaScored(int points, Vector3 worldPos)
        {
            score += points;
            if (score > sessionTopScore) sessionTopScore = score;
            Refresh();
            SpawnPopup(points, worldPos);
        }

        private void Refresh()
        {
            if (scoreLabel != null) scoreLabel.text = "Score: " + score;
            if (topLabel != null) topLabel.text = "Best: " + sessionTopScore;
        }

        private static Text CreateScoreText(Transform parent, string name, Vector2 anchoredPos, int fontSize, string initial)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(360f, 48f);
            rt.anchoredPosition = anchoredPos;
            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleRight;
            t.color = Color.white;
            t.text = initial;
            return t;
        }

        private void EnsurePopupLayer()
        {
            if (canvasRect == null) return;
            var existing = canvasRect.Find("PopupLayer") as RectTransform;
            if (existing != null) { popupLayer = existing; popupLayer.SetAsLastSibling(); return; }
            var go = new GameObject("PopupLayer", typeof(RectTransform));
            popupLayer = (RectTransform)go.transform;
            popupLayer.SetParent(canvasRect, false);
            popupLayer.anchorMin = Vector2.zero;
            popupLayer.anchorMax = Vector2.one;
            popupLayer.offsetMin = popupLayer.offsetMax = Vector2.zero;
            popupLayer.SetAsLastSibling();
        }

        private void SpawnPopup(int points, Vector3 worldPos)
        {
            if (popupLayer == null) EnsurePopupLayer();
            if (popupLayer == null || canvas == null || canvasRect == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            Vector3 anchorWorld = worldPos + new Vector3(0f, worldYOffset, 0f);
            Vector3 screen = cam.WorldToScreenPoint(anchorWorld);
            Vector2 local;
            Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, uiCam, out local))
                return;

            var go = new GameObject("ScorePopup", typeof(RectTransform), typeof(Text), typeof(Outline), typeof(Shadow));
            var rt = (RectTransform)go.transform;
            rt.SetParent(popupLayer, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(260f, 90f);
            rt.anchoredPosition = local;
            rt.localScale = Vector3.one * punchScale;

            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.alignment = TextAnchor.MiddleCenter;
            t.text = (points > 0 ? "+" : "") + points;
            t.fontStyle = FontStyle.Bold;
            t.fontSize = points >= 3 ? 64 : (points == 2 ? 52 : 44);
            t.raycastTarget = false;
            t.color = points switch
            {
                3 => new Color(0.45f, 1.0f, 0.55f),
                2 => new Color(1.0f, 0.95f, 0.35f),
                1 => new Color(0.95f, 0.7f, 0.4f),
                _ => new Color(1.0f, 0.35f, 0.35f),
            };

            var outline = go.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
            outline.effectDistance = new Vector2(2.5f, -2.5f);

            var shadow = go.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(0f, -3f);

            StartCoroutine(AnimatePopup(go, rt, t, local));
        }

        private IEnumerator AnimatePopup(GameObject go, RectTransform rt, Text txt, Vector2 startLocal)
        {
            Vector2 endLocal = startLocal + new Vector2(0f, popupRise);
            Color c0 = txt.color;
            float elapsed = 0f;
            while (elapsed < popupDuration)
            {
                elapsed += Time.deltaTime;
                float p = Mathf.Clamp01(elapsed / popupDuration);

                float scaleP = Mathf.Clamp01(elapsed / punchTime);
                float scale = Mathf.Lerp(punchScale, 1f, scaleP);
                rt.localScale = Vector3.one * scale;

                float ease = 1f - (1f - p) * (1f - p);
                rt.anchoredPosition = Vector2.Lerp(startLocal, endLocal, ease);

                float a = p < fadeStart ? 1f : 1f - (p - fadeStart) / (1f - fadeStart);
                Color c = c0; c.a = a;
                txt.color = c;
                yield return null;
            }
            Destroy(go);
        }
    }
}
