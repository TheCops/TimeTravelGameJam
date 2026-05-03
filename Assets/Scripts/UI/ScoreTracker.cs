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
            Banana.OnAnyBananaResolved += HandleBananaResolved;
            Refresh();
        }

        private void OnDestroy()
        {
            var gm = GameManager.Instance;
            if (gm != null) gm.OnPlayingState -= HandlePlayingStarted;
            Banana.OnAnyBananaResolved -= HandleBananaResolved;
        }

        private void HandlePlayingStarted()
        {
            score = 0;
            Refresh();
        }

        private void HandleBananaResolved(BananaScoreInfo info)
        {
            score += info.Points;
            if (score > sessionTopScore) sessionTopScore = score;
            Refresh();
            SpawnPopup(info);
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

        private void SpawnPopup(BananaScoreInfo info)
        {
            if (popupLayer == null) EnsurePopupLayer();
            if (popupLayer == null || canvas == null || canvasRect == null) return;
            var gm = GameManager.Instance;
            var cam = gm != null && gm.SceneCamera != null ? gm.SceneCamera : Camera.main;
            if (cam == null) return;

            Vector3 anchorWorld = info.WorldPos + new Vector3(0f, worldYOffset, 0f);
            Vector3 screen = cam.WorldToScreenPoint(anchorWorld);
            Vector2 local;
            Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, uiCam, out local))
                return;

            Vector2 popupSize = new Vector2(420f, 130f);
            local = ClampToCanvas(local, popupSize);

            var go = new GameObject("ScorePopup", typeof(RectTransform), typeof(Text), typeof(Outline), typeof(Shadow));
            var rt = (RectTransform)go.transform;
            rt.SetParent(popupLayer, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = popupSize;
            rt.anchoredPosition = local;
            rt.localScale = Vector3.one * punchScale;

            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = FontStyle.Bold;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 22;
            t.resizeTextMaxSize = info.IsEaten ? 56 : (info.Hits >= 2 ? 64 : (info.Base >= 3 ? 60 : 50));
            t.text = BuildPopupText(info);
            t.color = ColorForInfo(info);

            var outline = go.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
            outline.effectDistance = new Vector2(2.5f, -2.5f);

            var shadow = go.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(0f, -3f);

            StartCoroutine(AnimatePopup(go, rt, t, local));
        }

        private static string BuildPopupText(BananaScoreInfo info)
        {
            if (info.IsEaten) return info.Label + " " + info.Points;
            if (info.IsLoss) return info.Label + " " + info.Points;
            string sign = info.Points >= 0 ? "+" : "";
            string head = info.Label + " " + sign + info.Points;
            if (info.Hits >= 2) return head + "\nx" + (1 + info.Hits) + " chain!";
            if (info.Hits == 1) return head + "\nx2 combo";
            return head;
        }

        private static Color ColorForInfo(BananaScoreInfo info)
        {
            if (info.IsEaten) return new Color(0.85f, 0.55f, 0.30f);
            if (info.IsLoss) return new Color(1.0f, 0.35f, 0.35f);
            if (info.Hits >= 2) return new Color(1.0f, 0.55f, 0.95f);
            if (info.Base == 5) return new Color(1.0f, 0.6f, 0.25f);
            return info.Base switch
            {
                3 => new Color(0.45f, 1.0f, 0.55f),
                2 => new Color(1.0f, 0.95f, 0.35f),
                1 => new Color(0.95f, 0.7f, 0.4f),
                _ => Color.white,
            };
        }

        private Vector2 ClampToCanvas(Vector2 local, Vector2 popupSize)
        {
            if (canvasRect == null) return local;
            Vector2 canvasSize = canvasRect.rect.size;
            float padding = 16f;
            float halfW = popupSize.x * 0.5f + padding;
            float halfH = popupSize.y * 0.5f + padding;
            float minX = -canvasSize.x * 0.5f + halfW;
            float maxX =  canvasSize.x * 0.5f - halfW;
            float minY = -canvasSize.y * 0.5f + halfH;
            float maxY =  canvasSize.y * 0.5f - halfH - popupRise;
            local.x = Mathf.Clamp(local.x, minX, maxX);
            local.y = Mathf.Clamp(local.y, minY, maxY);
            return local;
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
